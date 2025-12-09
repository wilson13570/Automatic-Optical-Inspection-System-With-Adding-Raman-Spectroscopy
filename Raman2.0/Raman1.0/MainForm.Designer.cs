namespace Raman1._0
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToJGPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aUTOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mANUALToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.lOGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.sETToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSerialPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAuto = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelRunTimeV = new System.Windows.Forms.Label();
            this.labelStateV = new System.Windows.Forms.Label();
            this.sizeComboBox = new System.Windows.Forms.ComboBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelRunTime = new System.Windows.Forms.Label();
            this.labelState = new System.Windows.Forms.Label();
            this.picCamera = new System.Windows.Forms.PictureBox();
            this.btnAutoCal = new System.Windows.Forms.Button();
            this.btnAutoFocus = new System.Windows.Forms.Button();
            this.btnAutoScan = new System.Windows.Forms.Button();
            this.panelManual = new System.Windows.Forms.Panel();
            this.btnB = new System.Windows.Forms.Button();
            this.btnL = new System.Windows.Forms.Button();
            this.btnF = new System.Windows.Forms.Button();
            this.btnR = new System.Windows.Forms.Button();
            this.btnShot = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnBackward = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnForward = new System.Windows.Forms.Button();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.panelLog = new System.Windows.Forms.Panel();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.tableLayoutPanelPreview = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            this.panelAuto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
            this.panelManual.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.tableLayoutPanelPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItem1,
            this.aUTOToolStripMenuItem,
            this.toolStripMenuItem2,
            this.mANUALToolStripMenuItem,
            this.toolStripMenuItem3,
            this.lOGToolStripMenuItem,
            this.toolStripMenuItem4,
            this.sETToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(2539, 31);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLogToolStripMenuItem,
            this.saveToJGPToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(56, 27);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveLogToolStripMenuItem
            // 
            this.saveLogToolStripMenuItem.Name = "saveLogToolStripMenuItem";
            this.saveLogToolStripMenuItem.Size = new System.Drawing.Size(192, 26);
            this.saveLogToolStripMenuItem.Text = "Save Log";
            // 
            // saveToJGPToolStripMenuItem
            // 
            this.saveToJGPToolStripMenuItem.Name = "saveToJGPToolStripMenuItem";
            this.saveToJGPToolStripMenuItem.Size = new System.Drawing.Size(192, 26);
            this.saveToJGPToolStripMenuItem.Text = "SaveTo JPG";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(27, 27);
            this.toolStripMenuItem1.Text = "|";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // aUTOToolStripMenuItem
            // 
            this.aUTOToolStripMenuItem.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aUTOToolStripMenuItem.Name = "aUTOToolStripMenuItem";
            this.aUTOToolStripMenuItem.Size = new System.Drawing.Size(62, 27);
            this.aUTOToolStripMenuItem.Text = "Auto";
            this.aUTOToolStripMenuItem.Click += new System.EventHandler(this.aUTOToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Enabled = false;
            this.toolStripMenuItem2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(27, 27);
            this.toolStripMenuItem2.Text = "|";
            // 
            // mANUALToolStripMenuItem
            // 
            this.mANUALToolStripMenuItem.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mANUALToolStripMenuItem.Name = "mANUALToolStripMenuItem";
            this.mANUALToolStripMenuItem.Size = new System.Drawing.Size(83, 27);
            this.mANUALToolStripMenuItem.Text = "Manual";
            this.mANUALToolStripMenuItem.Click += new System.EventHandler(this.mANUALToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Enabled = false;
            this.toolStripMenuItem3.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(27, 27);
            this.toolStripMenuItem3.Text = "|";
            // 
            // lOGToolStripMenuItem
            // 
            this.lOGToolStripMenuItem.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lOGToolStripMenuItem.Name = "lOGToolStripMenuItem";
            this.lOGToolStripMenuItem.Size = new System.Drawing.Size(54, 27);
            this.lOGToolStripMenuItem.Text = "Log";
            this.lOGToolStripMenuItem.Click += new System.EventHandler(this.lOGToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Enabled = false;
            this.toolStripMenuItem4.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(27, 27);
            this.toolStripMenuItem4.Text = "|";
            // 
            // sETToolStripMenuItem
            // 
            this.sETToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeSerialPortToolStripMenuItem,
            this.autoCalibrationToolStripMenuItem});
            this.sETToolStripMenuItem.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sETToolStripMenuItem.Name = "sETToolStripMenuItem";
            this.sETToolStripMenuItem.Size = new System.Drawing.Size(61, 27);
            this.sETToolStripMenuItem.Text = "Tool";
            // 
            // changeSerialPortToolStripMenuItem
            // 
            this.changeSerialPortToolStripMenuItem.Name = "changeSerialPortToolStripMenuItem";
            this.changeSerialPortToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
            this.changeSerialPortToolStripMenuItem.Text = "Change Serial Port";
            this.changeSerialPortToolStripMenuItem.Click += new System.EventHandler(this.changeSerialPortToolStripMenuItem_Click);
            // 
            // autoCalibrationToolStripMenuItem
            // 
            this.autoCalibrationToolStripMenuItem.Name = "autoCalibrationToolStripMenuItem";
            this.autoCalibrationToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
            this.autoCalibrationToolStripMenuItem.Text = "Auto Calibration";
            this.autoCalibrationToolStripMenuItem.Click += new System.EventHandler(this.autoCalibrationToolStripMenuItem_Click);
            // 
            // panelAuto
            // 
            this.panelAuto.BackColor = System.Drawing.Color.Transparent;
            this.panelAuto.Controls.Add(this.labelTitle);
            this.panelAuto.Controls.Add(this.labelRunTimeV);
            this.panelAuto.Controls.Add(this.labelStateV);
            this.panelAuto.Controls.Add(this.sizeComboBox);
            this.panelAuto.Controls.Add(this.progressBar1);
            this.panelAuto.Controls.Add(this.labelRunTime);
            this.panelAuto.Controls.Add(this.labelState);
            this.panelAuto.Controls.Add(this.picCamera);
            this.panelAuto.Controls.Add(this.btnAutoCal);
            this.panelAuto.Controls.Add(this.btnAutoFocus);
            this.panelAuto.Controls.Add(this.btnAutoScan);
            this.panelAuto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAuto.Location = new System.Drawing.Point(0, 0);
            this.panelAuto.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.panelAuto.Name = "panelAuto";
            this.panelAuto.Size = new System.Drawing.Size(2539, 1061);
            this.panelAuto.TabIndex = 11;
            // 
            // labelTitle
            // 
            this.labelTitle.BackColor = System.Drawing.SystemColors.Info;
            this.labelTitle.Font = new System.Drawing.Font("微軟正黑體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelTitle.ForeColor = System.Drawing.SystemColors.Desktop;
            this.labelTitle.Location = new System.Drawing.Point(49, 92);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(856, 45);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "整合嵌入式控制與深度學習之自動光學晶圓檢測系統";
            // 
            // labelRunTimeV
            // 
            this.labelRunTimeV.AutoSize = true;
            this.labelRunTimeV.BackColor = System.Drawing.Color.White;
            this.labelRunTimeV.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRunTimeV.Location = new System.Drawing.Point(1227, 171);
            this.labelRunTimeV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRunTimeV.Name = "labelRunTimeV";
            this.labelRunTimeV.Size = new System.Drawing.Size(18, 27);
            this.labelRunTimeV.TabIndex = 16;
            this.labelRunTimeV.Text = " ";
            // 
            // labelStateV
            // 
            this.labelStateV.AutoSize = true;
            this.labelStateV.BackColor = System.Drawing.Color.White;
            this.labelStateV.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStateV.Location = new System.Drawing.Point(1168, 111);
            this.labelStateV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStateV.Name = "labelStateV";
            this.labelStateV.Size = new System.Drawing.Size(90, 27);
            this.labelStateV.TabIndex = 15;
            this.labelStateV.Text = "Standby";
            // 
            // sizeComboBox
            // 
            this.sizeComboBox.BackColor = System.Drawing.Color.White;
            this.sizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sizeComboBox.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sizeComboBox.FormattingEnabled = true;
            this.sizeComboBox.Location = new System.Drawing.Point(72, 424);
            this.sizeComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.Size = new System.Drawing.Size(208, 30);
            this.sizeComboBox.TabIndex = 14;
            this.sizeComboBox.Tag = "";
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 1032);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(2539, 29);
            this.progressBar1.TabIndex = 13;
            // 
            // labelRunTime
            // 
            this.labelRunTime.AutoSize = true;
            this.labelRunTime.BackColor = System.Drawing.Color.White;
            this.labelRunTime.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRunTime.Location = new System.Drawing.Point(1075, 169);
            this.labelRunTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRunTime.Name = "labelRunTime";
            this.labelRunTime.Size = new System.Drawing.Size(134, 31);
            this.labelRunTime.TabIndex = 11;
            this.labelRunTime.Text = "Run Time :";
            // 
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.BackColor = System.Drawing.Color.White;
            this.labelState.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelState.Location = new System.Drawing.Point(1075, 111);
            this.labelState.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(89, 31);
            this.labelState.TabIndex = 10;
            this.labelState.Text = "State : ";
            // 
            // picCamera
            // 
            this.picCamera.Location = new System.Drawing.Point(1600, 625);
            this.picCamera.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.picCamera.Name = "picCamera";
            this.picCamera.Size = new System.Drawing.Size(853, 600);
            this.picCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCamera.TabIndex = 9;
            this.picCamera.TabStop = false;
            // 
            // btnAutoCal
            // 
            this.btnAutoCal.BackColor = System.Drawing.Color.White;
            this.btnAutoCal.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoCal.Location = new System.Drawing.Point(148, 928);
            this.btnAutoCal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAutoCal.Name = "btnAutoCal";
            this.btnAutoCal.Size = new System.Drawing.Size(133, 125);
            this.btnAutoCal.TabIndex = 8;
            this.btnAutoCal.Text = "Auto Cal";
            this.btnAutoCal.UseVisualStyleBackColor = false;
            this.btnAutoCal.Click += new System.EventHandler(this.btnAutoCal_Click);
            // 
            // btnAutoFocus
            // 
            this.btnAutoFocus.BackColor = System.Drawing.Color.White;
            this.btnAutoFocus.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoFocus.Location = new System.Drawing.Point(148, 740);
            this.btnAutoFocus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAutoFocus.Name = "btnAutoFocus";
            this.btnAutoFocus.Size = new System.Drawing.Size(133, 125);
            this.btnAutoFocus.TabIndex = 7;
            this.btnAutoFocus.Text = "Auto Focus";
            this.btnAutoFocus.UseVisualStyleBackColor = false;
            this.btnAutoFocus.Click += new System.EventHandler(this.btnAutoFocus_Click);
            // 
            // btnAutoScan
            // 
            this.btnAutoScan.BackColor = System.Drawing.Color.White;
            this.btnAutoScan.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoScan.Location = new System.Drawing.Point(148, 552);
            this.btnAutoScan.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAutoScan.Name = "btnAutoScan";
            this.btnAutoScan.Size = new System.Drawing.Size(133, 125);
            this.btnAutoScan.TabIndex = 6;
            this.btnAutoScan.Text = "AUTO SCAN";
            this.btnAutoScan.UseVisualStyleBackColor = false;
            this.btnAutoScan.Click += new System.EventHandler(this.btnAutoScan_Click);
            // 
            // panelManual
            // 
            this.panelManual.Controls.Add(this.btnB);
            this.panelManual.Controls.Add(this.btnL);
            this.panelManual.Controls.Add(this.btnF);
            this.panelManual.Controls.Add(this.btnR);
            this.panelManual.Controls.Add(this.btnShot);
            this.panelManual.Controls.Add(this.btnStop);
            this.panelManual.Controls.Add(this.btnBackward);
            this.panelManual.Controls.Add(this.btnRight);
            this.panelManual.Controls.Add(this.btnLeft);
            this.panelManual.Controls.Add(this.btnForward);
            this.panelManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelManual.Location = new System.Drawing.Point(0, 0);
            this.panelManual.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.panelManual.Name = "panelManual";
            this.panelManual.Size = new System.Drawing.Size(2539, 1061);
            this.panelManual.TabIndex = 12;
            // 
            // btnB
            // 
            this.btnB.Location = new System.Drawing.Point(301, 836);
            this.btnB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnB.Name = "btnB";
            this.btnB.Size = new System.Drawing.Size(63, 60);
            this.btnB.TabIndex = 9;
            this.btnB.Text = "b";
            this.btnB.UseVisualStyleBackColor = true;
            this.btnB.Click += new System.EventHandler(this.btnB_Click);
            // 
            // btnL
            // 
            this.btnL.Location = new System.Drawing.Point(4, 576);
            this.btnL.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnL.Name = "btnL";
            this.btnL.Size = new System.Drawing.Size(63, 60);
            this.btnL.TabIndex = 8;
            this.btnL.Text = "l";
            this.btnL.UseVisualStyleBackColor = true;
            this.btnL.Click += new System.EventHandler(this.btnL_Click);
            // 
            // btnF
            // 
            this.btnF.Location = new System.Drawing.Point(301, 310);
            this.btnF.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnF.Name = "btnF";
            this.btnF.Size = new System.Drawing.Size(63, 60);
            this.btnF.TabIndex = 7;
            this.btnF.Text = "f";
            this.btnF.UseVisualStyleBackColor = true;
            this.btnF.Click += new System.EventHandler(this.btnF_Click);
            // 
            // btnR
            // 
            this.btnR.Location = new System.Drawing.Point(603, 576);
            this.btnR.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnR.Name = "btnR";
            this.btnR.Size = new System.Drawing.Size(63, 60);
            this.btnR.TabIndex = 6;
            this.btnR.Text = "r";
            this.btnR.UseVisualStyleBackColor = true;
            this.btnR.Click += new System.EventHandler(this.btnR_Click);
            // 
            // btnShot
            // 
            this.btnShot.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShot.Location = new System.Drawing.Point(265, 928);
            this.btnShot.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnShot.Name = "btnShot";
            this.btnShot.Size = new System.Drawing.Size(133, 125);
            this.btnShot.TabIndex = 5;
            this.btnShot.Text = "SHOT";
            this.btnShot.UseVisualStyleBackColor = true;
            this.btnShot.Click += new System.EventHandler(this.btnShot_Click);
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(265, 541);
            this.btnStop.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(133, 125);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnBackward
            // 
            this.btnBackward.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBackward.Location = new System.Drawing.Point(265, 705);
            this.btnBackward.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnBackward.Name = "btnBackward";
            this.btnBackward.Size = new System.Drawing.Size(133, 125);
            this.btnBackward.TabIndex = 3;
            this.btnBackward.Text = "Backward";
            this.btnBackward.UseVisualStyleBackColor = true;
            this.btnBackward.Click += new System.EventHandler(this.btnBackward_Click);
            // 
            // btnRight
            // 
            this.btnRight.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRight.Location = new System.Drawing.Point(464, 541);
            this.btnRight.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(133, 125);
            this.btnRight.TabIndex = 2;
            this.btnRight.Text = "Right";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLeft.Location = new System.Drawing.Point(72, 541);
            this.btnLeft.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(133, 125);
            this.btnLeft.TabIndex = 1;
            this.btnLeft.Text = "Left";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnForward
            // 
            this.btnForward.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnForward.Location = new System.Drawing.Point(265, 376);
            this.btnForward.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(133, 125);
            this.btnForward.TabIndex = 0;
            this.btnForward.Text = "Forward";
            this.btnForward.UseVisualStyleBackColor = true;
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
            // 
            // panelLog
            // 
            this.panelLog.Controls.Add(this.btnAnalyze);
            this.panelLog.Controls.Add(this.tableLayoutPanelPreview);
            this.panelLog.Controls.Add(this.listBoxLog);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Location = new System.Drawing.Point(0, 0);
            this.panelLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelLog.Name = "panelLog";
            this.panelLog.Size = new System.Drawing.Size(2539, 1061);
            this.panelLog.TabIndex = 16;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnalyze.Location = new System.Drawing.Point(2360, 1184);
            this.btnAnalyze.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(117, 51);
            this.btnAnalyze.TabIndex = 3;
            this.btnAnalyze.Text = "Analyze";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // tableLayoutPanelPreview
            // 
            this.tableLayoutPanelPreview.ColumnCount = 3;
            this.tableLayoutPanelPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanelPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanelPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.4F));
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox2, 1, 0);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox3, 2, 0);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox4, 0, 1);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox5, 1, 1);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox6, 2, 1);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox7, 0, 2);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox8, 1, 2);
            this.tableLayoutPanelPreview.Controls.Add(this.pictureBox9, 2, 2);
            this.tableLayoutPanelPreview.Location = new System.Drawing.Point(623, 165);
            this.tableLayoutPanelPreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanelPreview.Name = "tableLayoutPanelPreview";
            this.tableLayoutPanelPreview.RowCount = 3;
            this.tableLayoutPanelPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanelPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanelPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.4F));
            this.tableLayoutPanelPreview.Size = new System.Drawing.Size(1859, 1011);
            this.tableLayoutPanelPreview.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(611, 328);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(623, 4);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(611, 328);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox3.Location = new System.Drawing.Point(1242, 4);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(613, 328);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 2;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox4.Location = new System.Drawing.Point(4, 340);
            this.pictureBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(611, 328);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 3;
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            this.pictureBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox5.Location = new System.Drawing.Point(623, 340);
            this.pictureBox5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(611, 328);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox5.TabIndex = 4;
            this.pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            this.pictureBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox6.Location = new System.Drawing.Point(1242, 340);
            this.pictureBox6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(613, 328);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox6.TabIndex = 5;
            this.pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            this.pictureBox7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox7.Location = new System.Drawing.Point(4, 676);
            this.pictureBox7.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(611, 331);
            this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox7.TabIndex = 6;
            this.pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox8.Location = new System.Drawing.Point(623, 676);
            this.pictureBox8.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(611, 331);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox8.TabIndex = 7;
            this.pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            this.pictureBox9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox9.Location = new System.Drawing.Point(1242, 676);
            this.pictureBox9.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(613, 331);
            this.pictureBox9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox9.TabIndex = 8;
            this.pictureBox9.TabStop = false;
            // 
            // listBoxLog
            // 
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.ItemHeight = 15;
            this.listBoxLog.Location = new System.Drawing.Point(180, 165);
            this.listBoxLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(371, 1069);
            this.listBoxLog.TabIndex = 1;
            this.listBoxLog.SelectedIndexChanged += new System.EventHandler(this.listBoxLog_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2539, 1061);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.panelAuto);
            this.Controls.Add(this.panelManual);
            this.Controls.Add(this.panelLog);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.Text = "自動光學晶圓檢測系統";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panelAuto.ResumeLayout(false);
            this.panelAuto.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
            this.panelManual.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.tableLayoutPanelPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aUTOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mANUALToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lOGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sETToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem saveLogToolStripMenuItem;
        private System.Windows.Forms.Panel panelAuto;
        private System.Windows.Forms.Panel panelManual;
        private System.Windows.Forms.Button btnAutoCal;
        private System.Windows.Forms.Button btnAutoFocus;
        private System.Windows.Forms.Button btnAutoScan;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnBackward;
        private System.Windows.Forms.Label labelRunTime;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.PictureBox picCamera;
        private System.Windows.Forms.Button btnShot;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ToolStripMenuItem saveToJGPToolStripMenuItem;
        private System.Windows.Forms.ComboBox sizeComboBox;
        private System.Windows.Forms.ToolStripMenuItem changeSerialPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoCalibrationToolStripMenuItem;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPreview;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.PictureBox pictureBox9;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Label labelRunTimeV;
        private System.Windows.Forms.Label labelStateV;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnF;
        private System.Windows.Forms.Button btnR;
        private System.Windows.Forms.Button btnB;
        private System.Windows.Forms.Button btnL;
    }
}