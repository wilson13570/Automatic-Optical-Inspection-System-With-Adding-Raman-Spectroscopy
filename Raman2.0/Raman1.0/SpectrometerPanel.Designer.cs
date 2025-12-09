// SpectrometerPanel.Designer.cs (UI 佈局初始化)
namespace Raman1._0
{
    partial class SpectrometerPanel
    {
        // 宣告 UI 控制元件
        private System.Windows.Forms.Button btnInitialize;
        private System.Windows.Forms.Button btnOpenDevice;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Label labelDeviceCountTitle;
        private System.Windows.Forms.Label labelDeviceCountValue;
        private System.Windows.Forms.Label labelSerialTitle;
        private System.Windows.Forms.Label labelSerialValue;
        private System.Windows.Forms.Label labelModelTitle;
        private System.Windows.Forms.Label labelModelValue;
        private System.Windows.Forms.Label labelMaxIntensityTitle;
        private System.Windows.Forms.Label labelMaxIntensityValue;
        private System.Windows.Forms.Label labelMaxWavelengthTitle;
        private System.Windows.Forms.Label labelMaxWavelengthValue;

        private void InitializeComponent()
        {
            this.btnInitialize = new System.Windows.Forms.Button();
            this.btnOpenDevice = new System.Windows.Forms.Button();
            this.btnCapture = new System.Windows.Forms.Button();
            this.labelDeviceCountTitle = new System.Windows.Forms.Label();
            this.labelDeviceCountValue = new System.Windows.Forms.Label();
            this.labelSerialTitle = new System.Windows.Forms.Label();
            this.labelSerialValue = new System.Windows.Forms.Label();
            this.labelModelTitle = new System.Windows.Forms.Label();
            this.labelModelValue = new System.Windows.Forms.Label();
            this.labelMaxIntensityTitle = new System.Windows.Forms.Label();
            this.labelMaxIntensityValue = new System.Windows.Forms.Label();
            this.labelMaxWavelengthTitle = new System.Windows.Forms.Label();
            this.labelMaxWavelengthValue = new System.Windows.Forms.Label();
            // 
            // btnInitialize
            // 
            this.btnInitialize.Location = new System.Drawing.Point(20, 20);
            this.btnInitialize.Name = "btnInitialize";
            this.btnInitialize.Size = new System.Drawing.Size(100, 30);
            this.btnInitialize.Text = "初始化";
            this.btnInitialize.UseVisualStyleBackColor = true;
            this.btnInitialize.Click += new System.EventHandler(this.btnInitialize_Click);
            // 
            // btnOpenDevice
            // 
            this.btnOpenDevice.Location = new System.Drawing.Point(20, 60);
            this.btnOpenDevice.Name = "btnOpenDevice";
            this.btnOpenDevice.Size = new System.Drawing.Size(100, 30);
            this.btnOpenDevice.Text = "開啟裝置";
            this.btnOpenDevice.UseVisualStyleBackColor = true;
            this.btnOpenDevice.Click += new System.EventHandler(this.btnOpenDevice_Click);
            // 
            // btnCapture
            // 
            this.btnCapture.Location = new System.Drawing.Point(20, 100);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(100, 30);
            this.btnCapture.Text = "擷取光譜";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // labelDeviceCountTitle
            // 
            this.labelDeviceCountTitle.AutoSize = true;
            this.labelDeviceCountTitle.Location = new System.Drawing.Point(150, 25);
            this.labelDeviceCountTitle.Name = "labelDeviceCountTitle";
            this.labelDeviceCountTitle.Size = new System.Drawing.Size(89, 18);
            this.labelDeviceCountTitle.Text = "設備數量：";
            // 
            // labelDeviceCountValue
            // 
            this.labelDeviceCountValue.AutoSize = true;
            this.labelDeviceCountValue.Location = new System.Drawing.Point(230, 25);
            this.labelDeviceCountValue.Name = "labelDeviceCountValue";
            this.labelDeviceCountValue.Size = new System.Drawing.Size(16, 18);
            this.labelDeviceCountValue.Text = "0";
            // 
            // labelSerialTitle
            // 
            this.labelSerialTitle.AutoSize = true;
            this.labelSerialTitle.Location = new System.Drawing.Point(150, 65);
            this.labelSerialTitle.Name = "labelSerialTitle";
            this.labelSerialTitle.Size = new System.Drawing.Size(56, 18);
            this.labelSerialTitle.Text = "序號：";
            // 
            // labelSerialValue
            // 
            this.labelSerialValue.AutoSize = true;
            this.labelSerialValue.Location = new System.Drawing.Point(230, 65);
            this.labelSerialValue.Name = "labelSerialValue";
            this.labelSerialValue.Size = new System.Drawing.Size(0, 18);
            this.labelSerialValue.Text = "";
            // 
            // labelModelTitle
            // 
            this.labelModelTitle.AutoSize = true;
            this.labelModelTitle.Location = new System.Drawing.Point(150, 90);
            this.labelModelTitle.Name = "labelModelTitle";
            this.labelModelTitle.Size = new System.Drawing.Size(56, 18);
            this.labelModelTitle.Text = "型號：";
            // 
            // labelModelValue
            // 
            this.labelModelValue.AutoSize = true;
            this.labelModelValue.Location = new System.Drawing.Point(230, 90);
            this.labelModelValue.Name = "labelModelValue";
            this.labelModelValue.Size = new System.Drawing.Size(0, 18);
            this.labelModelValue.Text = "";
            // 
            // labelMaxIntensityTitle
            // 
            this.labelMaxIntensityTitle.AutoSize = true;
            this.labelMaxIntensityTitle.Location = new System.Drawing.Point(150, 145);
            this.labelMaxIntensityTitle.Name = "labelMaxIntensityTitle";
            this.labelMaxIntensityTitle.Size = new System.Drawing.Size(88, 18);
            this.labelMaxIntensityTitle.Text = "最大強度：";
            // 
            // labelMaxIntensityValue
            // 
            this.labelMaxIntensityValue.AutoSize = true;
            this.labelMaxIntensityValue.Location = new System.Drawing.Point(230, 145);
            this.labelMaxIntensityValue.Name = "labelMaxIntensityValue";
            this.labelMaxIntensityValue.Size = new System.Drawing.Size(0, 18);
            this.labelMaxIntensityValue.Text = "";
            // 
            // labelMaxWavelengthTitle
            // 
            this.labelMaxWavelengthTitle.AutoSize = true;
            this.labelMaxWavelengthTitle.Location = new System.Drawing.Point(150, 170);
            this.labelMaxWavelengthTitle.Name = "labelMaxWavelengthTitle";
            this.labelMaxWavelengthTitle.Size = new System.Drawing.Size(56, 18);
            this.labelMaxWavelengthTitle.Text = "波長：";
            // 
            // labelMaxWavelengthValue
            // 
            this.labelMaxWavelengthValue.AutoSize = true;
            this.labelMaxWavelengthValue.Location = new System.Drawing.Point(230, 170);
            this.labelMaxWavelengthValue.Name = "labelMaxWavelengthValue";
            this.labelMaxWavelengthValue.Size = new System.Drawing.Size(0, 18);
            this.labelMaxWavelengthValue.Text = "";
            // 
            // SpectrometerPanel (UserControl 本身)
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnInitialize);
            this.Controls.Add(this.btnOpenDevice);
            this.Controls.Add(this.btnCapture);
            this.Controls.Add(this.labelDeviceCountTitle);
            this.Controls.Add(this.labelDeviceCountValue);
            this.Controls.Add(this.labelSerialTitle);
            this.Controls.Add(this.labelSerialValue);
            this.Controls.Add(this.labelModelTitle);
            this.Controls.Add(this.labelModelValue);
            this.Controls.Add(this.labelMaxIntensityTitle);
            this.Controls.Add(this.labelMaxIntensityValue);
            this.Controls.Add(this.labelMaxWavelengthTitle);
            this.Controls.Add(this.labelMaxWavelengthValue);
            this.Name = "SpectrometerPanel";
            this.Size = new System.Drawing.Size(800, 300);
        }
    }
}
