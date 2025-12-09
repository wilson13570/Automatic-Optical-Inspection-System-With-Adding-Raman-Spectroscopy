using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;  //Uart套件
using AForge.Video;  //Camera套件
using AForge.Video.DirectShow;  //Camera套件
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;




namespace Raman1._0
{
    public partial class MainForm : Form
    {
        private SerialPort serialPort;  //declare
        private FilterInfoCollection videoDevices;  
        private VideoCaptureDevice videoSource;
        private string selectedPythonScriptPath = null;
        private System.Windows.Forms.Timer runTimeTimer; // 用來計時的 Timer
        private DateTime startTime;          // 記錄開始時間
        private bool isAutoScanRunning = false;
        private TcpListener progressListener;
        private CancellationTokenSource progressCts;
        private bool isManualControlLocked = false;
        private Process tcpServerProcess;

        public MainForm()
        {
            InitializeComponent();
            serialPort = new SerialPort();
            serialPort.PortName = "COM16";        // ⚠️ 實際的 COM port 請依照設備調整
            serialPort.BaudRate = 38400;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = Handshake.None;
            serialPort.Open();
            serialPort.DataReceived += SerialPort_DataReceived;
            StartTcpServer();
            // 尋找所有 USB 鏡頭裝置
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("找不到任何視訊裝置！");
                return;
            }

            // 使用第一個 USB 鏡頭
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            // 設定事件處理，當有新影像時會呼叫 video_NewFrame 方法
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

            // 開始擷取畫面
            videoSource.Start();

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 預設只顯示 Auto 模式的畫面
            panelAuto.Visible = true;
            panelManual.Visible = false;
            panelLog.Visible = false;
            panelSpectrometer.Visible = false;
            sizeComboBox.Items.Add("3×3");
            sizeComboBox.Items.Add("5×5");
            sizeComboBox.Items.Add("7×7");
            sizeComboBox.Items.Add("15×15");

            LoadSampleList();

            runTimeTimer = new System.Windows.Forms.Timer();
            runTimeTimer.Interval = 1000; // 每秒觸發
            runTimeTimer.Tick += RunTimeTimer_Tick;

            StartProgressListener();


            string bgPath = Path.Combine(Application.StartupPath, "Resources", "bg.png");

            if (File.Exists(bgPath))
            {
                // 讀取圖片
                Image bgImage = Image.FromFile(bgPath);

                // 設定每個 Panel 的背景圖
                panelAuto.BackgroundImage = bgImage;
                panelAuto.BackgroundImageLayout = ImageLayout.Stretch;

                panelManual.BackgroundImage = bgImage;
                panelManual.BackgroundImageLayout = ImageLayout.Stretch;

                panelLog.BackgroundImage = bgImage;
                panelLog.BackgroundImageLayout = ImageLayout.Stretch;

                panelSpectrometer.BackgroundImage = bgImage;
                panelSpectrometer.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                MessageBox.Show("找不到背景圖片：" + bgPath);
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // 從背景執行緒複製影像
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

                // ⚠️ PictureBox.Image 要在 UI 執行緒更新
                this.BeginInvoke(new Action(() =>
                {
                    // 釋放舊圖像資源，避免記憶體或例外錯誤
                    if (picCamera.Image != null)
                    {
                        picCamera.Image.Dispose();
                    }

                    picCamera.Image = bitmap;
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ NewFrame 發生錯誤：" + ex.Message);
            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }
        private void DetachCameraFromAllPanels()
        {
            panelAuto.Controls.Remove(picCamera);
            panelManual.Controls.Remove(picCamera);
            panelLog.Controls.Remove(picCamera); // 如果你未來加其他 panel 也一起處理
        }

        private void PerformAutoCalibration()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("Z"); // 傳送 Z 做 Auto Cal
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }
        private void LoadImagesForSelectedSample()
        {
            if (listBoxLog.SelectedItem == null) return;

            string folderName = listBoxLog.SelectedItem.ToString();
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string imageFolderPath = Path.Combine(desktopPath, "SampleData", folderName);

            if (!Directory.Exists(imageFolderPath))
            {
                MessageBox.Show("找不到影像資料夾：" + imageFolderPath);
                return;
            }

            // 判斷是 3x3 還是 5x5
            int rows = 3;
            int cols = 3;

            if (folderName.EndsWith("_5x5"))
            {
                rows = 5;
                cols = 5;
            }

            int totalImages = rows * cols;

            var imageFiles = Directory.GetFiles(imageFolderPath, "*.jpg");
            Array.Sort(imageFiles); // 確保順序

            if (imageFiles.Length < totalImages)
            {
                MessageBox.Show($"圖片不足：需要 {totalImages} 張，實際只有 {imageFiles.Length} 張");
                return;
            }

            // 清空 TableLayoutPanel 舊資料
            tableLayoutPanelPreview.Controls.Clear();
            tableLayoutPanelPreview.RowStyles.Clear();
            tableLayoutPanelPreview.ColumnStyles.Clear();
            tableLayoutPanelPreview.RowCount = rows;
            tableLayoutPanelPreview.ColumnCount = cols;

            for (int r = 0; r < rows; r++)
            {
                tableLayoutPanelPreview.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            }

            for (int c = 0; c < cols; c++)
            {
                tableLayoutPanelPreview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            }

            // 動態建立 PictureBox 並放進格子中
            for (int i = 0; i < totalImages; i++)
            {
                PictureBox pb = new PictureBox();
                pb.Dock = DockStyle.Fill;
                pb.SizeMode = PictureBoxSizeMode.Zoom;

                try
                {
                    pb.Image = Image.FromFile(imageFiles[i]);
                }
                catch
                {
                    MessageBox.Show($"圖片載入失敗：{imageFiles[i]}");
                    continue;
                }

                int row = i / cols;
                int col = i % cols;
                tableLayoutPanelPreview.Controls.Add(pb, col, row);
            }
        }

        private void aUTOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetachCameraFromAllPanels();
            panelAuto.Visible = true;
            panelManual.Visible = false;
            panelLog.Visible = false;
            panelSpectrometer.Visible = false;
            // 回傳 PictureBox 到 Auto Panel
            panelManual.Controls.Remove(picCamera);
            panelAuto.Controls.Add(picCamera);
            picCamera.Location = new Point(1200, 500); // 根據 Auto 畫面的位置調整
        }


        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) //關閉視窗後結束模擬
        {
            Application.Exit(); // 結束整個應用程式
            progressCts?.Cancel();
            progressListener?.Stop();
            if (tcpServerProcess != null && !tcpServerProcess.HasExited)
            {
                try
                {
                    tcpServerProcess.Kill();
                    tcpServerProcess.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("關閉 TCP Server 失敗：" + ex.Message);
                }
            }

        }
        private void mANUALToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetachCameraFromAllPanels();
            panelAuto.Visible = false;
            panelManual.Visible = true;
            panelLog.Visible = false;
            panelSpectrometer.Visible = false;
            // 將 PictureBox 加到 Manual Panel 顯示
            panelAuto.Controls.Remove(picCamera);         // 移出 Auto Panel
            panelManual.Controls.Add(picCamera);          // 加入 Manual Panel
            picCamera.Location = new Point(1200,500);     // 重新設定位置（根據 Manual 版面）
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("A");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnBackward_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("B");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("D");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("C");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void changeSerialPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Form portForm = new Form())
            {
                portForm.Text = "Select Serial Port";
                portForm.Size = new Size(300, 150);

                ComboBox comboBox = new ComboBox() { Left = 30, Top = 20, Width = 200 };
                comboBox.DataSource = System.IO.Ports.SerialPort.GetPortNames();

                Button okButton = new Button() { Text = "OK", Left = 100, Width = 80, Top = 60 };
                okButton.DialogResult = DialogResult.OK;

                portForm.Controls.Add(comboBox);
                portForm.Controls.Add(okButton);
                portForm.AcceptButton = okButton;

                if (portForm.ShowDialog() == DialogResult.OK && comboBox.SelectedItem != null)
                {
                    string selectedPort = comboBox.SelectedItem.ToString();
                    serialPort1.PortName = selectedPort;
                    MessageBox.Show($"Serial port changed to: {selectedPort}");
                }
            }
        }

        private void autoCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformAutoCalibration();
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadExisting();

                foreach (char c in data)
                {
                    int progress = ConvertLetterToProgress(c);

                    if (progress != -1)
                    {
                        this.Invoke(new Action(() =>
                        {
                            progressBar1.Value = progress;

                            // 如果你有 Label 顯示進度（可選）
                            // labelProgress.Text = $"目前進度：{progress}%";

                            if (progress == 100)
                            {
                                runTimeTimer.Stop();                        // ⛔ 停止計時器
                                labelStateV.Text = "Complete";          // ✅ 更新狀態文字
                                isAutoScanRunning = false;                  // ✅ 解除 Auto Scan 鎖定

                                MessageBox.Show("任務完成！");
                            }

                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤（可選）
                MessageBox.Show("接收資料錯誤：" + ex.Message);
            }
        }

        private int ConvertLetterToProgress(char letter)
        {
            letter = char.ToUpper(letter); // 改成支援 E~O (大寫)

            if (letter >= 'E' && letter <= 'O') // E~O 對應 0~100%
            {
                return (letter - 'E') * 10;
            }
            else
            {
                return -1; // 表示無效字元
            }
        }

        private void btnShot_Click(object sender, EventArgs e)
        {
            if (picCamera.Image == null)
            {
                MessageBox.Show("目前無可拍攝畫面！");
                return;
            }

            try
            {
                Bitmap currentFrame = new Bitmap(picCamera.Image);
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string folderPath = Path.Combine(desktopPath, "ManualShots");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                string fileName = Path.Combine(folderPath, $"{timeStamp}.jpg");

                currentFrame.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                MessageBox.Show($"已儲存圖片至：{fileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("拍照失敗：" + ex.Message);
            }
        }


        private void btnRec_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("b");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isManualControlLocked = !isManualControlLocked;

            string status = isManualControlLocked ? "已鎖定手動操作" : "已解鎖，可手動操作";
            MessageBox.Show(status);
        }


        private void lOGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetachCameraFromAllPanels();
            panelAuto.Visible = false;
            panelManual.Visible = false;
            panelLog.Visible = true;
            panelSpectrometer.Visible = false;
        }
        private void LoadSampleList()
        {
            // 取得使用者桌面路徑
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // 組合完整 SampleData 資料夾路徑
            string sampleFolderPath = Path.Combine(desktopPath, "SampleData");

            // 檢查資料夾是否存在
            if (Directory.Exists(sampleFolderPath))
            {
                listBoxLog.Items.Clear(); // ✔️ 改成你設計畫面實際的名稱

                // 取得底下所有子資料夾路徑
                var folders = Directory.GetDirectories(sampleFolderPath);

                foreach (var folder in folders)
                {
                    string folderName = Path.GetFileName(folder); // 只取最後資料夾名稱
                    listBoxLog.Items.Add(folderName); // ✔️ 加進 ListBox
                }
            }
            else
            {
                MessageBox.Show("找不到 SampleData 資料夾，請確認是否已建立！");
            }
        }


        private void listBoxLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadImagesForSelectedSample();
        }
        private void RunPythonAnalysis()
        {
            if (listBoxLog.SelectedItem == null)
            {
                MessageBox.Show("請先選擇一筆樣本資料！");
                return;
            }

            // 使用相對於執行檔的固定路徑
            selectedPythonScriptPath = Path.Combine(Application.StartupPath, "analyze", "analyze.py");

            if (!File.Exists(selectedPythonScriptPath))
            {
                MessageBox.Show("找不到分析腳本 analyze.py，請確認檔案是否存在於 analyze 資料夾！");
                return;
            }

            string folderName = listBoxLog.SelectedItem.ToString();
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string samplePath = Path.Combine(desktopPath, "SampleData", folderName);
            string resultPath = Path.Combine(desktopPath, "AnalyzeResult", folderName);

            Directory.CreateDirectory(resultPath);

            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "python"; // 或填寫 python.exe 的完整路徑
            psi.Arguments = $"\"{selectedPythonScriptPath}\" \"{samplePath}\" \"{resultPath}\"";

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            try
            {
                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(errors))
                    {
                        if (errors.ToLower().Contains("traceback") || errors.ToLower().Contains("error"))
                        {
                            MessageBox.Show("分析失敗：\n" + errors);
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Python 輸出警告訊息（未顯示）:\n" + errors);
                            MessageBox.Show("分析完成，結果已儲存至桌面 AnalyzeResult 資料夾！");
                        }
                    }
                    else
                    {
                        MessageBox.Show("分析完成，結果已儲存至桌面 AnalyzeResult 資料夾！");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("執行分析時發生錯誤：" + ex.Message);
            }
        }


        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            RunPythonAnalysis();
        }

        private async void btnAutoScan_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("X");
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("⚠️ 自動掃描已在執行中，請勿重複啟動！");
                return;
            }

            if (sizeComboBox.SelectedItem == null)
            {
                MessageBox.Show("請先選擇掃描尺寸（例如 3x3、5x5）！");
                return;
            }

            string sizeText = sizeComboBox.SelectedItem.ToString().ToLower().Replace("×", "x");
            string tcpCommand = $"AUTO_SCAN:{sizeText}\n";

            labelStateV.Text = "啟動中...";
            isAutoScanRunning = true;
            startTime = DateTime.Now;        // ⏱️ 提早記錄時間
            labelRunTimeV.Text = "00:00";    // ⏱️ 顯示初始時間
            runTimeTimer.Start();            // ⏱️ 開始計時
            labelStateV.Text = "Running";    // ✅ 提早顯示狀態


            // ✅ 非同步執行 TCP 傳送
            await Task.Run(() =>
            {
                try
                {
                    string response = "";
                    string serverIp = "127.0.0.1";
                    int serverPort = 5000;

                    using (TcpClient client = new TcpClient(serverIp, serverPort))
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(tcpCommand);
                        stream.Write(data, 0, data.Length);

                        byte[] buffer = new byte[1024];
                        var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                        int timeout = 10000000; // 最多等待 100 秒

                        if (readTask.Wait(timeout))
                        {
                            int bytesRead = readTask.Result;
                            response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                            // ⏬原本成功後初始化 UI 的程式碼請接在這裡（不要變動）

                            this.Invoke(new Action(() =>
                            {
                                if (!isAutoScanRunning) return; // ✅ 避免掃描結束後又重啟 UI

                                progressBar1.Value = 0;
                                labelStateV.Text = "進度：0%";
                                startTime = DateTime.Now;
                                labelRunTimeV.Text = "00:00";
                                runTimeTimer.Start();
                                labelStateV.Text = "Running";
                            }));
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show("❌ 等待 Python 回應逾時（未收到任務回應）");
                                labelStateV.Text = "失敗（未回應）";
                                isAutoScanRunning = false;

                                try
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    Console.WriteLine("📴 Timeout後重新連接 SerialPort 成功");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("⚠️ COM 重連失敗: " + ex.Message);
                                }
                            }));
                            return;

                        }


                        if (response != "OK")
                        {
                            this.Invoke(new Action(() =>
                            {
                                labelStateV.Text = "Python異常";   // 可改成 "異常回應" 或 "無效"
                                isAutoScanRunning = false;
                                runTimeTimer.Stop();               // ⛔ 停止計時器
                                progressBar1.Value = 0;            // 重設進度條
                                btnAutoScan.Enabled = true;                          // ❌ 不跳 MessageBox
                            }));
                            return;
                        }

                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("TCP 傳送失敗: " + ex.Message);
                        labelStateV.Text = "失敗";             // ✅ 顯示狀態文字
                        isAutoScanRunning = false;             // ✅ 解除鎖定狀態
                    }));
                }

            });
        }


        private void RunTimeTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            labelRunTimeV.Text = elapsed.ToString(@"mm\:ss");
        }
        private  void StartProgressListener()
        {
            progressCts = new CancellationTokenSource();
            progressListener = new TcpListener(System.Net.IPAddress.Any, 5001);
            progressListener.Start();

            Task.Run(() =>
            {
                try
                {
                    while (!progressCts.IsCancellationRequested)
                    {
                        TcpClient client = progressListener.AcceptTcpClient(); // ✅ 改為同步方法
                        Task.Run(() => HandleProgressClient(client));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Progress Listener 發生錯誤：" + ex.Message);
                }
            });
        }
        private async Task HandleProgressClient(TcpClient client)
        {
            using (client)
            using (var reader = new StreamReader(client.GetStream()))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("PROGRESS:"))
                    {
                        char letter = line.Substring("PROGRESS:".Length).Trim().FirstOrDefault();
                        int progress = ConvertLetterToProgress(letter);

                        if (progress != -1)
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    progressBar1.Value = progress;
                                    labelStateV.Text = $"進度：{progress}%";
                                    if (progress == 100)
                                    {
                                        runTimeTimer.Stop();
                                        labelStateV.Text = "完成";
                                        isAutoScanRunning = false;
                                        LoadSampleList(); // ✅ 更新 Log List
                                        MessageBox.Show("任務完成！");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("⚠️ UI更新錯誤: " + ex.Message);
                                }
                            }));

                        }
                    }
                }
            }
        }
        private async void btnAutoFocus_Click(object sender, EventArgs e)
        {
            string scriptPath = Path.Combine(Application.StartupPath, "Scripts", "Blur_Detection.py");

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("找不到自動對焦腳本 Blur_Detection.py，請確認檔案是否存在！");
                return;
            }

            // ✅ UI 更新：顯示正在對焦中
            labelStateV.Text = "對焦中...";
            btnAutoFocus.Enabled = false;

            await Task.Run(() =>
            {
                var psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "python";
                psi.Arguments = $"\"{scriptPath}\"";
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = Encoding.UTF8;  // 避免亂碼
                psi.StandardErrorEncoding = Encoding.UTF8;

                try
                {
                    using (var process = System.Diagnostics.Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string errors = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        this.Invoke(new Action(() =>
                        {
                            if (!string.IsNullOrEmpty(errors))
                            {
                                labelStateV.Text = "對焦失敗";
                                MessageBox.Show("自動對焦失敗：\n" + errors);
                            }
                            else
                            {
                                labelStateV.Text = "對焦完成";
                                MessageBox.Show("自動對焦完成！");
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        labelStateV.Text = "對焦錯誤";
                        MessageBox.Show("執行自動對焦時發生錯誤：" + ex.Message);
                    }));
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        btnAutoFocus.Enabled = true;  // ✅ 還原按鈕
                    }));
                }
            });
        }

        private void btnF_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("a");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnR_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("c");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnB_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("b");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private void btnL_Click(object sender, EventArgs e)
        {
            if (isManualControlLocked)
            {
                MessageBox.Show("手動操作目前已鎖定！");
                return;
            }
            if (isAutoScanRunning)
            {
                MessageBox.Show("Auto Scan 運作中，無法進行手動操作！");
                return;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("d");
            }
            else
            {
                MessageBox.Show("Serial port not open.");
            }
        }

        private async void btnAutoCal_Click(object sender, EventArgs e)
        {
            string scriptPath = Path.Combine(Application.StartupPath, "Scripts", "cal.py");

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("找不到自動校準腳本 cal.py，請確認檔案是否存在！");
                return;
            }

            labelStateV.Text = "校準中...";
            btnAutoCal.Enabled = false; // 防止重複點擊

            await Task.Run(() =>
            {
                var psi = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                try
                {
                    using (var process = System.Diagnostics.Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string errors = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        this.Invoke(new Action(() =>
                        {
                            if (!string.IsNullOrEmpty(errors))
                            {
                                labelStateV.Text = "校準失敗";
                                MessageBox.Show("自動校準失敗：\n" + errors);
                            }
                            else
                            {
                                labelStateV.Text = "校準完成";
                                MessageBox.Show("自動校準完成！");
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        labelStateV.Text = "校準錯誤";
                        MessageBox.Show("執行自動校準時發生錯誤：" + ex.Message);
                    }));
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        btnAutoCal.Enabled = true; // 還原按鈕狀態
                    }));
                }
            });
        }
        private void StartTcpServer()
        {
            string scriptPath = Path.Combine(Application.StartupPath, "Scripts", "tcp_server.py");

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("找不到 TCP Server 腳本 tcp_server.py！");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                tcpServerProcess = new Process();
                tcpServerProcess.StartInfo = psi;

                tcpServerProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine("TCP stdout: " + e.Data);
                };

                tcpServerProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        MessageBox.Show("TCP 錯誤輸出：\n" + e.Data);
                };

                tcpServerProcess.Start();
                tcpServerProcess.BeginOutputReadLine();
                tcpServerProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show("啟動 TCP Server 失敗：" + ex.Message);
            }
        }

        private void spectrometerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetachCameraFromAllPanels();   // 先把相機 PictureBox 移出所有 Panel

            panelAuto.Visible = false;
            panelManual.Visible = false;
            panelLog.Visible = false;
            panelSpectrometer.Visible = true;   // 只顯示光譜儀 Panel

            // 目前 SpectrometerPanel 不用顯示相機，所以不用把 picCamera 加進去
        }

    }
}
