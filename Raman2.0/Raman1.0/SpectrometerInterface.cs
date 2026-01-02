using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Raman1._0
{
    internal static class SpectrometerInterface
    {
        private const string SDK_DLL = "SDK_DLL\\UserApplication.dll";
        private static readonly object _sdkLock = new object();

        #region P/Invoke（Cdecl）

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerGetDeviceList(out uint number, [Out] uint[] list);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerGetDeviceAmount(uint vid, uint pid, out uint deviceCount);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerOpen(uint index, out IntPtr handle, uint vid, uint pid);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerClose(IntPtr handle);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectromoduleGetFrameSize(IntPtr handle, out ushort frameSize);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerWavelengthAcquire(IntPtr handle, [Out] float[] wavelengthArray);

        // 連續抓取（不清 buffer）
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerDataAcquire(IntPtr handle, uint integration_time_us, [Out] float[] buffer, uint average);

        // 單次抓取（會清 buffer）
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerDataOneshot(IntPtr handle, uint integration_time_us, [Out] float[] buffer, uint average);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectromoduleGetMaximumIntegrationTime(IntPtr api_handle, out uint time_ms);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectromoduleGetMinimumIntegrationTime(IntPtr api_handle, out uint time_ns);

        // 觸發模式開關（SDK 2.47）
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerSetTriggerIO(IntPtr api_handle, uint enable, uint level);

        #endregion

        public static int FrameSize { get; private set; }
        public static float[] Wavelengths { get; private set; }
        public static float[] DarkBackground { get; private set; }

        // 供「自動恢復」使用：記住最後一次成功開啟的 VID/PID/Index
        private static bool _hasLastOpenParams;
        private static uint _lastVid;
        private static uint _lastPid;
        private static uint _lastIndex;

        #region Error helpers（公開給 Panel 判斷）

        public static uint GetBaseStatusCode(uint status)
        {
            // SDK: 0x80000000 = API_EXT_START（延伸錯誤碼起始）
            if ((status & 0x80000000u) != 0)
                return status & 0x7FFFFFFFu;
            return status;
        }

        public static bool IsTimeoutStatus(uint status) => GetBaseStatusCode(status) == 8u;
        public static bool IsProtocolErrorStatus(uint status) => GetBaseStatusCode(status) == 3u;
        public static bool IsHandleInvalidStatus(uint status) => GetBaseStatusCode(status) == 7u;

        public static string FormatStatus(uint status)
        {
            return "0x" + status.ToString("X8") + " (" + DescribeStatus(status) + ")";
        }

        private static string DescribeStatus(uint status)
        {
            uint baseCode = GetBaseStatusCode(status);
            string name;
            switch (baseCode)
            {
                case 0: name = "API_SUCCESS"; break;
                case 1: name = "API_INT_BUFFER_INVALID"; break;
                case 2: name = "API_INT_FEATURE_UNSUPPORTED"; break;
                case 3: name = "API_INT_PROTOCOL_ERROR"; break;
                case 4: name = "API_INT_CALIBRATION_ERROR"; break;
                case 5: name = "API_INT_MEMORY_ERROR"; break;
                case 6: name = "API_INT_ARGUMENT_ERROR"; break;
                case 7: name = "API_INT_HANDLE_INVALID"; break;
                case 8: name = "API_INT_TIMEOUT"; break;
                default: name = "UNKNOWN"; break;
            }

            if ((status & 0x80000000u) != 0)
                return name + " (API_EXT_START)";
            return name;
        }

        #endregion

        #region Initialize / Recover

        public static bool Initialize(out IntPtr handle, out string errorMessage)
        {
            handle = IntPtr.Zero;
            errorMessage = string.Empty;

            try
            {
                lock (_sdkLock)
                {
                    uint vid, pid, index;
                    string openErr;

                    if (!TryOpenFirstAvailable(out handle, out vid, out pid, out index, out openErr))
                    {
                        errorMessage = openErr;
                        return false;
                    }

                    // 記住開啟參數（供自動恢復使用）
                    _hasLastOpenParams = true;
                    _lastVid = vid;
                    _lastPid = pid;
                    _lastIndex = index;

                    // 強制關閉 Trigger mode（避免在不知情情況下卡等待觸發而 TIMEOUT）
                    DisableTriggerModeBestEffort(handle);

                    // 讀 frame size / wavelengths
                    string infoErr;
                    if (!RefreshDeviceInfo(handle, out infoErr))
                    {
                        errorMessage = infoErr;
                        try { UAI_SpectrometerClose(handle); } catch { }
                        handle = IntPtr.Zero;
                        return false;
                    }

                    // 初始化後嘗試載入暗背景（有檔就載，沒有就略過）
                    DarkBackground = null;
                    string darkErr;
                    LoadDarkBackgroundFromFile(out darkErr);

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Initialize 例外：" + ex.Message;
                return false;
            }
        }

        public static bool Recover(ref IntPtr handle, out string message)
        {
            message = string.Empty;

            try
            {
                lock (_sdkLock)
                {
                    // 先關閉舊 handle（即使已經壞掉也 try）
                    if (handle != IntPtr.Zero)
                    {
                        try { UAI_SpectrometerClose(handle); } catch { }
                        handle = IntPtr.Zero;
                    }

                    // 優先用「上次成功的 VID/PID/Index」重開
                    if (_hasLastOpenParams)
                    {
                        uint st = UAI_SpectrometerOpen(_lastIndex, out handle, _lastVid, _lastPid);
                        if (st == 0 && handle != IntPtr.Zero)
                        {
                            DisableTriggerModeBestEffort(handle);

                            string infoErr;
                            if (!RefreshDeviceInfo(handle, out infoErr))
                            {
                                message = "Recover: 重新讀取裝置資訊失敗：" + infoErr;
                                try { UAI_SpectrometerClose(handle); } catch { }
                                handle = IntPtr.Zero;
                                return false;
                            }

                            message = "Recover: 使用上次 VID/PID/Index 重開成功。";
                            return true;
                        }
                    }

                    // 如果上次參數失敗，重新掃描整個清單再開一次
                    uint vid, pid, index;
                    string openErr;
                    if (!TryOpenFirstAvailable(out handle, out vid, out pid, out index, out openErr))
                    {
                        message = "Recover: 重新掃描仍找不到可開啟裝置：" + openErr;
                        handle = IntPtr.Zero;
                        return false;
                    }

                    _hasLastOpenParams = true;
                    _lastVid = vid;
                    _lastPid = pid;
                    _lastIndex = index;

                    DisableTriggerModeBestEffort(handle);

                    string infoErr2;
                    if (!RefreshDeviceInfo(handle, out infoErr2))
                    {
                        message = "Recover: 重新讀取裝置資訊失敗：" + infoErr2;
                        try { UAI_SpectrometerClose(handle); } catch { }
                        handle = IntPtr.Zero;
                        return false;
                    }

                    message = "Recover: 重新掃描後重開成功。";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Recover 例外：" + ex.Message;
                handle = IntPtr.Zero;
                return false;
            }
        }

        private static bool TryOpenFirstAvailable(out IntPtr handle, out uint vid, out uint pid, out uint index, out string errorMessage)
        {
            handle = IntPtr.Zero;
            vid = 0;
            pid = 0;
            index = 0;
            errorMessage = string.Empty;

            uint listCount = 0;
            uint[] vidPidList = new uint[32]; // 最多 16 組 VID/PID

            uint status = UAI_SpectrometerGetDeviceList(out listCount, vidPidList);
            if (status != 0 || listCount == 0)
            {
                errorMessage = "UAI_SpectrometerGetDeviceList 失敗或無裝置，status=" + FormatStatus(status);
                return false;
            }

            int pairCount = (int)Math.Min(listCount, (uint)(vidPidList.Length / 2));
            for (int i = 0; i < pairCount; i++)
            {
                uint v = vidPidList[i * 2];
                uint p = vidPidList[i * 2 + 1];

                uint deviceCount;
                status = UAI_SpectrometerGetDeviceAmount(v, p, out deviceCount);
                if (status != 0 || deviceCount == 0)
                    continue;

                for (uint devIndex = 0; devIndex < deviceCount; devIndex++)
                {
                    status = UAI_SpectrometerOpen(devIndex, out handle, v, p);
                    if (status == 0 && handle != IntPtr.Zero)
                    {
                        vid = v;
                        pid = p;
                        index = devIndex;
                        return true;
                    }
                    handle = IntPtr.Zero;
                }
            }

            errorMessage = "找不到可開啟的光譜儀裝置（可能驅動/權限/占用）。";
            return false;
        }

        private static void DisableTriggerModeBestEffort(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            try
            {
                // enable=0 => Disable trigger mode
                uint st = UAI_SpectrometerSetTriggerIO(handle, 0u, 0u);

                // 若不支援（FEATURE_UNSUPPORTED）就忽略；其他錯誤也先不讓初始化失敗
                // 因為某些型號可能沒有 trigger pin
                _ = st;
            }
            catch
            {
                // ignore
            }
        }

        private static bool RefreshDeviceInfo(IntPtr handle, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (handle == IntPtr.Zero)
            {
                errorMessage = "RefreshDeviceInfo: handle 為 0。";
                return false;
            }

            ushort frameSize;
            uint st = UAI_SpectromoduleGetFrameSize(handle, out frameSize);
            if (st != 0 || frameSize == 0)
            {
                errorMessage = "UAI_SpectromoduleGetFrameSize 失敗，status=" + FormatStatus(st);
                return false;
            }

            FrameSize = frameSize;

            Wavelengths = new float[FrameSize];
            st = UAI_SpectrometerWavelengthAcquire(handle, Wavelengths);
            if (st != 0)
            {
                errorMessage = "UAI_SpectrometerWavelengthAcquire 失敗，status=" + FormatStatus(st);
                return false;
            }

            // 若已有暗背景但點數不同，直接作廢，避免錯誤扣除
            if (DarkBackground != null && DarkBackground.Length != FrameSize)
                DarkBackground = null;

            return true;
        }

        #endregion

        #region Integration time range (us)

        public static bool TryGetIntegrationTimeRangeUs(
            IntPtr handle,
            out uint minUs,
            out uint maxUs,
            out string errorMessage)
        {
            minUs = 1_000;      // 1 ms
            maxUs = 1_000_000;  // 1 s
            errorMessage = string.Empty;

            if (handle == IntPtr.Zero)
            {
                errorMessage = "TryGetIntegrationTimeRangeUs: handle 為 0。";
                return false;
            }

            try
            {
                lock (_sdkLock)
                {
                    uint stMax = UAI_SpectromoduleGetMaximumIntegrationTime(handle, out uint maxMs);
                    uint stMin = UAI_SpectromoduleGetMinimumIntegrationTime(handle, out uint minNs);

                    if (stMax != 0 || stMin != 0)
                    {
                        errorMessage =
                            "取得 integration time 範圍失敗：max=" + FormatStatus(stMax) +
                            ", min=" + FormatStatus(stMin);
                        return false;
                    }

                    maxUs = maxMs * 1000u;                 // ms -> us
                    minUs = (minNs + 999u) / 1000u;        // ns -> us 向上取整
                    if (minUs == 0) minUs = 1;
                    if (maxUs < minUs) maxUs = minUs;

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "TryGetIntegrationTimeRangeUs 例外：" + ex.Message;
                return false;
            }
        }

        #endregion

        #region Acquire Spectrum（含 TIMEOUT 自動恢復）

        public static bool AcquireSpectrum(
            ref IntPtr handle,
            uint integrationTimeUs,
            uint average,
            float[] intensities,
            bool oneshot,
            out float maxIntensity,
            out float maxWavelength,
            out uint status,
            out string errorMessage)
        {
            maxIntensity = 0f;
            maxWavelength = 0f;
            status = 0u;
            errorMessage = string.Empty;

            if (handle == IntPtr.Zero)
            {
                errorMessage = "AcquireSpectrum: handle 為 0，尚未初始化裝置。";
                return false;
            }
            if (FrameSize <= 0 || Wavelengths == null)
            {
                errorMessage = "AcquireSpectrum: FrameSize / Wavelengths 尚未初始化。";
                return false;
            }
            if (intensities == null || intensities.Length < FrameSize)
            {
                errorMessage = "AcquireSpectrum: intensities 陣列長度不足。";
                return false;
            }

            try
            {
                // 第一次嘗試
                status = AcquireCore(handle, integrationTimeUs, average, intensities, oneshot);

                if (status != 0)
                {
                    // 可恢復的狀況：TIMEOUT / PROTOCOL / HANDLE_INVALID
                    if (IsTimeoutStatus(status) || IsProtocolErrorStatus(status) || IsHandleInvalidStatus(status))
                    {
                        string recMsg;
                        bool recOk = Recover(ref handle, out recMsg);

                        if (!recOk || handle == IntPtr.Zero)
                        {
                            errorMessage = "UAI_SpectrometerData 失敗，status=" + FormatStatus(status) +
                                           "；自動恢復失敗：" + recMsg;
                            return false;
                        }

                        // 恢復後，確保外部傳入的 buffer 點數足夠
                        if (FrameSize <= 0 || Wavelengths == null || intensities.Length < FrameSize)
                        {
                            errorMessage = "自動恢復後 FrameSize/Wavelengths/buffer 不一致，請重新初始化。";
                            return false;
                        }

                        // 恢復後再試一次（第二次）
                        status = AcquireCore(handle, integrationTimeUs, average, intensities, oneshot);
                        if (status != 0)
                        {
                            errorMessage = "自動恢復後仍擷取失敗，status=" + FormatStatus(status) + "；" + recMsg;
                            return false;
                        }
                    }
                    else
                    {
                        errorMessage = "UAI_SpectrometerData 失敗，status=" + FormatStatus(status);
                        return false;
                    }
                }

                // 計算最大值與對應波長
                float localMax = float.MinValue;
                int maxIndex = 0;
                for (int i = 0; i < FrameSize; i++)
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
                maxWavelength = (Wavelengths != null && Wavelengths.Length > maxIndex) ? Wavelengths[maxIndex] : 0f;

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "AcquireSpectrum 例外：" + ex.Message;
                return false;
            }
        }

        private static uint AcquireCore(IntPtr handle, uint integrationTimeUs, uint average, float[] intensities, bool oneshot)
        {
            uint st;

            lock (_sdkLock)
            {
                if (oneshot)
                {
                    st = UAI_SpectrometerDataOneshot(handle, integrationTimeUs, intensities, average);

                    // 若不支援 Oneshot，fallback 到 Acquire
                    if (GetBaseStatusCode(st) == 2u) // FEATURE_UNSUPPORTED
                        st = UAI_SpectrometerDataAcquire(handle, integrationTimeUs, intensities, average);
                }
                else
                {
                    st = UAI_SpectrometerDataAcquire(handle, integrationTimeUs, intensities, average);

                    // 若 Acquire 不支援（理論上少見），fallback 到 Oneshot
                    if (GetBaseStatusCode(st) == 2u) // FEATURE_UNSUPPORTED
                        st = UAI_SpectrometerDataOneshot(handle, integrationTimeUs, intensities, average);
                }
            }

            return st;
        }

        #endregion

        #region Close

        public static void Close(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            try
            {
                lock (_sdkLock)
                {
                    UAI_SpectrometerClose(handle);
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region Dark Background Management (C# 7.3 compatible)

        private static string GetDarkFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background_dark.csv");
        }

        public static bool SetDarkBackground(float[] background, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (background == null)
            {
                errorMessage = "暗背景資料為 null。";
                return false;
            }
            if (FrameSize <= 0)
            {
                errorMessage = "FrameSize 尚未初始化。請先 Initialize。";
                return false;
            }
            if (background.Length != FrameSize)
            {
                errorMessage = "暗背景資料長度與 FrameSize 不一致。";
                return false;
            }

            var copy = new float[FrameSize];
            Array.Copy(background, copy, FrameSize);
            DarkBackground = copy;
            return true;
        }

        public static bool SaveDarkBackgroundToFile(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (DarkBackground == null)
            {
                errorMessage = "尚未設定 DarkBackground，無法儲存。";
                return false;
            }
            if (Wavelengths == null || Wavelengths.Length != FrameSize)
            {
                errorMessage = "Wavelengths 尚未就緒，無法儲存。";
                return false;
            }

            try
            {
                string path = GetDarkFilePath();
                using (var sw = new StreamWriter(path, false))
                {
                    for (int i = 0; i < FrameSize; i++)
                    {
                        string wl = Wavelengths[i].ToString("R", CultureInfo.InvariantCulture);
                        string v = DarkBackground[i].ToString("R", CultureInfo.InvariantCulture);
                        sw.WriteLine(wl + "," + v);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "儲存暗背景失敗：" + ex.Message;
                return false;
            }
        }

        public static bool LoadDarkBackgroundFromFile(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                string path = GetDarkFilePath();
                if (!File.Exists(path))
                    return false;

                string[] lines = File.ReadAllLines(path);
                if (FrameSize <= 0 || lines.Length != FrameSize)
                {
                    errorMessage = "暗背景檔案點數與目前 FrameSize 不一致，略過載入。";
                    return false;
                }

                var bg = new float[FrameSize];
                for (int i = 0; i < FrameSize; i++)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length < 2)
                    {
                        errorMessage = "暗背景檔案格式錯誤（缺少逗號分隔）。";
                        return false;
                    }

                    if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float intensity))
                    {
                        errorMessage = "暗背景檔案數值解析失敗（第 " + i + " 行）。";
                        return false;
                    }

                    bg[i] = intensity;
                }

                DarkBackground = bg;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "載入暗背景失敗：" + ex.Message;
                return false;
            }
        }

        #endregion
    }
}
