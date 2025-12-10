using System;
using System.Runtime.InteropServices;

namespace Raman1._0
{
    /// <summary>
    /// 封裝 SD1200-UVN 的 SDK 呼叫邏輯，只提供本專案需要的幾個功能：
    /// 1. 初始化並開啟第一台找到的光譜儀
    /// 2. 取得一次光譜（強度陣列 + 對應波長 + 最大值資訊）
    /// 3. 關閉裝置
    /// </summary>
    internal static class SpectrometerInterface
    {
        // DLL 走相對路徑：bin\Debug\SDK_DLL\UserApplication.dll
        private const string SDK_DLL = "SDK_DLL\\UserApplication.dll";

        #region P/Invoke 宣告（全部改用 Cdecl，避免 PInvokeStackImbalance）

        // 3.3.3 UAI_SpectrometerGetDeviceList
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerGetDeviceList(
            out uint number,
            [Out] uint[] list);

        // 3.3.2 UAI_SpectrometerGetDeviceAmount
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerGetDeviceAmount(
            uint vid,
            uint pid,
            out uint deviceCount);

        // 3.3.1 UAI_SpectrometerOpen
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerOpen(
            uint index,
            out IntPtr handle,
            uint vid,
            uint pid);

        // 3.3.4 UAI_SpectrometerClose
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerClose(
            IntPtr handle);

        // 3.3.7 UAI_SpectromoduleGetFrameSize
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectromoduleGetFrameSize(
            IntPtr handle,
            out ushort frameSize);

        // 3.3.8 UAI_SpectrometerWavelengthAcquire
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerWavelengthAcquire(
            IntPtr handle,
            [Out] float[] wavelengthArray);

        // 3.3.10 UAI_SpectrometerDataAcquire
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint UAI_SpectrometerDataAcquire(
            IntPtr handle,
            uint integration_time_us,
            [Out] float[] buffer,
            uint average);

        #endregion

        /// <summary>目前光譜資料點數 (frame size)</summary>
        public static int FrameSize { get; private set; }

        /// <summary>每個資料點對應的波長 (nm)</summary>
        public static float[] Wavelengths { get; private set; }

        /// <summary>
        /// 掃描並開啟第一台找到的光譜儀，同時讀取 frameSize 與 wavelength 表。
        /// 成功回傳 true，失敗回傳 false 並帶出錯誤訊息。
        /// </summary>
        public static bool Initialize(out IntPtr handle, out string errorMessage)
        {
            handle = IntPtr.Zero;
            errorMessage = string.Empty;

            try
            {
                uint status;

                // 1. 取得支援的 VID / PID 列表
                uint listCount = 0;
                uint[] vidPidList = new uint[16]; // 最多 8 組 VID/PID
                status = UAI_SpectrometerGetDeviceList(out listCount, vidPidList);
                if (status != 0 || listCount == 0)
                {
                    errorMessage = $"UAI_SpectrometerGetDeviceList 失敗或無裝置，status = 0x{status:X8}";
                    return false;
                }

                // 2. 對每一組 VID/PID 嘗試尋找裝置數量並開啟第 0 台
                for (int i = 0; i < listCount * 2; i += 2)
                {
                    uint vid = vidPidList[i];
                    uint pid = vidPidList[i + 1];

                    uint deviceCount;
                    status = UAI_SpectrometerGetDeviceAmount(vid, pid, out deviceCount);
                    if (status != 0)
                    {
                        // 先略過這組 VID/PID
                        continue;
                    }

                    if (deviceCount == 0)
                        continue;

                    status = UAI_SpectrometerOpen(0, out handle, vid, pid);
                    if (status == 0 && handle != IntPtr.Zero)
                    {
                        // 開啟成功
                        break;
                    }
                    else
                    {
                        handle = IntPtr.Zero;
                    }
                }

                if (handle == IntPtr.Zero)
                {
                    errorMessage = "找不到可開啟的光譜儀裝置。";
                    return false;
                }

                // 3. 讀取 frame size
                ushort frameSize;
                status = UAI_SpectromoduleGetFrameSize(handle, out frameSize);
                if (status != 0 || frameSize == 0)
                {
                    errorMessage = $"UAI_SpectromoduleGetFrameSize 失敗，status = 0x{status:X8}";
                    return false;
                }

                FrameSize = frameSize;

                // 4. 讀取波長表
                Wavelengths = new float[FrameSize];
                status = UAI_SpectrometerWavelengthAcquire(handle, Wavelengths);
                if (status != 0)
                {
                    errorMessage = $"UAI_SpectrometerWavelengthAcquire 失敗，status = 0x{status:X8}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Initialize 例外：" + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 擷取一次光譜資料。
        /// intensities 長度需 >= FrameSize。
        /// integrationTimeUs 單位為微秒，例如 200000 代表 200 ms。
        /// average 建議先給 1。
        /// </summary>
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
                uint status = UAI_SpectrometerDataAcquire(
                    handle,
                    integrationTimeUs,
                    intensities,
                    average);

                if (status != 0)
                {
                    errorMessage = $"UAI_SpectrometerDataAcquire 失敗，status = 0x{status:X8}";
                    return false;
                }

                // 找出最大值與對應波長
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
                maxWavelength = Wavelengths != null && Wavelengths.Length > maxIndex
                    ? Wavelengths[maxIndex]
                    : 0f;

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "AcquireSpectrum 例外：" + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 關閉光譜儀裝置
        /// </summary>
        public static void Close(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                try
                {
                    UAI_SpectrometerClose(handle);
                }
                catch
                {
                    // 忽略關閉錯誤
                }
            }
        }
    }
}
