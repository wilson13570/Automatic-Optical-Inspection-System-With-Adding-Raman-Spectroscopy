namespace Raman1._0
{
    partial class SpectrometerPanel
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnInitialize;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;

        private System.Windows.Forms.Button btnSaveDarkBackground;
        private System.Windows.Forms.CheckBox chkSubtractBackground;

        private System.Windows.Forms.CheckBox chkAutoIntegration;
        private System.Windows.Forms.Label labelIntegrationTitle;
        private System.Windows.Forms.NumericUpDown nudIntegrationTimeUs;

        private System.Windows.Forms.Label labelDeviceInfo;

        private System.Windows.Forms.PictureBox pictureSpectrum;

        private System.Windows.Forms.Label labelMaxIntensityTitle;
        private System.Windows.Forms.Label labelMaxIntensityValue;
        private System.Windows.Forms.Label labelMaxWavelengthTitle;
        private System.Windows.Forms.Label labelMaxWavelengthValue;

        private System.Windows.Forms.Label labelCurrentIntegrationTitle;
        private System.Windows.Forms.Label labelCurrentIntegrationValue;

        private System.Windows.Forms.Label labelSaturated;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnInitialize = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();

            this.btnSaveDarkBackground = new System.Windows.Forms.Button();
            this.chkSubtractBackground = new System.Windows.Forms.CheckBox();

            this.chkAutoIntegration = new System.Windows.Forms.CheckBox();
            this.labelIntegrationTitle = new System.Windows.Forms.Label();
            this.nudIntegrationTimeUs = new System.Windows.Forms.NumericUpDown();

            this.labelDeviceInfo = new System.Windows.Forms.Label();

            this.pictureSpectrum = new System.Windows.Forms.PictureBox();

            this.labelMaxIntensityTitle = new System.Windows.Forms.Label();
            this.labelMaxIntensityValue = new System.Windows.Forms.Label();
            this.labelMaxWavelengthTitle = new System.Windows.Forms.Label();
            this.labelMaxWavelengthValue = new System.Windows.Forms.Label();

            this.labelCurrentIntegrationTitle = new System.Windows.Forms.Label();
            this.labelCurrentIntegrationValue = new System.Windows.Forms.Label();

            this.labelSaturated = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.nudIntegrationTimeUs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSpectrum)).BeginInit();
            this.SuspendLayout();

            // btnInitialize
            this.btnInitialize.Location = new System.Drawing.Point(20, 20);
            this.btnInitialize.Name = "btnInitialize";
            this.btnInitialize.Size = new System.Drawing.Size(110, 32);
            this.btnInitialize.TabIndex = 0;
            this.btnInitialize.Text = "初始化裝置";
            this.btnInitialize.UseVisualStyleBackColor = true;
            this.btnInitialize.Click += new System.EventHandler(this.btnInitialize_Click);

            // btnStart
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(140, 20);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(110, 32);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "開始擷取";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            // btnStop
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(260, 20);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(110, 32);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "停止擷取";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            // btnSaveDarkBackground
            this.btnSaveDarkBackground.Location = new System.Drawing.Point(380, 20);
            this.btnSaveDarkBackground.Name = "btnSaveDarkBackground";
            this.btnSaveDarkBackground.Size = new System.Drawing.Size(140, 32);
            this.btnSaveDarkBackground.TabIndex = 3;
            this.btnSaveDarkBackground.Text = "儲存當前暗背景";
            this.btnSaveDarkBackground.UseVisualStyleBackColor = true;
            this.btnSaveDarkBackground.Click += new System.EventHandler(this.btnSaveDarkBackground_Click);

            // chkSubtractBackground
            this.chkSubtractBackground.AutoSize = true;
            this.chkSubtractBackground.Location = new System.Drawing.Point(380, 60);
            this.chkSubtractBackground.Name = "chkSubtractBackground";
            this.chkSubtractBackground.Size = new System.Drawing.Size(90, 19);
            this.chkSubtractBackground.TabIndex = 4;
            this.chkSubtractBackground.Text = "背景扣除";
            this.chkSubtractBackground.UseVisualStyleBackColor = true;
            this.chkSubtractBackground.CheckedChanged += new System.EventHandler(this.chkSubtractBackground_CheckedChanged);

            // chkAutoIntegration
            this.chkAutoIntegration.AutoSize = true;
            this.chkAutoIntegration.Location = new System.Drawing.Point(480, 60);
            this.chkAutoIntegration.Name = "chkAutoIntegration";
            this.chkAutoIntegration.Size = new System.Drawing.Size(90, 19);
            this.chkAutoIntegration.TabIndex = 5;
            this.chkAutoIntegration.Text = "自動積分";
            this.chkAutoIntegration.UseVisualStyleBackColor = true;
            this.chkAutoIntegration.CheckedChanged += new System.EventHandler(this.chkAutoIntegration_CheckedChanged);

            // labelIntegrationTitle
            this.labelIntegrationTitle.AutoSize = true;
            this.labelIntegrationTitle.Location = new System.Drawing.Point(580, 61);
            this.labelIntegrationTitle.Name = "labelIntegrationTitle";
            this.labelIntegrationTitle.Size = new System.Drawing.Size(93, 15);
            this.labelIntegrationTitle.TabIndex = 6;
            this.labelIntegrationTitle.Text = "積分時間(µs):";

            // nudIntegrationTimeUs
            this.nudIntegrationTimeUs.Location = new System.Drawing.Point(680, 58);
            this.nudIntegrationTimeUs.Name = "nudIntegrationTimeUs";
            this.nudIntegrationTimeUs.Size = new System.Drawing.Size(110, 25);
            this.nudIntegrationTimeUs.TabIndex = 7;
            // 先給保守值：初始化後會用 SDK 回報範圍覆寫
            this.nudIntegrationTimeUs.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudIntegrationTimeUs.Maximum = new decimal(new int[] { 2000000, 0, 0, 0 });
            this.nudIntegrationTimeUs.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudIntegrationTimeUs.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            this.nudIntegrationTimeUs.ValueChanged += new System.EventHandler(this.nudIntegrationTimeUs_ValueChanged);

            // labelDeviceInfo
            this.labelDeviceInfo.AutoSize = true;
            this.labelDeviceInfo.Location = new System.Drawing.Point(20, 64);
            this.labelDeviceInfo.Name = "labelDeviceInfo";
            this.labelDeviceInfo.Size = new System.Drawing.Size(124, 15);
            this.labelDeviceInfo.TabIndex = 8;
            this.labelDeviceInfo.Text = "裝置尚未初始化...";

            // pictureSpectrum
            this.pictureSpectrum.BackColor = System.Drawing.Color.Black;
            this.pictureSpectrum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureSpectrum.Location = new System.Drawing.Point(20, 92);
            this.pictureSpectrum.Name = "pictureSpectrum";
            this.pictureSpectrum.Size = new System.Drawing.Size(770, 270);
            this.pictureSpectrum.TabIndex = 9;
            this.pictureSpectrum.TabStop = false;

            // labelMaxIntensityTitle
            this.labelMaxIntensityTitle.AutoSize = true;
            this.labelMaxIntensityTitle.Location = new System.Drawing.Point(20, 375);
            this.labelMaxIntensityTitle.Name = "labelMaxIntensityTitle";
            this.labelMaxIntensityTitle.Size = new System.Drawing.Size(71, 15);
            this.labelMaxIntensityTitle.TabIndex = 10;
            this.labelMaxIntensityTitle.Text = "最大強度:";

            // labelMaxIntensityValue
            this.labelMaxIntensityValue.AutoSize = true;
            this.labelMaxIntensityValue.Location = new System.Drawing.Point(90, 375);
            this.labelMaxIntensityValue.Name = "labelMaxIntensityValue";
            this.labelMaxIntensityValue.Size = new System.Drawing.Size(22, 15);
            this.labelMaxIntensityValue.TabIndex = 11;
            this.labelMaxIntensityValue.Text = "---";

            // labelMaxWavelengthTitle
            this.labelMaxWavelengthTitle.AutoSize = true;
            this.labelMaxWavelengthTitle.Location = new System.Drawing.Point(170, 375);
            this.labelMaxWavelengthTitle.Name = "labelMaxWavelengthTitle";
            this.labelMaxWavelengthTitle.Size = new System.Drawing.Size(71, 15);
            this.labelMaxWavelengthTitle.TabIndex = 12;
            this.labelMaxWavelengthTitle.Text = "對應波長:";

            // labelMaxWavelengthValue
            this.labelMaxWavelengthValue.AutoSize = true;
            this.labelMaxWavelengthValue.Location = new System.Drawing.Point(240, 375);
            this.labelMaxWavelengthValue.Name = "labelMaxWavelengthValue";
            this.labelMaxWavelengthValue.Size = new System.Drawing.Size(22, 15);
            this.labelMaxWavelengthValue.TabIndex = 13;
            this.labelMaxWavelengthValue.Text = "---";

            // labelCurrentIntegrationTitle
            this.labelCurrentIntegrationTitle.AutoSize = true;
            this.labelCurrentIntegrationTitle.Location = new System.Drawing.Point(340, 375);
            this.labelCurrentIntegrationTitle.Name = "labelCurrentIntegrationTitle";
            this.labelCurrentIntegrationTitle.Size = new System.Drawing.Size(86, 15);
            this.labelCurrentIntegrationTitle.TabIndex = 14;
            this.labelCurrentIntegrationTitle.Text = "當前積分時間:";

            // labelCurrentIntegrationValue
            this.labelCurrentIntegrationValue.AutoSize = true;
            this.labelCurrentIntegrationValue.Location = new System.Drawing.Point(425, 375);
            this.labelCurrentIntegrationValue.Name = "labelCurrentIntegrationValue";
            this.labelCurrentIntegrationValue.Size = new System.Drawing.Size(22, 15);
            this.labelCurrentIntegrationValue.TabIndex = 15;
            this.labelCurrentIntegrationValue.Text = "---";

            // labelSaturated
            this.labelSaturated.AutoSize = true;
            this.labelSaturated.ForeColor = System.Drawing.Color.Red;
            this.labelSaturated.Location = new System.Drawing.Point(600, 375);
            this.labelSaturated.Name = "labelSaturated";
            this.labelSaturated.Size = new System.Drawing.Size(70, 15);
            this.labelSaturated.TabIndex = 16;
            this.labelSaturated.Text = "Saturated";
            this.labelSaturated.Visible = false;

            // SpectrometerPanel
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.labelSaturated);
            this.Controls.Add(this.labelCurrentIntegrationValue);
            this.Controls.Add(this.labelCurrentIntegrationTitle);
            this.Controls.Add(this.labelMaxWavelengthValue);
            this.Controls.Add(this.labelMaxWavelengthTitle);
            this.Controls.Add(this.labelMaxIntensityValue);
            this.Controls.Add(this.labelMaxIntensityTitle);
            this.Controls.Add(this.pictureSpectrum);
            this.Controls.Add(this.labelDeviceInfo);
            this.Controls.Add(this.nudIntegrationTimeUs);
            this.Controls.Add(this.labelIntegrationTitle);
            this.Controls.Add(this.chkAutoIntegration);
            this.Controls.Add(this.chkSubtractBackground);
            this.Controls.Add(this.btnSaveDarkBackground);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnInitialize);
            this.Name = "SpectrometerPanel";
            this.Size = new System.Drawing.Size(820, 410);
            this.Disposed += new System.EventHandler(this.SpectrometerPanel_Disposed);

            ((System.ComponentModel.ISupportInitialize)(this.nudIntegrationTimeUs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSpectrum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
