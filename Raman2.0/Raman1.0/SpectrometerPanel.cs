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

        // 自動積分 soft 上限（避免過長曝光導致 timeout 或 UI 看似卡住）
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

        #region 擷取迴圈（已加入節奏控制，避免過快輪詢造成逾時）

        private void CaptureLoop()
        {
            bool releasedOnError = false;
            var sw = Stopwatch.StartNew();
            long lastUiUpdateMs = 0;

            try
            {
                while (_isCapturing)
                {
                    if (_deviceHandle == IntPtr.Zero) break;
                    if (_intensities == null || SpectrometerInterface.FrameSize <= 0) break;

                    long loopStartMs = sw.ElapsedMilliseconds;
                    uint usedIntegrationUs = ClampU(_integrationTimeUs, _minIntegrationUs, _maxIntegrationUs);

                    float rawMax;
                    float rawMaxWl;
                    string error;

                    bool ok = SpectrometerInterface.AcquireSpectrum(
                        _deviceHandle,
                        usedIntegrationUs,
                        _averageCount,
                        _intensities,
                        out rawMax,
                        out rawMaxWl,
                        out error);

                    if (!ok)
                    {
                        _isCapturing = false;

                        // 釋放 handle，避免裝置停在 busy 狀態
                        try { SpectrometerInterface.Close(_deviceHandle); } catch { }
                        _deviceHandle = IntPtr.Zero;
                        releasedOnError = true;

                        BeginInvoke(new Action(() =>
                        {
                            btnStart.Enabled = false;
                            btnStop.Enabled = false;
                            labelDeviceInfo.Text = "擷取失敗，裝置已釋放。請重新初始化；若仍失敗請拔插 USB。";
                            MessageBox.Show("擷取光譜失敗：\n" + error, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        break;
                    }

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
                                    try
                                    {
                                        uint clamped = ClampU(newUs, _minIntegrationUs, _maxIntegrationUs);
                                        nudIntegrationTimeUs.Value = (decimal)clamped;
                                    }
                                    finally
                                    {
                                        _suppressNudValueChanged = false;
                                    }
                                }));
                            }
                        }
                    }

                    // UI 更新頻率限制（避免 BeginInvoke 排隊）
                    long nowMs = sw.ElapsedMilliseconds;
                    if (nowMs - lastUiUpdateMs >= UI_UPDATE_MIN_INTERVAL_MS)
                    {
                        lastUiUpdateMs = nowMs;

                        float[] intensityCopy = new float[_intensities.Length];
                        Array.Copy(_intensities, intensityCopy, _intensities.Length);

                        BeginInvoke(new Action(() =>
                        {
                            UpdateSpectrumDisplay(intensityCopy, rawMax, rawMaxWl);
                        }));
                    }

                    // 控制抓取節奏，避免過快導致裝置狀態機逾時
                    int desiredPeriodMs = (int)Math.Max(10, (usedIntegrationUs / 1000) + 5);
                    int elapsedThisLoopMs = (int)(sw.ElapsedMilliseconds - loopStartMs);
                    int sleepMs = desiredPeriodMs - elapsedThisLoopMs;
                    if (sleepMs > 0)
                        Thread.Sleep(sleepMs);
                }
            }
            finally
            {
                BeginInvoke(new Action(() =>
                {
                    btnStop.Enabled = false;
                    btnStart.Enabled = (_deviceHandle != IntPtr.Zero) && !releasedOnError;
                }));
                _isCapturing = false;
            }
        }

        private void StopCapture(bool waitForCurrentAcquire)
        {
            _isCapturing = false;

            if (_captureThread != null && _captureThread.IsAlive)
            {
                try
                {
                    if (waitForCurrentAcquire)
                    {
                        uint us = ClampU(_integrationTimeUs, _minIntegrationUs, _maxIntegrationUs);
                        int waitMs = (int)Math.Min(15000, (us / 1000) + 2000);
                        _captureThread.Join(waitMs);
                    }
                    else
                    {
                        _captureThread.Join(200);
                    }
                }
                catch { }
            }

            btnStart.Enabled = (_deviceHandle != IntPtr.Zero);
            btnStop.Enabled = false;
        }

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

        #region 暗背景（沿用目前流程）

        private void btnSaveDarkBackground_Click(object sender, EventArgs e)
        {
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

            uint usedIntegrationUs = ClampU(_integrationTimeUs, _minIntegrationUs, _maxIntegrationUs);

            float[] darkData = new float[SpectrometerInterface.FrameSize];
            string error;
            bool ok = SpectrometerInterface.AcquireSpectrum(
                _deviceHandle, usedIntegrationUs, _averageCount,
                darkData, out _, out _, out error);

            if (!ok)
            {
                MessageBox.Show("擷取暗背景失敗：\n" + error, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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

        #region 繪圖（含 X/Y 軸刻度與單位）

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
                    int left = 60, right = 15, top = 15, bottom = 40;

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
                            g.DrawString($"{wlMin:F0} nm", f, br, x0 - 10, yAxis + 8);
                            g.DrawString($"{wlMid:F0} nm", f, br, x1 - 12, yAxis + 8);
                            g.DrawString($"{wlMax:F0} nm", f, br, x2 - 20, yAxis + 8);
                        }
                    }

                    using (Font f = new Font("Arial", 9))
                    using (Brush br = new SolidBrush(Color.White))
                    using (Pen tickPen = new Pen(Color.White, 1f))
                    {
                        // Y 軸刻度
                        for (int i = 0; i <= 5; i++)
                        {
                            float y = top + plotHeight - i * (plotHeight / 5);
                            g.DrawLine(tickPen, left - 5, y, left, y);
                            float value = globalMax * i / 5f;
                            g.DrawString($"{value:F0}", f, br, left - 55, y - 8);
                        }
                        // 標記最大值位置
                        if (maxWavelength > 0)
                        {
                            g.DrawString($"Peak λ: {maxWavelength:F1} nm", f, br, left + 5, top - 15);
                            g.DrawString($"Intensity: {maxIntensity:F0}", f, br, left + 5, top - 30);
                        }
                    }
                }
            }

            pictureSpectrum.Image = bmp;
        }

        #endregion
    }
}
