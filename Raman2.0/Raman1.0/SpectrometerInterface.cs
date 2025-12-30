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

        // 連續抓取（不會清 buffer）
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerDataAcquire(IntPtr handle, uint integration_time_us, [Out] float[] buffer, uint average);

        // 單次抓取（會清 buffer）—建議用於你目前的連續量測
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerDataOneshot(IntPtr handle, uint integration_time_us, [Out] float[] buffer, uint average);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectromoduleGetMaximumIntegrationTime(IntPtr api_handle, out uint time_ms);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectromoduleGetMinimumIntegrationTime(IntPtr api_handle, out uint time_ns);

        #endregion

        public static int FrameSize { get; private set; }
        public static float[] Wavelengths { get; private set; }
        public static float[] DarkBackground { get; private set; }

        #region Error code helpers

        private static uint GetBaseStatus(uint status)
        {
            // SDK: 0x80000000 = API_EXT_START
            if ((status & 0x80000000u) != 0)
                return status & 0x7FFFFFFFu;
            return status;
        }

        private static string DescribeStatus(uint status)
        {
            uint baseCode = GetBaseStatus(status);
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

        private static string FormatStatus(uint status)
        {
            return "0x" + status.ToString("X8") + " (" + DescribeStatus(status) + ")";
        }

        #endregion

        public static bool Initialize(out IntPtr handle, out string errorMessage)
        {
            handle = IntPtr.Zero;
            errorMessage = string.Empty;

            try
            {
                lock (_sdkLock)
                {
                    uint status;

                    uint listCount = 0;
                    uint[] vidPidList = new uint[16]; // 最多 8 組 VID/PID
                    status = UAI_SpectrometerGetDeviceList(out listCount, vidPidList);
                    if (status != 0 || listCount == 0)
                    {
                        errorMessage = "UAI_SpectrometerGetDeviceList 失敗或無裝置，status=" + FormatStatus(status);
                        return false;
                    }

                    for (int i = 0; i < (int)listCount * 2; i += 2)
                    {
                        uint vid = vidPidList[i];
                        uint pid = vidPidList[i + 1];

                        uint deviceCount;
                        status = UAI_SpectrometerGetDeviceAmount(vid, pid, out deviceCount);
                        if (status != 0 || deviceCount == 0)
                            continue;

                        status = UAI_SpectrometerOpen(0, out handle, vid, pid);
                        if (status == 0 && handle != IntPtr.Zero)
                            break;

                        handle = IntPtr.Zero;
                    }

                    if (handle == IntPtr.Zero)
                    {
                        errorMessage = "找不到可開啟的光譜儀裝置。";
                        return false;
                    }

                    ushort frameSize;
                    status = UAI_SpectromoduleGetFrameSize(handle, out frameSize);
                    if (status != 0 || frameSize == 0)
                    {
                        errorMessage = "UAI_SpectromoduleGetFrameSize 失敗，status=" + FormatStatus(status);
                        try { UAI_SpectrometerClose(handle); } catch { }
                        handle = IntPtr.Zero;
                        return false;
                    }

                    FrameSize = frameSize;

                    Wavelengths = new float[FrameSize];
                    status = UAI_SpectrometerWavelengthAcquire(handle, Wavelengths);
                    if (status != 0)
                    {
                        errorMessage = "UAI_SpectrometerWavelengthAcquire 失敗，status=" + FormatStatus(status);
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

                    maxUs = maxMs * 1000u;
                    minUs = (minNs + 999u) / 1000u; // ns->us 向上取整
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

        public static bool AcquireSpectrum(
            IntPtr handle,
            uint integrationTimeUs,
            uint average,
            float[] intensities,
            out float maxIntensity,
            out float maxWavelength,
            out string errorMessage)
        {
            maxIntensity = 0f;
            maxWavelength = 0f;
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
                uint status;
                lock (_sdkLock)
                {
                    // 核心修正：優先用 Oneshot（會清 buffer），避免長時間連抓後狀態卡住導致固定 timeout
                    status = UAI_SpectrometerDataOneshot(handle, integrationTimeUs, intensities, average);

                    // 若某些型號不支援 Oneshot，才 fallback
                    if (GetBaseStatus(status) == 2) // API_INT_FEATURE_UNSUPPORTED
                    {
                        status = UAI_SpectrometerDataAcquire(handle, integrationTimeUs, intensities, average);
                    }
                }

                if (status != 0)
                {
                    errorMessage = "UAI_SpectrometerData(Oneshot/Acquire) 失敗，status=" + FormatStatus(status);
                    return false;
                }

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
