using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Raman1._0
{
    public partial class SpectrometerPanel : UserControl
    {
        private IntPtr _deviceHandle = IntPtr.Zero;

        private float[] _intensities;
        private Thread _captureThread;
        private volatile bool _isCapturing;

        // 擷取參數
        private volatile uint _integrationTimeUs = 100_000; // 100 ms
        private uint _averageCount = 1;

        // integration time 範圍（由裝置讀回）
        private uint _minIntegrationUs = 1_000;      // 保底：1ms
        private uint _maxIntegrationUs = 1_000_000;  // 保底：1s

        // 自動積分 soft 上限（避免過長曝光導致看似卡住）
        private const uint AUTO_SOFT_MAX_US = 2_000_000;

        private bool _suppressNudValueChanged;
        private volatile bool _autoIntegrationEnabled;

        // 自動積分目標區間（16-bit）
        private const float TARGET_LOW = 45000f;
        private const float TARGET_HIGH = 55000f;
        private const float TARGET_MID = (TARGET_LOW + TARGET_HIGH) * 0.5f;

        // UI 更新頻率上限：避免 BeginInvoke 排隊過多（建議 10~20 FPS）
        private const int UI_UPDATE_MIN_INTERVAL_MS = 50; // 20 FPS

        public SpectrometerPanel()
        {
            InitializeComponent();
        }

        #region 初始化 / 開始停止

        private async void btnInitialize_Click(object sender, EventArgs e)
        {
            btnInitialize.Enabled = false;
            try
            {
                if (_isCapturing)
                {
                    MessageBox.Show("請先停止擷取再初始化。");
                    return;
                }

                if (_deviceHandle != IntPtr.Zero)
                {
                    SpectrometerInterface.Close(_deviceHandle);
                    _deviceHandle = IntPtr.Zero;
                }

                labelDeviceInfo.Text = "初始化中...";

                IntPtr handle = IntPtr.Zero;
                string error = string.Empty;

                bool ok = await Task.Run(() =>
                {
                    return SpectrometerInterface.Initialize(out handle, out error);
                });

                if (!ok || handle == IntPtr.Zero)
                {
                    MessageBox.Show("光譜儀初始化失敗：\n" + error, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    labelDeviceInfo.Text = "裝置尚未初始化...";
                    return;
                }

                _deviceHandle = handle;
                _intensities = new float[SpectrometerInterface.FrameSize];

                // 讀取 integration time 範圍（us）
                string rangeErr;
                if (SpectrometerInterface.TryGetIntegrationTimeRangeUs(_deviceHandle, out uint minUs, out uint maxUs, out rangeErr))
                {
                    _minIntegrationUs = minUs;
                    _maxIntegrationUs = maxUs;
                }
                else
                {
                    labelDeviceInfo.Text = "初始化完成，但取得積分範圍失敗：" + rangeErr;
                }

                _integrationTimeUs = ClampU(_integrationTimeUs, _minIntegrationUs, _maxIntegrationUs);

                if (nudIntegrationTimeUs != null)
                {
                    _suppressNudValueChanged = true;
                    try
                    {
                        nudIntegrationTimeUs.Minimum = (decimal)_minIntegrationUs;
                        nudIntegrationTimeUs.Maximum = (decimal)_maxIntegrationUs;
                        nudIntegrationTimeUs.Value = (decimal)_integrationTimeUs;
                    }
                    finally
                    {
                        _suppressNudValueChanged = false;
                    }
                }

                labelDeviceInfo.Text = $"裝置已就緒，點數: {SpectrometerInterface.FrameSize}，積分範圍: {_minIntegrationUs}~{_maxIntegrationUs} µs";
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            finally
            {
                btnInitialize.Enabled = true;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_deviceHandle == IntPtr.Zero)
            {
                MessageBox.Show("請先執行初始化。");
                return;
            }
            if (_isCapturing) return;

            _isCapturing = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            _captureThread = new Thread(CaptureLoop);
            _captureThread.IsBackground = true;
            _captureThread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopCapture(waitForCurrentAcquire: true);
        }

        private void SpectrometerPanel_Disposed(object sender, EventArgs e)
        {
            StopCapture(waitForCurrentAcquire: true);

            if (_deviceHandle != IntPtr.Zero)
            {
                SpectrometerInterface.Close(_deviceHandle);
                _deviceHandle = IntPtr.Zero;
            }
        }

        #endregion

        #region 自動/手動積分 UI

        private void chkAutoIntegration_CheckedChanged(object sender, EventArgs e)
        {
            _autoIntegrationEnabled = (chkAutoIntegration != null && chkAutoIntegration.Checked);
        }

        // Designer 綁定的方法名必須存在
        private void nudIntegrationTimeUs_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressNudValueChanged) return;
            if (nudIntegrationTimeUs == null) return;

            uint v = (uint)nudIntegrationTimeUs.Value;
            v = ClampU(v, _minIntegrationUs, _maxIntegrationUs);
            _integrationTimeUs = v;

            if ((uint)nudIntegrationTimeUs.Value != v)
            {
                _suppressNudValueChanged = true;
                try { nudIntegrationTimeUs.Value = (decimal)v; }
                finally { _suppressNudValueChanged = false; }
            }
        }

        #endregion

        #region 擷取迴圈（TIMEOUT 自動恢復 + 不彈錯誤視窗打斷）

        private void CaptureLoop()
        {
            var sw = Stopwatch.StartNew();
            long lastUiUpdateMs = 0;

            int consecutiveRecoverFail = 0;

            try
            {
                while (_isCapturing)
                {
                    if (_deviceHandle == IntPtr.Zero) break;

                    // 若 framesize 在恢復後有變動，確保 buffer 長度一致
                    if (_intensities == null || _intensities.Length != SpectrometerInterface.FrameSize)
                        _intensities = new float[SpectrometerInterface.FrameSize];

                    long loopStartMs = sw.ElapsedMilliseconds;

                    uint usedIntegrationUs = ClampU(_integrationTimeUs, _minIntegrationUs, _maxIntegrationUs);

                    float rawMax;
                    float rawMaxWl;
                    uint status;
                    string error;

                    // 連續擷取：預設用 Acquire（不清 buffer）
                    bool ok = SpectrometerInterface.AcquireSpectrum(
                        ref _deviceHandle,
                        usedIntegrationUs,
                        _averageCount,
                        _intensities,
                        oneshot: false,
                        out rawMax,
                        out rawMaxWl,
                        out status,
                        out error);

                    if (!ok)
                    {
                        // 可恢復的錯誤：TIMEOUT / PROTOCOL / HANDLE_INVALID
                        if (SpectrometerInterface.IsTimeoutStatus(status) ||
                            SpectrometerInterface.IsProtocolErrorStatus(status) ||
                            SpectrometerInterface.IsHandleInvalidStatus(status))
                        {
                            consecutiveRecoverFail++;

                            BeginInvoke(new Action(() =>
                            {
                                labelDeviceInfo.Text = $"擷取異常：{SpectrometerInterface.FormatStatus(status)}，嘗試自動恢復中...({consecutiveRecoverFail})";
                            }));

                            // 多次仍失敗才停止並提示
                            if (consecutiveRecoverFail >= 3)
                            {
                                _isCapturing = false;

                                BeginInvoke(new Action(() =>
                                {
                                    btnStart.Enabled = (_deviceHandle != IntPtr.Zero);
                                    btnStop.Enabled = false;

                                    MessageBox.Show(
                                        "連續發生逾時/通訊異常，已嘗試自動恢復但仍失敗。\n\n" +
                                        "建議處置：\n" +
                                        "1) 按「初始化裝置」重新連線\n" +
                                        "2) 若仍失敗，拔插 USB 後再初始化\n\n" +
                                        "最後錯誤：\n" + error,
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }));
                                break;
                            }

                            // 稍微緩衝一下再繼續 loop（避免立刻重打）
                            Thread.Sleep(80);
                            continue;
                        }

                        // 非可恢復錯誤：停止並提示（這類錯誤通常是參數/記憶體/校準等）
                        _isCapturing = false;

                        BeginInvoke(new Action(() =>
                        {
                            btnStart.Enabled = (_deviceHandle != IntPtr.Zero);
                            btnStop.Enabled = false;
                            labelDeviceInfo.Text = "擷取失敗（非可恢復錯誤）。請重新初始化；若仍失敗請拔插 USB。";

                            MessageBox.Show("擷取光譜失敗：\n" + error, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        break;
                    }

                    // 成功就把連續失敗計數歸零
                    consecutiveRecoverFail = 0;

                    // 自動積分（用 rawMax 判斷）
                    if (_autoIntegrationEnabled)
                    {
                        uint newUs = ComputeAutoIntegrationUs(usedIntegrationUs, rawMax);
                        if (newUs != usedIntegrationUs)
                        {
                            _integrationTimeUs = newUs;

                            if (nudIntegrationTimeUs != null)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    _suppressNudValueChanged = true;
                                    try { nudIntegrationTimeUs.Value = (decimal)_integrationTimeUs; }
                                    finally { _suppressNudValueChanged = false; }
                                }));
                            }
                        }
                    }

                    long nowMs = sw.ElapsedMilliseconds;
                    if (nowMs - lastUiUpdateMs >= UI_UPDATE_MIN_INTERVAL_MS)
                    {
                        // 複製一份給 UI，避免 UI 繪圖時與下一輪覆寫衝突
                        float[] intensitiesCopy = new float[_intensities.Length];
                        Array.Copy(_intensities, intensitiesCopy, _intensities.Length);

                        uint showUs = _integrationTimeUs;

                        BeginInvoke(new Action(() =>
                        {
                            // 更新圖
                            UpdateSpectrumDisplay(intensitiesCopy, rawMax, rawMaxWl);

                            // 更新數值顯示
                            labelCurrentIntegrationValue.Text = showUs.ToString() + " µs";
                        }));

                        lastUiUpdateMs = nowMs;
                    }

                    // 節奏控制：避免把 SDK 打爆（尤其是很短積分時間時）
                    // 目標：一輪時間 >= integration + 5ms（最少也留 2ms 空檔）
                    long elapsedThisLoop = sw.ElapsedMilliseconds - loopStartMs;
                    int targetMs = (int)(usedIntegrationUs / 1000u) + 5;
                    if (targetMs < 2) targetMs = 2;

                    int sleepMs = targetMs - (int)elapsedThisLoop;
                    if (sleepMs > 0)
                        Thread.Sleep(sleepMs);
                    else
                        Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("擷取執行緒例外：\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
            finally
            {
                BeginInvoke(new Action(() =>
                {
                    btnStart.Enabled = (_deviceHandle != IntPtr.Zero);
                    btnStop.Enabled = false;
                }));
            }
        }

        private void StopCapture(bool waitForCurrentAcquire)
        {
            _isCapturing = false;

            if (_captureThread != null && _captureThread.IsAlive)
            {
                try
                {
                    bool ended = true;

                    if (waitForCurrentAcquire)
                    {
                        uint us = ClampU(_integrationTimeUs, _minIntegrationUs, _maxIntegrationUs);
                        int waitMs = (int)Math.Min(15000, (us / 1000) + 2000);
                        ended = _captureThread.Join(waitMs);
                    }
                    else
                    {
                        ended = _captureThread.Join(200);
                    }

                    // 若執行緒還卡著（多半是 SDK/USB 內部阻塞），嘗試關閉 handle 來中斷
                    if (!ended)
                    {
                        if (_deviceHandle != IntPtr.Zero)
                        {
                            try { SpectrometerInterface.Close(_deviceHandle); } catch { }
                            _deviceHandle = IntPtr.Zero;
                        }

                        _captureThread.Join(2000);
                    }
                }
                catch { }
            }

            btnStart.Enabled = (_deviceHandle != IntPtr.Zero);
            btnStop.Enabled = false;
        }

        #endregion

        #region 自動積分計算（沿用你現有策略）

        private uint ComputeAutoIntegrationUs(uint currentUs, float rawMax)
        {
            if (rawMax < 1f) rawMax = 1f;

            if (rawMax >= TARGET_LOW && rawMax <= TARGET_HIGH)
                return currentUs;

            float ratio;
            if (rawMax > TARGET_HIGH)
            {
                ratio = TARGET_MID / rawMax; // < 1
                ratio = ClampF(ratio, 0.2f, 0.85f);
            }
            else
            {
                ratio = TARGET_MID / rawMax; // > 1
                ratio = ClampF(ratio, 1.15f, 5.0f);
            }

            uint newUs = (uint)Math.Round(currentUs * ratio);

            newUs = ClampU(newUs, _minIntegrationUs, _maxIntegrationUs);

            uint softMax = Math.Min(_maxIntegrationUs, AUTO_SOFT_MAX_US);
            if (newUs > softMax) newUs = softMax;

            uint diff = (newUs > currentUs) ? (newUs - currentUs) : (currentUs - newUs);
            uint thresh = Math.Max(50u, currentUs / 50u);
            if (diff < thresh)
                return currentUs;

            return newUs;
        }

        private static float ClampF(float v, float lo, float hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

        private static uint ClampU(uint v, uint lo, uint hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

        #endregion

        #region 暗背景（沿用你目前可用的流程）

        private void btnSaveDarkBackground_Click(object sender, EventArgs e)
        {
            if (_deviceHandle == IntPtr.Zero || _intensities == null || _intensities.Length == 0)
            {
                MessageBox.Show("請先初始化並開始擷取，才能儲存暗背景。");
                return;
            }

            // 這裡直接取「目前畫面顯示前的最新 raw intensities」較合理
            // 注意：UpdateSpectrumDisplay 已改成不會修改原始資料（避免扣背景後污染 raw）
            float[] darkData = new float[_intensities.Length];
            Array.Copy(_intensities, darkData, _intensities.Length);

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
            if (chkSubtractBackground != null && chkSubtractBackground.Checked)
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

        #region 繪圖（含 X/Y 軸刻度與單位；不再污染 raw intensities）

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

                    int left = 60;
                    int right = 15;
                    int top = 15;
                    int bottom = 40;

                    bool doDarkSubtract =
                        (chkSubtractBackground != null && chkSubtractBackground.Checked) &&
                        (SpectrometerInterface.DarkBackground != null) &&
                        (SpectrometerInterface.DarkBackground.Length == n);

                    float localMax = float.MinValue;
                    int maxIndex = 0;

                    // 先找最大值（用於自動縮放與顯示）
                    if (doDarkSubtract)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float v = intensities[i] - SpectrometerInterface.DarkBackground[i];
                            if (v < 0) v = 0;

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
                            maxWavelength = SpectrometerInterface.Wavelengths[maxIndex];
                        else
                            maxWavelength = 0f;
                    }
                    else
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float v = intensities[i];
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
                            maxWavelength = SpectrometerInterface.Wavelengths[maxIndex];
                        else
                            maxWavelength = 0f;
                    }

                    float globalMax = (maxIntensity > 0) ? maxIntensity : 1f;

                    float plotWidth = (width - left - right);
                    float plotHeight = (height - top - bottom);
                    if (plotWidth <= 1 || plotHeight <= 1) return;

                    float xScale = plotWidth / (n - 1);
                    float yScale = plotHeight / globalMax;

                    PointF prev = PointF.Empty;
                    bool hasPrev = false;

                    using (Pen pen = new Pen(Color.Lime, 1f))
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float v = intensities[i];
                            if (doDarkSubtract)
                            {
                                v = v - SpectrometerInterface.DarkBackground[i];
                                if (v < 0) v = 0;
                            }

                            float x = left + i * xScale;
                            float y = top + (plotHeight - v * yScale);

                            if (y < top) y = top;
                            if (y > top + plotHeight) y = top + plotHeight;

                            PointF pt = new PointF(x, y);
                            if (hasPrev) g.DrawLine(pen, prev, pt);
                            prev = pt;
                            hasPrev = true;
                        }
                    }

                    using (Pen axisPen = new Pen(Color.White, 1f))
                    {
                        g.DrawLine(axisPen, left, top + plotHeight, left + plotWidth, top + plotHeight);
                        g.DrawLine(axisPen, left, top + plotHeight, left, top);
                    }

                    using (Font f = new Font("Arial", 9))
                    using (Brush br = new SolidBrush(Color.White))
                    using (Pen tickPen = new Pen(Color.White, 1f))
                    {
                        float wlMin = 0, wlMid = 0, wlMax = 0;
                        bool hasWL = SpectrometerInterface.Wavelengths != null &&
                                     SpectrometerInterface.Wavelengths.Length == n;

                        if (hasWL)
                        {
                            wlMin = SpectrometerInterface.Wavelengths[0];
                            wlMax = SpectrometerInterface.Wavelengths[n - 1];
                            wlMid = SpectrometerInterface.Wavelengths[(n - 1) / 2];
                        }

                        float x0 = left;
                        float x1 = left + plotWidth * 0.5f;
                        float x2 = left + plotWidth;
                        float yAxis = top + plotHeight;

                        g.DrawLine(tickPen, x0, yAxis, x0, yAxis + 5);
                        g.DrawLine(tickPen, x1, yAxis, x1, yAxis + 5);
                        g.DrawLine(tickPen, x2, yAxis, x2, yAxis + 5);

                        if (hasWL)
                        {
                            g.DrawString(wlMin.ToString("F0"), f, br, x0 - 10, yAxis + 8);
                            g.DrawString(wlMid.ToString("F0"), f, br, x1 - 10, yAxis + 8);
                            g.DrawString(wlMax.ToString("F0"), f, br, x2 - 10, yAxis + 8);
                        }

                        float y0 = top + plotHeight;
                        float y1 = top + plotHeight * 0.5f;
                        float y2 = top;

                        g.DrawLine(tickPen, left - 5, y0, left, y0);
                        g.DrawLine(tickPen, left - 5, y1, left, y1);
                        g.DrawLine(tickPen, left - 5, y2, left, y2);

                        g.DrawString("0", f, br, 5, y0 - 8);
                        g.DrawString((globalMax * 0.5f).ToString("F0"), f, br, 5, y1 - 8);
                        g.DrawString(globalMax.ToString("F0"), f, br, 5, y2 - 8);

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
                pictureSpectrum.Image.Dispose();

            pictureSpectrum.Image = bmp;

            labelMaxIntensityValue.Text = maxIntensity.ToString("F0");
            labelMaxWavelengthValue.Text = maxWavelength.ToString("F1") + " nm";
        }

        #endregion
    }
}
