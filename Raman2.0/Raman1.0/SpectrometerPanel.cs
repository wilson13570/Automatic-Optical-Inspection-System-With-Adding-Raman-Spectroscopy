// SpectrometerPanel.cs
using System;
using System.Text;
using System.Windows.Forms;

namespace Raman1._0
{
    public partial class SpectrometerPanel : UserControl
    {
        private IntPtr deviceHandle = IntPtr.Zero;  // 紀錄目前開啟的光譜儀裝置 Handle

        public SpectrometerPanel()
        {
            InitializeComponent();
        }

        // 「初始化」按鈕點擊事件處理：偵測光譜儀裝置數量
        private void btnInitialize_Click(object sender, EventArgs e)
        {
            uint totalDevices;
            bool ok = SpectrometerInterface.Initialize(out totalDevices);
            if (!ok)
            {
                MessageBox.Show("初始化光譜儀時發生錯誤！", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 顯示找到的設備數量
            labelDeviceCountValue.Text = totalDevices.ToString();
        }

        // 「開啟裝置」按鈕點擊事件處理：開啟第一個光譜儀，並顯示序號與型號
        private void btnOpenDevice_Click(object sender, EventArgs e)
        {
            // 確認至少有一台設備
            if (labelDeviceCountValue.Text == "0" || labelDeviceCountValue.Text == "")
            {
                MessageBox.Show("沒有偵測到光譜儀裝置，請先點擊初始化並確認設備已連接。", "提示",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // 開啟索引0的裝置
            string serial, model;
            bool ok = SpectrometerInterface.OpenDevice(0, out deviceHandle, out serial, out model);
            if (!ok || deviceHandle == IntPtr.Zero)
            {
                MessageBox.Show("開啟光譜儀失敗！", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 顯示裝置序號與型號
            labelSerialValue.Text = serial;
            labelModelValue.Text = model;
        }

        // 「擷取光譜」按鈕點擊事件處理：擷取一次光譜並顯示最大值及波長
        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (deviceHandle == IntPtr.Zero)
            {
                MessageBox.Show("尚未開啟任何光譜儀裝置！", "提示",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            float maxIntensity;
            float maxWavelength;
            bool ok = SpectrometerInterface.CaptureSpectrum(deviceHandle, out maxIntensity, out maxWavelength);
            if (!ok)
            {
                MessageBox.Show("擷取光譜資料失敗！", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 顯示結果：最大強度值及對應波長
            labelMaxIntensityValue.Text = maxIntensity.ToString("F2");
            labelMaxWavelengthValue.Text = maxWavelength.ToString("F2");
        }
    }
}
