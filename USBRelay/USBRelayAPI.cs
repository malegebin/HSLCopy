using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace USBRelay_API
{
 
    public static class USBRelayAPI
    {
        const string DLL_PATH = ".\\usb_relay_device.dll";

        // --- 1. 结构体映射 ---

        // 对应 C++ 中的 usb_relay_device_type 枚举
        public enum RelayDeviceType : int
        {
            OneChannel = 1,
            TwoChannel = 2,
            FourChannel = 4,
            EightChannel = 8
        }

        // 对应 C++ 中的 usb_relay_device_info 结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct RelayDeviceInfo
        {
            public nint SerialNumber; // unsigned char*
            public nint DevicePath;   // char*
            public RelayDeviceType Type;
            public nint Next;         // 指向下一个节点的指针 (usb_relay_device_info*)
        }

        // --- 2. API 函数导入 ---

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_init();

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_exit();

        // 枚举设备，返回第一个结点的指针
        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint usb_relay_device_enumerate();

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void usb_relay_device_free_enumerate(nint deviceInfo);

        // 打开设备（注意：头文件里返回的是 int 句柄，或者指针 handle）
        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_open(nint deviceInfo);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int usb_relay_device_open_with_serial_number(string serialNumber, uint len);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void usb_relay_device_close(int hHandle);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_open_one_relay_channel(int hHandle, int index);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_close_one_relay_channel(int hHandle, int index);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_open_all_relay_channel(int hHandle);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_close_all_relay_channel(int hHandle);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_get_status(int hHandle, out uint status);
    }
}
