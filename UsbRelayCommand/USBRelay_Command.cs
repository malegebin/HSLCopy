using Common.Attributes;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using static Common.Attributes.CommandAttribute;
using static USBRelay_API.USBRelayAPI;

namespace UsbRelayCommand
{
    [Command]
    [DeviceCategory("继电器控制板驱动")]
    public class USBRelay_Command
    {
        #region 私有变量与状态记录
        private static int _handle = 0;
        private static bool _isLibInit = false;
        private static readonly object _lockObj = new object();

        // 极性枚举
        public enum Kml_Enum { KM1 = 1, KM2 = 2, KM3 = 3, KM4 = 4, KM5 = 5, KM6 = 6, KM7 = 7, KM8 = 8 }
        private enum PolarityState { Unknown, Positive, Negative, Off }
        private static PolarityState _currentPolarity = PolarityState.Unknown;
        public static bool IsReady { get; private set; } = false;
        #endregion

        #region 核心连接逻辑 (指针直连法)

        /// <summary>
        /// 确保连接。针对乱码设备，直接使用枚举到的结构体指针打开。
        /// </summary>
        private static void EnsureConnected()
        {
            if (!_isLibInit)
            {
                if (usb_relay_init() != 0) throw new Exception("继电器库初始化失败");
                _isLibInit = true;
            }

            // 如果句柄有效则跳过。注意：如果设备物理掉线，handle 仍可能不为 0，这由 ExecuteRelayAction 的重试机制处理。
            if (_handle != 0) return;

            nint head = usb_relay_device_enumerate();
            if (head == nint.Zero) throw new Exception("未发现继电器设备，请检查 USB 连接或供电");

            try
            {
                // 🚀 关键逻辑：直接使用枚举得到的 head 指针。
                // 这样 API 内部会通过 DevicePath 或底层索引打开，而不去对比那个“口@”乱码。
                _handle = usb_relay_device_open(head);

                if (_handle == 0)
                    throw new Exception("无法打开继电器：设备可能被占用或固件无响应");
            }
            finally
            {
                // 枚举链表用完必须释放
                usb_relay_device_free_enumerate(head);
            }
        }

        private static void ExecuteRelayAction(Action action)
        {
            lock (_lockObj)
            {
                try
                {
                    EnsureConnected();
                    action();
                }
                catch (Exception)
                {
                    // 发生异常（如乱码导致句柄失效）时，重置并重试
                    ResetConnection();
                    EnsureConnected();
                    action();
                }
            }
        }

        private static void ResetConnection()
        {
            if (_handle != 0)
            {
                usb_relay_device_close(_handle);
                _handle = 0;
            }
        }

        public static void FinalizeLib()
        {
            lock (_lockObj)
            {
                ResetConnection();
                if (_isLibInit)
                {
                    usb_relay_exit();
                    _isLibInit = false;
                }
            }
        }
        #endregion

        #region 动作封装 (带保护延迟)

        private static void SafeOpen(Kml_Enum channel1,Kml_Enum channel2)
        {
            int res = usb_relay_device_open_one_relay_channel(_handle, (int)channel1);
            int res2 = usb_relay_device_open_one_relay_channel(_handle, (int)channel2);
            if (res != 0&&res!=0) throw new Exception($"K{channel1},K{channel2} 开启失败");
        }

        private static void SafeClose(Kml_Enum channel1, Kml_Enum channel2)
        {
            int res = usb_relay_device_close_one_relay_channel(_handle, (int)channel1);
            int res2 = usb_relay_device_close_one_relay_channel(_handle, (int)channel2);
            if (res != 0 && res != 0) throw new Exception($"K{channel1},K{channel2} 开启失败");
        }

        #endregion

        #region 业务逻辑逻辑 (互锁控制)

        /// <summary>
        /// 正向：断 3,4 -> 延时 -> 通 1 -> 延时 -> 通 2 (严禁同时通)
        /// </summary>
        public static void SetPositivePolarity()
        {
            ExecuteRelayAction(() => {
                IsReady = false; // 开始动作，锁定状态

                if (_currentPolarity == PolarityState.Positive)
                {
                    IsReady = true;
                    return;
                }

                // 1. 同时断开反向组 (断开可以稍快，但也给200ms缓冲)
                SafeClose(Kml_Enum.KM3, Kml_Enum.KM4);


                // 2. 强行等待死区时间，等待电弧完全熄灭
                Thread.Sleep(120);

                // 3.同时闭合
                SafeOpen(Kml_Enum.KM1, Kml_Enum.KM2);

                _currentPolarity = PolarityState.Positive;
                IsReady = true; // 动作完成，释放状态
            });
        }

        /// <summary>
        /// 负向：断 1,2 -> 延时 -> 通 3 -> 延时 -> 通 4 (严禁同时通)
        /// </summary>
        public static void SetNegativePolarity()
        {
            ExecuteRelayAction(() => {
                IsReady = false; // 开始动作，锁定状态

                if (_currentPolarity == PolarityState.Negative)
                {
                    IsReady = true;
                    return;
                }

                // 1. 断开正向组
                SafeClose(Kml_Enum.KM1, Kml_Enum.KM2);

                // 2. 强行等待死区时间
                Thread.Sleep(500);

                // 3. 【核心优化】逐个开启反向组，严禁同时闭合
                // 3.同时闭合
                SafeOpen(Kml_Enum.KM3, Kml_Enum.KM4);

                _currentPolarity = PolarityState.Negative;
                IsReady = true; // 动作完成，释放状态
            });
        }


        /// <summary>
        ///切断所有k1到k4
        /// </summary>
        public static void SafetyCutOff()
        {
            ExecuteRelayAction(() => {
                IsReady = false;
                SafeClose(Kml_Enum.KM1, Kml_Enum.KM2);
                SafeClose(Kml_Enum.KM3, Kml_Enum.KM4);
                _currentPolarity = PolarityState.Off;
                IsReady = true;
            });
        }

        #endregion
    }
}