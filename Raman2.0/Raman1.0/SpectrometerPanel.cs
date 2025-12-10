using System;
using System.Drawing;
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
    /// </summary>
    public partial class SpectrometerPanel : UserControl
    {
        private IntPtr _deviceHandle = IntPtr.Zero;
        private float[] _intensities;
        private Thread _captureThread;
        private volatile bool _isCapturing;

        // 擷取參數
        private uint _integrationTimeUs = 100_000; // 100 ms
        private uint _averageCount = 1;            // 先用 1 次平均

        public SpectrometerPanel()
        {
            InitializeComponent();
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
                    {
                        break;
                    }

                    float maxIntensity;
                    float maxWavelength;
                    string error;

                    bool ok = SpectrometerInterface.AcquireSpectrum(
                        _deviceHandle,
                        _integrationTimeUs,
                        _averageCount,
                        _intensities,
                        out maxIntensity,
                        out maxWavelength,
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
                        UpdateSpectrumDisplay(intensityCopy, maxIntensity, maxWavelength);
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
                    float globalMax = maxIntensity;
                    if (globalMax <= 0)
                        globalMax = 1;

                    int n = intensities.Length;
                    float xScale = (float)(width - 20) / (n - 1);
                    float yScale = (float)(height - 20) / globalMax;

                    PointF prevPoint = PointF.Empty;
                    bool hasPrev = false;

                    using (Pen pen = new Pen(Color.Lime, 1f))
                    {
                        for (int i = 0; i < n; i++)
                        {
                            float x = 10 + i * xScale;
                            float y = height - 10 - intensities[i] * yScale;
                            if (y < 10) y = 10;
                            if (y > height - 10) y = height - 10;

                            PointF pt = new PointF(x, y);

                            if (hasPrev)
                            {
                                g.DrawLine(pen, prevPoint, pt);
                            }

                            prevPoint = pt;
                            hasPrev = true;
                        }
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
        #endregion
    }
}
