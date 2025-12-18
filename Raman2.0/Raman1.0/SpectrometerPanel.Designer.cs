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
        private System.Windows.Forms.Button btnSaveDarkBackground;
        private System.Windows.Forms.CheckBox chkSubtractBackground;

        // ===== 新增：自動積分時間控制 UI =====
        private System.Windows.Forms.CheckBox chkAutoIntegration;
        private System.Windows.Forms.Label labelIntegrationTimeTitle;
        private System.Windows.Forms.NumericUpDown nudIntegrationTime;
        private System.Windows.Forms.Label labelCurrentIntegrationTimeTitle;
        private System.Windows.Forms.Label labelCurrentIntegrationTimeValue;
        private System.Windows.Forms.Label labelSaturated;

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
            this.btnSaveDarkBackground = new System.Windows.Forms.Button();
            this.chkSubtractBackground = new System.Windows.Forms.CheckBox();

            this.chkAutoIntegration = new System.Windows.Forms.CheckBox();
            this.labelIntegrationTimeTitle = new System.Windows.Forms.Label();
            this.nudIntegrationTime = new System.Windows.Forms.NumericUpDown();
            this.labelCurrentIntegrationTimeTitle = new System.Windows.Forms.Label();
            this.labelCurrentIntegrationTimeValue = new System.Windows.Forms.Label();
            this.labelSaturated = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.pictureSpectrum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntegrationTime)).BeginInit();
            this.SuspendLayout();
            // 
            // btnInitialize
            // 
            this.btnInitialize.Location = new System.Drawing.Point(20, 24);
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
            this.btnStart.Location = new System.Drawing.Point(140, 24);
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
            this.btnStop.Location = new System.Drawing.Point(260, 24);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 30);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "停止擷取";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnSaveDarkBackground
            // 
            this.btnSaveDarkBackground.Location = new System.Drawing.Point(380, 24);
            this.btnSaveDarkBackground.Name = "btnSaveDarkBackground";
            this.btnSaveDarkBackground.Size = new System.Drawing.Size(130, 30);
            this.btnSaveDarkBackground.TabIndex = 3;
            this.btnSaveDarkBackground.Text = "儲存當前暗背景";
            this.btnSaveDarkBackground.UseVisualStyleBackColor = true;
            this.btnSaveDarkBackground.Click += new System.EventHandler(this.btnSaveDarkBackground_Click);
            // 
            // chkSubtractBackground
            // 
            this.chkSubtractBackground.AutoSize = true;
            this.chkSubtractBackground.Location = new System.Drawing.Point(530, 30);
            this.chkSubtractBackground.Name = "chkSubtractBackground";
            this.chkSubtractBackground.Size = new System.Drawing.Size(90, 19);
            this.chkSubtractBackground.TabIndex = 4;
            this.chkSubtractBackground.Text = "背景扣除";
            this.chkSubtractBackground.UseVisualStyleBackColor = true;
            this.chkSubtractBackground.CheckedChanged += new System.EventHandler(this.chkSubtractBackground_CheckedChanged);

            // 
            // chkAutoIntegration
            // 
            this.chkAutoIntegration.AutoSize = true;
            this.chkAutoIntegration.Location = new System.Drawing.Point(630, 30);
            this.chkAutoIntegration.Name = "chkAutoIntegration";
            this.chkAutoIntegration.Size = new System.Drawing.Size(90, 19);
            this.chkAutoIntegration.TabIndex = 5;
            this.chkAutoIntegration.Text = "自動積分";
            this.chkAutoIntegration.UseVisualStyleBackColor = true;
            this.chkAutoIntegration.CheckedChanged += new System.EventHandler(this.chkAutoIntegration_CheckedChanged);
            // 
            // labelIntegrationTimeTitle
            // 
            this.labelIntegrationTimeTitle.AutoSize = true;
            this.labelIntegrationTimeTitle.Location = new System.Drawing.Point(630, 60);
            this.labelIntegrationTimeTitle.Name = "labelIntegrationTimeTitle";
            this.labelIntegrationTimeTitle.Size = new System.Drawing.Size(91, 15);
            this.labelIntegrationTimeTitle.TabIndex = 6;
            this.labelIntegrationTimeTitle.Text = "積分時間(µs):";
            // 
            // nudIntegrationTime
            // 
            this.nudIntegrationTime.Location = new System.Drawing.Point(720, 56);
            this.nudIntegrationTime.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nudIntegrationTime.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudIntegrationTime.Name = "nudIntegrationTime";
            this.nudIntegrationTime.Size = new System.Drawing.Size(70, 23);
            this.nudIntegrationTime.TabIndex = 7;
            this.nudIntegrationTime.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudIntegrationTime.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudIntegrationTime.ValueChanged += new System.EventHandler(this.nudIntegrationTime_ValueChanged);

            // 
            // labelDeviceInfo
            // 
            this.labelDeviceInfo.AutoSize = true;
            this.labelDeviceInfo.Location = new System.Drawing.Point(20, 60);
            this.labelDeviceInfo.Name = "labelDeviceInfo";
            this.labelDeviceInfo.Size = new System.Drawing.Size(124, 15);
            this.labelDeviceInfo.TabIndex = 8;
            this.labelDeviceInfo.Text = "裝置尚未初始化...";
            // 
            // pictureSpectrum
            // 
            this.pictureSpectrum.BackColor = System.Drawing.Color.Black;
            this.pictureSpectrum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureSpectrum.Location = new System.Drawing.Point(20, 90);
            this.pictureSpectrum.Name = "pictureSpectrum";
            this.pictureSpectrum.Size = new System.Drawing.Size(760, 260);
            this.pictureSpectrum.TabIndex = 9;
            this.pictureSpectrum.TabStop = false;
            // 
            // labelCurrentIntegrationTimeTitle
            // 
            this.labelCurrentIntegrationTimeTitle.AutoSize = true;
            this.labelCurrentIntegrationTimeTitle.Location = new System.Drawing.Point(20, 350);
            this.labelCurrentIntegrationTimeTitle.Name = "labelCurrentIntegrationTimeTitle";
            this.labelCurrentIntegrationTimeTitle.Size = new System.Drawing.Size(91, 15);
            this.labelCurrentIntegrationTimeTitle.TabIndex = 10;
            this.labelCurrentIntegrationTimeTitle.Text = "當前積分時間:";
            // 
            // labelCurrentIntegrationTimeValue
            // 
            this.labelCurrentIntegrationTimeValue.AutoSize = true;
            this.labelCurrentIntegrationTimeValue.Location = new System.Drawing.Point(110, 350);
            this.labelCurrentIntegrationTimeValue.Name = "labelCurrentIntegrationTimeValue";
            this.labelCurrentIntegrationTimeValue.Size = new System.Drawing.Size(22, 15);
            this.labelCurrentIntegrationTimeValue.TabIndex = 11;
            this.labelCurrentIntegrationTimeValue.Text = "---";
            // 
            // labelSaturated
            // 
            this.labelSaturated.AutoSize = true;
            this.labelSaturated.ForeColor = System.Drawing.Color.Red;
            this.labelSaturated.Location = new System.Drawing.Point(330, 350);
            this.labelSaturated.Name = "labelSaturated";
            this.labelSaturated.Size = new System.Drawing.Size(119, 15);
            this.labelSaturated.TabIndex = 12;
            this.labelSaturated.Text = "飽和 (Saturated)";
            this.labelSaturated.Visible = false;
            // 
            // labelMaxIntensityTitle
            // 
            this.labelMaxIntensityTitle.AutoSize = true;
            this.labelMaxIntensityTitle.Location = new System.Drawing.Point(20, 370);
            this.labelMaxIntensityTitle.Name = "labelMaxIntensityTitle";
            this.labelMaxIntensityTitle.Size = new System.Drawing.Size(71, 15);
            this.labelMaxIntensityTitle.TabIndex = 13;
            this.labelMaxIntensityTitle.Text = "最大強度:";
            // 
            // labelMaxIntensityValue
            // 
            this.labelMaxIntensityValue.AutoSize = true;
            this.labelMaxIntensityValue.Location = new System.Drawing.Point(80, 370);
            this.labelMaxIntensityValue.Name = "labelMaxIntensityValue";
            this.labelMaxIntensityValue.Size = new System.Drawing.Size(22, 15);
            this.labelMaxIntensityValue.TabIndex = 14;
            this.labelMaxIntensityValue.Text = "---";
            // 
            // labelMaxWavelengthTitle
            // 
            this.labelMaxWavelengthTitle.AutoSize = true;
            this.labelMaxWavelengthTitle.Location = new System.Drawing.Point(160, 370);
            this.labelMaxWavelengthTitle.Name = "labelMaxWavelengthTitle";
            this.labelMaxWavelengthTitle.Size = new System.Drawing.Size(71, 15);
            this.labelMaxWavelengthTitle.TabIndex = 15;
            this.labelMaxWavelengthTitle.Text = "對應波長:";
            // 
            // labelMaxWavelengthValue
            // 
            this.labelMaxWavelengthValue.AutoSize = true;
            this.labelMaxWavelengthValue.Location = new System.Drawing.Point(220, 370);
            this.labelMaxWavelengthValue.Name = "labelMaxWavelengthValue";
            this.labelMaxWavelengthValue.Size = new System.Drawing.Size(22, 15);
            this.labelMaxWavelengthValue.TabIndex = 16;
            this.labelMaxWavelengthValue.Text = "---";
            // 
            // SpectrometerPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.labelMaxWavelengthValue);
            this.Controls.Add(this.labelMaxWavelengthTitle);
            this.Controls.Add(this.labelMaxIntensityValue);
            this.Controls.Add(this.labelMaxIntensityTitle);
            this.Controls.Add(this.labelSaturated);
            this.Controls.Add(this.labelCurrentIntegrationTimeValue);
            this.Controls.Add(this.labelCurrentIntegrationTimeTitle);
            this.Controls.Add(this.pictureSpectrum);
            this.Controls.Add(this.labelDeviceInfo);
            this.Controls.Add(this.nudIntegrationTime);
            this.Controls.Add(this.labelIntegrationTimeTitle);
            this.Controls.Add(this.chkAutoIntegration);
            this.Controls.Add(this.chkSubtractBackground);
            this.Controls.Add(this.btnSaveDarkBackground);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnInitialize);
            this.Name = "SpectrometerPanel";
            this.Size = new System.Drawing.Size(800, 400);
            this.Disposed += new System.EventHandler(this.SpectrometerPanel_Disposed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureSpectrum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntegrationTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
