namespace Raman1._0
{
    partial class SpectrometerPanel
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnInitialize;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label labelDeviceInfo;
        private System.Windows.Forms.PictureBox pictureSpectrum;
        private System.Windows.Forms.Label labelMaxIntensityTitle;
        private System.Windows.Forms.Label labelMaxIntensityValue;
        private System.Windows.Forms.Label labelMaxWavelengthTitle;
        private System.Windows.Forms.Label labelMaxWavelengthValue;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計程式碼

        private void InitializeComponent()
        {
            this.btnInitialize = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.labelDeviceInfo = new System.Windows.Forms.Label();
            this.pictureSpectrum = new System.Windows.Forms.PictureBox();
            this.labelMaxIntensityTitle = new System.Windows.Forms.Label();
            this.labelMaxIntensityValue = new System.Windows.Forms.Label();
            this.labelMaxWavelengthTitle = new System.Windows.Forms.Label();
            this.labelMaxWavelengthValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSpectrum)).BeginInit();
            this.SuspendLayout();
            // 
            // btnInitialize
            // 
            this.btnInitialize.Location = new System.Drawing.Point(20, 20);
            this.btnInitialize.Name = "btnInitialize";
            this.btnInitialize.Size = new System.Drawing.Size(100, 30);
            this.btnInitialize.TabIndex = 0;
            this.btnInitialize.Text = "初始化裝置";
            this.btnInitialize.UseVisualStyleBackColor = true;
            this.btnInitialize.Click += new System.EventHandler(this.btnInitialize_Click);
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(140, 20);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 30);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "開始擷取";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(260, 20);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 30);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "停止擷取";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // labelDeviceInfo
            // 
            this.labelDeviceInfo.AutoSize = true;
            this.labelDeviceInfo.Location = new System.Drawing.Point(20, 60);
            this.labelDeviceInfo.Name = "labelDeviceInfo";
            this.labelDeviceInfo.Size = new System.Drawing.Size(125, 12);
            this.labelDeviceInfo.TabIndex = 3;
            this.labelDeviceInfo.Text = "裝置尚未初始化...";
            // 
            // pictureSpectrum
            // 
            this.pictureSpectrum.BackColor = System.Drawing.Color.Black;
            this.pictureSpectrum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureSpectrum.Location = new System.Drawing.Point(20, 90);
            this.pictureSpectrum.Name = "pictureSpectrum";
            this.pictureSpectrum.Size = new System.Drawing.Size(760, 260);
            this.pictureSpectrum.TabIndex = 4;
            this.pictureSpectrum.TabStop = false;
            // 
            // labelMaxIntensityTitle
            // 
            this.labelMaxIntensityTitle.AutoSize = true;
            this.labelMaxIntensityTitle.Location = new System.Drawing.Point(20, 370);
            this.labelMaxIntensityTitle.Name = "labelMaxIntensityTitle";
            this.labelMaxIntensityTitle.Size = new System.Drawing.Size(53, 12);
            this.labelMaxIntensityTitle.TabIndex = 5;
            this.labelMaxIntensityTitle.Text = "最大強度:";
            // 
            // labelMaxIntensityValue
            // 
            this.labelMaxIntensityValue.AutoSize = true;
            this.labelMaxIntensityValue.Location = new System.Drawing.Point(80, 370);
            this.labelMaxIntensityValue.Name = "labelMaxIntensityValue";
            this.labelMaxIntensityValue.Size = new System.Drawing.Size(23, 12);
            this.labelMaxIntensityValue.TabIndex = 6;
            this.labelMaxIntensityValue.Text = "---";
            // 
            // labelMaxWavelengthTitle
            // 
            this.labelMaxWavelengthTitle.AutoSize = true;
            this.labelMaxWavelengthTitle.Location = new System.Drawing.Point(160, 370);
            this.labelMaxWavelengthTitle.Name = "labelMaxWavelengthTitle";
            this.labelMaxWavelengthTitle.Size = new System.Drawing.Size(53, 12);
            this.labelMaxWavelengthTitle.TabIndex = 7;
            this.labelMaxWavelengthTitle.Text = "對應波長:";
            // 
            // labelMaxWavelengthValue
            // 
            this.labelMaxWavelengthValue.AutoSize = true;
            this.labelMaxWavelengthValue.Location = new System.Drawing.Point(220, 370);
            this.labelMaxWavelengthValue.Name = "labelMaxWavelengthValue";
            this.labelMaxWavelengthValue.Size = new System.Drawing.Size(23, 12);
            this.labelMaxWavelengthValue.TabIndex = 8;
            this.labelMaxWavelengthValue.Text = "---";
            // 
            // SpectrometerPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.labelMaxWavelengthValue);
            this.Controls.Add(this.labelMaxWavelengthTitle);
            this.Controls.Add(this.labelMaxIntensityValue);
            this.Controls.Add(this.labelMaxIntensityTitle);
            this.Controls.Add(this.pictureSpectrum);
            this.Controls.Add(this.labelDeviceInfo);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnInitialize);
            this.Name = "SpectrometerPanel";
            this.Size = new System.Drawing.Size(800, 400);
            this.Disposed += new System.EventHandler(this.SpectrometerPanel_Disposed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureSpectrum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
