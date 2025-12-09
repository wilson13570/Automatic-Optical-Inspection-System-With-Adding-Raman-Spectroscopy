using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Raman1._0
{
    static class SpectrometerInterface
    {
        #region SDK DLL Function Imports (P/Invoke)
        private const string SDK_DLL = @"SDK_DLL\UserApplication.dll";

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerGetDeviceList(out uint number, [Out] uint[] list);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerGetDeviceAmount(uint vid, uint pid, out uint deviceCount);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerOpen(uint index, out IntPtr handle, uint vid, uint pid);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerClose(IntPtr handle);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerGetSerialNumber(IntPtr handle, [Out] byte[] serial);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerGetModelName(IntPtr handle, [Out] byte[] model);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectromoduleGetFrameSize(IntPtr handle, out ushort size);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerWavelengthAcquire(IntPtr handle, [Out] float[] wavelengthArray);

        [DllImport(SDK_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern uint UAI_SpectrometerDataOneshot(IntPtr handle, uint integration_time_us, [Out] float[] buffer, uint average);
        #endregion

        /// <summary>
        /// 初始化光譜儀，取得所有已連接裝置的總數量
        /// </summary>
        public static bool Initialize(out uint totalDevices)
        {
            totalDevices = 0;
            uint status;
            uint listCount = 0;
            uint[] vidPidList = new uint[16];  // 緩衝陣列，用來存放 VID/PID 列表 (假設最多支援8組裝置類型)
            status = UAI_SpectrometerGetDeviceList(out listCount, vidPidList);
            if (status != 0 || listCount == 0)
            {
                // 如果status非0代表函式執行錯誤；或listCount為0代表SDK無支援的裝置類型
                return false;
            }
            // 逐一查詢每組 VID/PID 有幾台裝置連接
            for (int i = 0; i < listCount * 2; i += 2)
            {
                uint vid = vidPidList[i];
                uint pid = vidPidList[i + 1];
                uint count = 0;
                status = UAI_SpectrometerGetDeviceAmount(vid, pid, out count);
                if (status != 0)
                {
                    return false;
                }
                totalDevices += count;
            }
            return true;
        }

        /// <summary>
        /// 開啟指定的光譜儀裝置 (依序號索引)，並取得設備序號與型號
        /// </summary>
        public static bool OpenDevice(uint deviceIndex, out IntPtr handle, out string serial, out string model)
        {
            handle = IntPtr.Zero;
            serial = string.Empty;
            model = string.Empty;
            // 取得支援的 VID/PID 列表
            uint listCount = 0;
            uint[] vidPidList = new uint[16];
            uint status = UAI_SpectrometerGetDeviceList(out listCount, vidPidList);
            if (status != 0 || listCount == 0)
            {
                return false;
            }
            // 取第一組 VID/PID 來開啟裝置 (假設 SD1200 是唯一支援類型)
            uint vid = vidPidList[0];
            uint pid = vidPidList[1];
            status = UAI_SpectrometerOpen(deviceIndex, out handle, vid, pid);
            if (status != 0 || handle == IntPtr.Zero)
            {
                return false;
            }
            // 取得裝置序號與型號（各 16 bytes）
            byte[] serialBytes = new byte[16];
            byte[] modelBytes = new byte[16];
            UAI_SpectrometerGetSerialNumber(handle, serialBytes);
            UAI_SpectrometerGetModelName(handle, modelBytes);
            // 將 byte 陣列轉換成字串，去除 '\0' 結尾
            serial = Encoding.ASCII.GetString(serialBytes).TrimEnd('\0');
            model = Encoding.ASCII.GetString(modelBytes).TrimEnd('\0');
            return true;
        }

        /// <summary>
        /// 單次擷取光譜，回傳最大強度及其對應波長
        /// </summary>
        public static bool CaptureSpectrum(IntPtr handle, out float maxIntensity, out float maxWavelength)
        {
            maxIntensity = 0f;
            maxWavelength = 0f;
            if (handle == IntPtr.Zero) return false;
            // 取得光譜資料長度 (frame size)
            ushort frameSize = 0;
            uint status = UAI_SpectromoduleGetFrameSize(handle, out frameSize);
            if (status != 0 || frameSize == 0)
            {
                return false;
            }
            // 建立緩衝陣列以存放光譜強度資料與對應波長
            float[] intensityArray = new float[frameSize];
            float[] wavelengthArray = new float[frameSize];
            // 設定積分時間和平均次數（例如 100ms、平均1次）
            uint integrationTimeUs = 100_000;  // 100,000 微秒 = 100 毫秒
            uint averageCount = 1;
            // 擷取光譜強度資料
            status = UAI_SpectrometerDataOneshot(handle, integrationTimeUs, intensityArray, averageCount);
            if (status != 0)
            {
                return false;
            }
            // 獲取波長陣列
            status = UAI_SpectrometerWavelengthAcquire(handle, wavelengthArray);
            if (status != 0)
            {
                return false;
            }
            // 找出最大強度值及其索引
            float maxVal = -1f;
            int maxIndex = -1;
            for (int i = 0; i < frameSize; i++)
            {
                if (intensityArray[i] > maxVal)
                {
                    maxVal = intensityArray[i];
                    maxIndex = i;
                }
            }
            if (maxIndex >= 0)
            {
                maxIntensity = maxVal;
                maxWavelength = wavelengthArray[maxIndex];
            }
            return true;
        }

        /// <summary>
        /// 關閉光譜儀裝置（釋放資源）
        /// </summary>
        public static void CloseDevice(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                UAI_SpectrometerClose(handle);
            }
        }
    }
}
