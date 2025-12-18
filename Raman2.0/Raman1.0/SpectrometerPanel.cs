using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Raman1._0
{
    /// <summary>
    /// SD1200-UVN 專用操作面板：
    /// - 初始化 / 開啟第一台光譜儀
    /// - 自動連續擷取光譜直到按下「停止」
    /// - 即時在 PictureBox 畫出最新光譜
    /// - 顯示當前光譜的最大強度與對應波長
    /// - 暗背景扣除（可選）
    /// - 自動積分時間控制（可選）
    /// </summary>
    public partial class SpectrometerPanel : UserControl
    {
        private IntPtr _deviceHandle = IntPtr.Zero;
        private float[] _intensities;
        private Thread _captureThread;
        private volatile bool _isCapturing;

        // 擷取參數（注意：CaptureThread 會讀取這些值）
        private volatile uint _integrationTimeUs = 100_000; // 100 ms
        private volatile uint _averageCount = 1;            // 先用 1 次平均
        private volatile bool _autoIntegrationEnabled = false;

        // UI 更新用（避免 ValueChanged 事件被程式改值時反覆觸發）
        private bool _suppressIntegrationValueChanged = false;

        // 自動積分控制參數
        private const int AUTO_IT_MAX_ITER = 8; // 每次畫面更新最多嘗試調整幾次
        private const float TARGET_LOW_RATIO = 0.60f;
        private const float TARGET_HIGH_RATIO = 0.85f;

        private static readonly string LastIntegrationFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_integration_time_us.txt");

        public SpectrometerPanel()
        {
            InitializeComponent();

            // UI 初始狀態
            _autoIntegrationEnabled = (chkAutoIntegration != null) && chkAutoIntegration.Checked;
            if (nudIntegrationTime != null)
                nudIntegrationTime.Enabled = !_autoIntegrationEnabled;

            // 嘗試載入上次使用的積分時間
            LoadLastIntegrationTime();
            SyncIntegrationUi();
        }

        #region 事件處理
        private void btnInitialize_Click(object sender, EventArgs e)
        {
            if (_deviceHandle != IntPtr.Zero)
            {
                MessageBox.Show("光譜儀已初始化完成。若要重新初始化，請先重啟程式。");
                return;
            }

            string error;
            if (!SpectrometerInterface.Initialize(out _deviceHandle, out error))
            {
                MessageBox.Show("光譜儀初始化失敗：\n" + error, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _deviceHandle = IntPtr.Zero;
                return;
            }

            // 配置強度緩衝陣列
            _intensities = new float[SpectrometerInterface.FrameSize];

            labelDeviceInfo.Text = $"裝置已就緒，點數: {SpectrometerInterface.FrameSize}";
            btnStart.Enabled = true;
            btnStop.Enabled = false;

            // 顯示目前積分時間
            SyncIntegrationUi();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_deviceHandle == IntPtr.Zero)
            {
                MessageBox.Show("請先執行初始化。");
                return;
            }
            if (_isCapturing)
                return;

            _isCapturing = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            _captureThread = new Thread(CaptureLoop);
            _captureThread.IsBackground = true;
            _captureThread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopCapture();
        }

        private void SpectrometerPanel_Disposed(object sender, EventArgs e)
        {
            StopCapture();
            SpectrometerInterface.Close(_deviceHandle);
            _deviceHandle = IntPtr.Zero;

            SaveLastIntegrationTime();
        }

        private void chkAutoIntegration_CheckedChanged(object sender, EventArgs e)
        {
            _autoIntegrationEnabled = chkAutoIntegration.Checked;

            // 勾選自動時：禁用手動輸入（避免人為與自動互相打架）
            if (nudIntegrationTime != null)
                nudIntegrationTime.Enabled = !_autoIntegrationEnabled;

            // 切回手動時，立刻用目前欄位的值作為積分時間（若欄位存在）
            if (!_autoIntegrationEnabled && nudIntegrationTime != null)
            {
                _integrationTimeUs = (uint)nudIntegrationTime.Value;
            }

            SyncIntegrationUi();
        }

        private void nudIntegrationTime_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressIntegrationValueChanged)
                return;

            // 只有在「自動積分」關閉時才採用手動值
            if (!_autoIntegrationEnabled)
            {
                _integrationTimeUs = (uint)nudIntegrationTime.Value;
                SyncIntegrationUi();
            }
        }
        #endregion

        #region 擷取與繪圖
        private void CaptureLoop()
        {
            try
            {
                while (_isCapturing)
                {
                    if (_intensities == null || SpectrometerInterface.FrameSize <= 0)
                        break;

                    float rawMaxIntensity;
                    float rawMaxWavelength;
                    bool saturated;
                    uint usedIntegrationUs;
                    string error;

                    bool ok = AcquireSpectrumWithAutoIntegration(
                        _intensities,
                        out rawMaxIntensity,
                        out rawMaxWavelength,
                        out saturated,
                        out usedIntegrationUs,
                        out error);

                    if (!ok)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("擷取光譜失敗：\n" + error, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        break;
                    }

                    // 為避免 UI 與背景執行緒同時使用同一個陣列，複製一份
                    float[] intensityCopy = new float[_intensities.Length];
                    Array.Copy(_intensities, intensityCopy, _intensities.Length);

                    this.BeginInvoke(new Action(() =>
                    {
                        // 顯示光譜（若有背景扣除，會在 UpdateSpectrumDisplay 內處理）
                        UpdateSpectrumDisplay(intensityCopy, rawMaxIntensity, rawMaxWavelength);

                        // 更新積分時間顯示
                        UpdateIntegrationStatusUi(usedIntegrationUs, saturated);
                    }));

                    // 避免 CPU 過高，可視情況調整
                    Thread.Sleep(10);
                }
            }
            finally
            {
                this.BeginInvoke(new Action(() =>
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = false;
                }));
                _isCapturing = false;
            }
        }

        private void StopCapture()
        {
            _isCapturing = false;
            if (_captureThread != null && _captureThread.IsAlive)
            {
                try
                {
                    _captureThread.Join(500);
                }
                catch { }
            }
            btnStart.Enabled = true;
            btnStop.Enabled = false;

            SaveLastIntegrationTime();
        }

        /// <summary>
        /// 核心：先擷取 raw 光譜，再依照 raw 最大值做「自動積分時間控制」。
        /// 注意：自動判斷一定用 raw（未扣背景）避免誤判。
        /// </summary>
        private bool AcquireSpectrumWithAutoIntegration(
            float[] buffer,
            out float rawMaxIntensity,
            out float rawMaxWavelength,
            out bool saturated,
            out uint usedIntegrationTimeUs,
            out string error)
        {
            rawMaxIntensity = 0f;
            rawMaxWavelength = 0f;
            saturated = false;
            usedIntegrationTimeUs = _integrationTimeUs;
            error = string.Empty;

            if (_deviceHandle == IntPtr.Zero)
            {
                error = "裝置尚未初始化。";
                return false;
            }

            uint minUs = SpectrometerInterface.IntegrationTimeMinUs;
            uint maxUs = SpectrometerInterface.IntegrationTimeMaxUs;

            float adcMax = SpectrometerInterface.AdcMaxValue;
            float targetLow = adcMax * TARGET_LOW_RATIO;
            float targetHigh = adcMax * TARGET_HIGH_RATIO;

            // 使用當前積分時間作為起始值
            uint it = usedIntegrationTimeUs;

            for (int iter = 0; iter < AUTO_IT_MAX_ITER; iter++)
            {
                bool ok = SpectrometerInterface.AcquireSpectrum(
                    _deviceHandle,
                    it,
                    _averageCount,
                    buffer,
                    out rawMaxIntensity,
                    out rawMaxWavelength,
                    out error);

                if (!ok)
                    return false;

                saturated = (rawMaxIntensity >= adcMax);

                // 若未啟用自動積分，擷取一次就結束
                if (!_autoIntegrationEnabled)
                    break;

                // 需要縮短（太亮或飽和）
                if (rawMaxIntensity > targetHigh && it > minUs)
                {
                    uint candidate = (uint)Math.Round(it * 0.5);
                    if (candidate < minUs) candidate = minUs;
                    uint newIt = candidate;

                    if (newIt == it)
                        break;

                    it = newIt;
                    continue;
                }

                // 需要拉長（太暗）
                if (rawMaxIntensity < targetLow && it < maxUs)
                {
                    uint candidate = (uint)Math.Round(it * 1.5);
                    if (candidate > maxUs) candidate = maxUs;
                    uint newIt = candidate;

                    if (newIt == it)
                        break;

                    it = newIt;
                    continue;
                }

                // 在區間內，結束
                break;
            }

            usedIntegrationTimeUs = it;
            _integrationTimeUs = it; // 更新全域狀態，讓下一輪從此值開始

            return true;
        }

        private void UpdateIntegrationStatusUi(uint integrationTimeUs, bool saturated)
        {
            // 顯示當前積分時間（同時把 NumericUpDown 同步到實際值，方便你切回手動時接續）
            if (labelCurrentIntegrationTimeValue != null)
            {
                double ms = integrationTimeUs / 1000.0;
                labelCurrentIntegrationTimeValue.Text = $"{integrationTimeUs} µs ({ms:F1} ms)";
            }

            if (labelSaturated != null)
                labelSaturated.Visible = saturated;

            // 若目前是自動模式，nudIntegrationTime 被禁用，但仍可同步顯示最新值
            if (nudIntegrationTime != null)
            {
                try
                {
                    _suppressIntegrationValueChanged = true;

                    decimal v = integrationTimeUs;
                    if (v < nudIntegrationTime.Minimum) v = nudIntegrationTime.Minimum;
                    if (v > nudIntegrationTime.Maximum) v = nudIntegrationTime.Maximum;
                    nudIntegrationTime.Value = v;
                }
                finally
                {
                    _suppressIntegrationValueChanged = false;
                }
            }
        }

        private void SyncIntegrationUi()
        {
            if (labelCurrentIntegrationTimeValue != null)
            {
                double ms = _integrationTimeUs / 1000.0;
                labelCurrentIntegrationTimeValue.Text = $"{_integrationTimeUs} µs ({ms:F1} ms)";
            }

            if (labelSaturated != null)
                labelSaturated.Visible = false;

            if (nudIntegrationTime != null)
            {
                try
                {
                    _suppressIntegrationValueChanged = true;

                    decimal v = _integrationTimeUs;
                    if (v < nudIntegrationTime.Minimum) v = nudIntegrationTime.Minimum;
                    if (v > nudIntegrationTime.Maximum) v = nudIntegrationTime.Maximum;
                    nudIntegrationTime.Value = v;
                }
                finally
                {
                    _suppressIntegrationValueChanged = false;
                }
            }
        }

        private void LoadLastIntegrationTime()
        {
            try
            {
                if (!File.Exists(LastIntegrationFilePath))
                    return;

                string s = File.ReadAllText(LastIntegrationFilePath).Trim();
                uint last;
                if (uint.TryParse(s, out last))
                {
                    // 夾制在範圍內
                    if (last < SpectrometerInterface.IntegrationTimeMinUs) last = SpectrometerInterface.IntegrationTimeMinUs;
                    if (last > SpectrometerInterface.IntegrationTimeMaxUs) last = SpectrometerInterface.IntegrationTimeMaxUs;
                    _integrationTimeUs = last;
                }
            }
            catch
            {
                // 讀取失敗不影響主流程
            }
        }

        private void SaveLastIntegrationTime()
        {
            try
            {
                File.WriteAllText(LastIntegrationFilePath, _integrationTimeUs.ToString());
            }
            catch
            {
                // 儲存失敗不影響主流程
            }
        }

        /// <summary>
        /// 在 PictureBox 中繪製光譜並更新最大值資訊
        /// </summary>
        private void UpdateSpectrumDisplay(float[] intensities, float maxIntensity, float maxWavelength)
        {
            if (pictureSpectrum.Width <= 0 || pictureSpectrum.Height <= 0)
                return;

            int width = pictureSpectrum.Width;
            int height = pictureSpectrum.Height;

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);

                if (intensities != null && intensities.Length > 1)
                {
                    int n = intensities.Length;

                    // 留出顯示刻度文字的邊界
                    int left = 60;
                    int right = 15;
                    int top = 15;
                    int bottom = 40;

                    // ===== 1) 暗背景扣除（若勾選且背景存在且長度相符）=====
                    bool doDarkSubtract =
                        (chkSubtractBackground != null && chkSubtractBackground.Checked) &&
                        (SpectrometerInterface.DarkBackground != null) &&
                        (SpectrometerInterface.DarkBackground.Length == n);

                    float localMax = float.MinValue;
                    int maxIndex = 0;

                    if (doDarkSubtract)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float v = intensities[i] - SpectrometerInterface.DarkBackground[i];
                            if (v < 0) v = 0;
                            intensities[i] = v;

                            if (v > localMax)
                            {
                                localMax = v;
                                maxIndex = i;
                            }
                        }

                        if (localMax < 0) localMax = 0;
                        maxIntensity = localMax;
                        if (SpectrometerInterface.Wavelengths != null &&
                            SpectrometerInterface.Wavelengths.Length > maxIndex)
                        {
                            maxWavelength = SpectrometerInterface.Wavelengths[maxIndex];
                        }
                        else
                        {
                            maxWavelength = 0f;
                        }
                    }

                    // ===== 2) 計算繪圖比例 =====
                    float globalMax = (maxIntensity > 0) ? maxIntensity : 1f;

                    float plotWidth = (width - left - right);
                    float plotHeight = (height - top - bottom);
                    if (plotWidth <= 1 || plotHeight <= 1) return;

                    float xScale = plotWidth / (n - 1);
                    float yScale = plotHeight / globalMax;

                    // ===== 3) 畫光譜曲線 =====
                    PointF prev = PointF.Empty;
                    bool hasPrev = false;

                    using (Pen pen = new Pen(Color.Lime, 1f))
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float x = left + i * xScale;
                            float y = top + (plotHeight - intensities[i] * yScale);

                            if (y < top) y = top;
                            if (y > top + plotHeight) y = top + plotHeight;

                            PointF pt = new PointF(x, y);
                            if (hasPrev) g.DrawLine(pen, prev, pt);
                            prev = pt;
                            hasPrev = true;
                        }
                    }

                    // ===== 4) 畫座標軸 =====
                    using (Pen axisPen = new Pen(Color.White, 1f))
                    {
                        // X 軸
                        g.DrawLine(axisPen, left, top + plotHeight, left + plotWidth, top + plotHeight);
                        // Y 軸
                        g.DrawLine(axisPen, left, top + plotHeight, left, top);
                    }

                    // ===== 5) 刻度 + 單位標註 =====
                    using (Font f = new Font("Arial", 9))
                    using (Brush br = new SolidBrush(Color.White))
                    using (Pen tickPen = new Pen(Color.White, 1f))
                    {
                        // X 軸：用 Wavelengths 標 min/mid/max（若可用）
                        float wlMin = 0, wlMid = 0, wlMax = 0;
                        bool hasWL = SpectrometerInterface.Wavelengths != null &&
                                     SpectrometerInterface.Wavelengths.Length == n;

                        if (hasWL)
                        {
                            wlMin = SpectrometerInterface.Wavelengths[0];
                            wlMax = SpectrometerInterface.Wavelengths[n - 1];
                            wlMid = SpectrometerInterface.Wavelengths[(n - 1) / 2];
                        }

                        // X ticks at left/middle/right
                        float x0 = left;
                        float x1 = left + plotWidth * 0.5f;
                        float x2 = left + plotWidth;

                        float yAxis = top + plotHeight;

                        // tick lines
                        g.DrawLine(tickPen, x0, yAxis, x0, yAxis + 5);
                        g.DrawLine(tickPen, x1, yAxis, x1, yAxis + 5);
                        g.DrawLine(tickPen, x2, yAxis, x2, yAxis + 5);

                        if (hasWL)
                        {
                            g.DrawString(wlMin.ToString("F0"), f, br, x0 - 10, yAxis + 8);
                            g.DrawString(wlMid.ToString("F0"), f, br, x1 - 10, yAxis + 8);
                            g.DrawString(wlMax.ToString("F0"), f, br, x2 - 10, yAxis + 8);
                        }

                        // Y 軸：0 / max/2 / max
                        float y0 = top + plotHeight;
                        float y1 = top + plotHeight * 0.5f;
                        float y2 = top;

                        g.DrawLine(tickPen, left - 5, y0, left, y0);
                        g.DrawLine(tickPen, left - 5, y1, left, y1);
                        g.DrawLine(tickPen, left - 5, y2, left, y2);

                        g.DrawString("0", f, br, 5, y0 - 8);
                        g.DrawString((globalMax * 0.5f).ToString("F0"), f, br, 5, y1 - 8);
                        g.DrawString(globalMax.ToString("F0"), f, br, 5, y2 - 8);

                        // Axis labels
                        string xLabel = "波長 (nm)";
                        SizeF xSize = g.MeasureString(xLabel, f);
                        g.DrawString(xLabel, f, br, left + plotWidth / 2f - xSize.Width / 2f, height - 20);

                        string yLabel = "強度 (a.u.)";
                        var state = g.Save();
                        g.TranslateTransform(18f, top + plotHeight / 2f);
                        g.RotateTransform(-90f);
                        SizeF ySize = g.MeasureString(yLabel, f);
                        g.DrawString(yLabel, f, br, -ySize.Width / 2f, -ySize.Height / 2f);
                        g.Restore(state);
                    }
                }
            }

            if (pictureSpectrum.Image != null)
            {
                pictureSpectrum.Image.Dispose();
            }
            pictureSpectrum.Image = bmp;

            labelMaxIntensityValue.Text = maxIntensity.ToString("F0");
            labelMaxWavelengthValue.Text = maxWavelength.ToString("F1") + " nm";
        }

        private void btnSaveDarkBackground_Click(object sender, EventArgs e)
        {
            // 確認裝置已初始化且未在擷取中
            if (_deviceHandle == IntPtr.Zero)
            {
                MessageBox.Show("請先執行初始化。");
                return;
            }
            if (_isCapturing)
            {
                MessageBox.Show("請先停止擷取再儲存暗背景。");
                return;
            }

            // 從光譜儀擷取一次光譜作為暗背景
            float[] darkData = new float[SpectrometerInterface.FrameSize];
            string error;
            bool ok = SpectrometerInterface.AcquireSpectrum(
                         _deviceHandle, _integrationTimeUs, _averageCount,
                         darkData, out _, out _, out error);
            if (!ok)
            {
                MessageBox.Show("擷取暗背景失敗：\n" + error, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 將擷取到的暗背景光譜儲存到記憶體並寫入檔案
            string setErr;
            if (!SpectrometerInterface.SetDarkBackground(darkData, out setErr))
            {
                MessageBox.Show("設定暗背景失敗：\n" + setErr);
                return;
            }

            string saveErr;
            if (!SpectrometerInterface.SaveDarkBackgroundToFile(out saveErr))
            {
                MessageBox.Show("暗背景已設定，但儲存檔案失敗：\n" + saveErr);
                return;
            }

            MessageBox.Show("暗背景已儲存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void chkSubtractBackground_CheckedChanged(object sender, EventArgs e)
        {
            // 啟用背景扣除時，若尚未設定暗背景則給予提醒
            if (chkSubtractBackground.Checked)
            {
                if (SpectrometerInterface.DarkBackground == null)
                {
                    MessageBox.Show("尚未設定暗背景，無法執行背景扣除！", "提醒",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    chkSubtractBackground.Checked = false;
                }
            }
        }

        #endregion
    }
}
