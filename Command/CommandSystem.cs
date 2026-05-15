using Common.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Common.Attributes.ATSCommandAttribute;

namespace Command
{
    [ATSCommand]
    [DeviceCategory("系统指令")]
    public static class CommandSystem
    {
        #region 类型转换
        /// <summary>
        /// int转化为double
        /// </summary>
        /// <param name="Param1">传入值：整型数值</param>
        /// <example>Param1:40   返回值：40</example>
        /// <returns></returns>
        public static double Int_ConvertTo_Double(int Param1)
        {
            return Convert.ToDouble(Param1);
        }

        /// <summary>
        /// double转化为Int16
        /// </summary>
        /// <param name="Param1">传入值：实数数值</param>
        /// <example>Param1:4   返回值：4</example>
        /// <example>Param1:4.67   返回值：4</example>
        /// <returns></returns>
        public static Int16 Double_ConvertTo_Int16(double Param1)
        {
            Int16 Value = Convert.ToInt16(Param1);
            return Value;
        }

        /// <summary>
        /// double转化为int,即Int32
        /// </summary>
        /// <param name="Param1">传入值：实数数值</param>
        /// <example>Param1:4   返回值：4</example>
        /// <example>Param1:4.67   返回值：4</example>
        /// <returns></returns>
        public static int Double_ConvertTo_Int(double Param1)
        {
            int Value = Convert.ToInt32(Param1);
            return Value;
        }

        /// <summary>
        /// double转化为Int64
        /// </summary>
        /// <param name="Param1">传入值：实数数值</param>
        /// <example>Param1:4   返回值：4</example>
        /// <example>Param1:4.67   返回值：4</example>
        /// <returns></returns>
        public static Int64 Double_ConvertTo_Int64(double Param1)
        {
            Int64 Value = Convert.ToInt64(Param1);
            return Value;
        }

        /// <summary>
        /// float转化为double
        /// </summary>
        /// <param name="Param1">传入值：浮点数值</param>
        /// <example>Param1:4.0   返回值：4</example>
        /// <example>Param1:4.2   返回值：4.2</example>
        /// <returns></returns>
        public static double Float_ConvertTo_Double(float Param1)
        {
            double Value = Convert.ToDouble(Param1);
            return Value;
        }

        /// <summary>
        /// 判断string转化为int，转换成功返回 true，失败返回 false
        /// </summary>
        /// <param name="Param1">传入值：字符串</param>
        /// <example>Param1:"2"   返回值：true</example>
        /// <example>Param1:"abc"   返回值：false</example>
        /// <returns></returns>
        public static bool BoolString_ConvertTo_Int(string Param1)
        {
            bool Value = int.TryParse(Param1, out int intValue);
            return Value;
        }

        /// <summary>
        /// string转化为int
        /// </summary>
        /// <param name="Param1">传入值：字符串</param>
        /// <example>Param1:"2"   返回值：2</example>
        /// <returns></returns>
        public static int String_ConvertTo_Int(string Param1)
        {
            int Value = Convert.ToInt32(Param1);
            return Value;
        }

        /// <summary>
        /// int转化为string
        /// </summary>
        /// <param name="Param1">传入值：整型数值</param>
        /// <example>Param1:2   返回值："2"</example>
        /// <returns></returns>
        public static string Int_ConvertTo_String(int Param1)
        {
            string Value = Convert.ToString(Param1);
            return Value;
        }

        /// <summary>
        /// 判断string转化为double，转换成功返回 true，失败返回 false
        /// </summary>
        /// <param name="Param1">传入值：字符串</param>
        /// <example>Param1:"2"   返回值：true</example>
        /// <example>Param1:"2.1"   返回值：true</example>
        /// <example>Param1:"abc"   返回值：false</example>
        /// <returns></returns>
        public static bool BoolString_ConvertTo_Double(string Param1)
        {
            bool Value = double.TryParse(Param1, out double intValue);
            return Value;
        }

        /// <summary>
        /// string转化为double
        /// </summary>
        /// <param name="Param1">传入值：字符串</param>
        /// <example>Param1:"2"   返回值：2</example>
        /// <example>Param1:"2.1"   返回值：2.1</example>
        /// <returns></returns>
        public static double String_ConvertTo_Double(string Param1)
        {
            double Value = Convert.ToDouble(Param1);
            return Value;
        }

        /// <summary>
        /// double转化为string
        /// </summary>
        /// <param name="Param1">传入值：实数数值</param>
        /// <example>Param1:2   返回值："2"</example>
        /// <example>Param1:2.1   返回值："2.1"</example>
        /// <returns></returns>
        public static string Double_ConvertTo_String(double Param1)
        {
            string Value = Convert.ToString(Param1);
            return Value;
        }

        /// <summary>
        /// float转化为string
        /// </summary>
        /// <param name="Param1">传入值：浮点数值</param>
        /// <example>Param1:2.0   返回值："2.0"</example>
        /// <example>Param1:2.1   返回值："2.1"</example>
        /// <returns></returns>
        public static string Float_ConvertTo_String(float Param1)
        {
            string Value = Convert.ToString(Param1);
            return Value;
        }

        /// <summary>
        /// 判断string转化为datetime，转换成功返回 true，失败返回 false
        /// </summary>
        /// <param name="Param1">传入值：时间字符串</param>
        /// <example>Param1:"2025-08-14"   返回值：true</example>
        /// <example>Param1:"2025-08-14 13:14:15"   返回值：true</example>
        /// <example>Param1:"abc"   返回值：false</example>
        /// <returns></returns>
        public static bool BoolString_ConvertTo_Datetime(string Param1)
        {
            bool Value = DateTime.TryParse(Param1, out DateTime DateTimeValue);
            return Value;
        }

        /// <summary>
        /// string转化为datetime
        /// </summary>
        /// <param name="Param1">传入值：时间字符串</param>
        /// <example>Param1:"2025-08-14"   返回值：2025/8/14 0:00:00</example>
        /// <example>Param1:"2025-08-14 13:14:15"   返回值：2025/8/14 13:14:15</example>
        /// <returns></returns>
        public static DateTime String_ConvertTo_Datetime(string Param1)
        {
            DateTime Value = Convert.ToDateTime(Param1);
            return Value;
        }

        /// <summary>
        /// datetime转化为string
        /// </summary>
        /// <param name="Param1">传入值：时间</param>
        /// <example>Param1:2025/8/14 13:14:15   返回值："2025/08/14 13:14:15"</example>
        /// <returns></returns>
        public static string Datetime_ConvertTo_String1(DateTime Param1)
        {
            string Value = Convert.ToString(Param1);
            return Value;
        }

        /// <summary>
        /// datetime转化为string
        /// </summary>
        /// <param name="Param1">传入值：时间</param>
        /// <param name="DateTimeFormat">传入值：字符串时间格式</param>
        /// <example>Param1:2025/8/14 13:14:15   DateTimeFormat:"yyyy-MM-dd"            返回值："2025-08-14"</example>
        /// <example>Param1:2025/8/14 13:14:15   DateTimeFormat:"yyyy-MM-dd hh:mm:ss"   返回值："2025-08-14 01:14:15"</example>
        /// <example>Param1:2025/8/14 13:14:15   DateTimeFormat:"yyyy-MM-dd HH:mm:ss"   返回值："2025-08-14 13:14:15"</example>
        /// <example>Param1:2025/8/14 13:14:15   DateTimeFormat:"yyyyMMdd"              返回值："20250814"</example>
        /// <returns></returns>
        public static string Datetime_ConvertTo_String2(DateTime Param1,string DateTimeFormat)
        {
            string Value = Param1.ToString(DateTimeFormat);
            return Value;
        }

        /// <summary>
        /// bool转化为string
        /// </summary>
        /// <param name="Param1">传入值：布尔值</param>
        /// <example>Param1:true   返回值："true"</example>
        /// <example>Param1:false   返回值："false"</example>
        /// <returns></returns>
        public static string Bool_ConvertTo_String(bool Param1)
        {
            string Value = Convert.ToString(Param1);
            return Value;
        }

        /// <summary>
        /// string转化为bool
        /// </summary>
        /// <param name="Param1">传入值：字符串</param>
        /// <example>Param1:"true"   返回值：true</example>
        /// <example>Param1:"false"   返回值：false</example>
        /// <example>Param1:"abc"   返回值：false</example>
        /// <returns></returns>
        public static bool String_ConvertTo_Bool(string Param1)
        {
            bool Value = bool.TryParse(Param1, out bool boolValue);
            return Value;
        }

        /// <summary>
        /// 将无符号整型转换为整型
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static int Uint_ConvertTo_Int(uint u)
        {
            return (int)u;
        }

        /// <summary>
        /// 将对象转换为字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Object_ConvertTo_String(object obj)
        {
            return Convert.ToString(obj);
        }

        /// <summary>
        /// 将字符串内容转换为byte
        /// </summary>
        /// <param name="s">要转换的内容</param>
        /// <returns></returns>
        public static byte String_ConvertTo_Byte(string s)
        {
            return Convert.ToByte(s);
        }

        /// <summary>
        /// 转换为short(int16)
        /// </summary>
        /// <param name="s">要转换的内容</param>
        /// <returns></returns>
        public static short String_ConvertTo_Short(string s)
        {
            return Convert.ToInt16(s);
        }

        /// <summary>
        /// 将字符串转换为无符号整数。
        /// </summary>
        /// <param name="s">要转换的字符串。</param>
        /// <returns>转换后的无符号整数。</returns>
        public static uint String_ConvertTo_Uint(string s)
        {
            return Convert.ToUInt32(s);
        }

        /// <summary>
        /// 将指定进制的字符串转换为整数。
        /// </summary>
        /// <param name="s">要转换的字符串。</param>
        /// <param name="XRadix">源字符串的进制。只能为2,8,10,16</param>
        /// <returns>转换后的整数。</returns>
        public static int XRadixString_ConvertTo_Int(string s, int XRadix)
        {
            return Convert.ToInt32(s, XRadix);
        }
        #endregion


        #region 逻辑操作
        /// <summary>
        /// 执行整数的异或操作。
        /// </summary>
        /// <param name="a">第一个整数。</param>
        /// <param name="b">第二个整数。</param>
        /// <returns>异或操作结果。</returns>
        public static int XOR(int a, int b)
        {
            return a ^ b;
        }

        /// <summary>
        /// 执行整数的位或操作。
        /// </summary>
        /// <param name="a">第一个整数。</param>
        /// <param name="b">第二个整数。</param>
        /// <returns>位或操作结果。</returns>
        public static int BitwiseOR(int a, int b)
        {
            return a | b;
        }

        /// <summary>
        /// 执行整数的位与操作。
        /// </summary>
        /// <param name="a">第一个整数。</param>
        /// <param name="b">第二个整数。</param>
        /// <returns>位与操作结果。</returns>
        public static int BitwiseAND(int a, int b)
        {
            return a & b;
        }

        /// <summary>
        /// 将整数向左移动指定位数。
        /// </summary>
        /// <param name="a">要移动的整数。</param>
        /// <param name="BitwiseLeftShift">左移的位数。</param>
        /// <returns>移动后的结果。</returns>
        public static int LeftShift(int a, int BitwiseLeftShift)
        {
            return a << BitwiseLeftShift;
        }
        /// <summary>
        /// 将整数向右移动指定位数。
        /// </summary>
        /// <param name="a">要移动的整数。</param>
        /// <param name="BitwiseRightShift">右移的位数。</param>
        /// <returns>移动后的结果。</returns>
        public static int RightShift(int a, int BitwiseRightShift)
        {
            return a >> BitwiseRightShift;
        }
        /// <summary>
        /// 执行逻辑或操作。
        /// </summary>
        /// <param name="a">第一个逻辑值。</param>
        /// <param name="b">第二个逻辑值。</param>
        /// <returns>逻辑或操作结果。</returns>
        public static bool LogicalOR(bool a, bool b)
        {
            return a || b;
        }

        /// <summary>
        /// 执行逻辑与操作。
        /// </summary>
        /// <param name="a">第一个逻辑值。</param>
        /// <param name="b">第二个逻辑值。</param>
        /// <returns>逻辑与操作结果。</returns>
        public static bool LogicalAND(bool a, bool b)
        {
            return a && b;
        }

        /// <summary>
        /// 将字节的高低位对调。
        /// </summary>
        /// <param name="b">要对调的字节。</param>
        /// <returns>对调后的字节。</returns>
        public static byte ByteHighAndLowBitSwap(byte b)
        {
            return (byte)(((b & 0x0F) << 4) | ((b & 0xF0) >> 4));
        }

        /// <summary>
        /// 翻转字节数组中的元素顺序。
        /// </summary>
        /// <param name="b">要翻转的字节数组。</param>
        /// <returns>翻转后的字节数组。</returns>
        public static byte[] ByteArrayReverse(byte[] b)
        {
            return b.Reverse().ToArray();
        }

        /// <summary>
        /// 将当前线程挂起指定的时间。
        /// </summary>
        /// <param name="time">要延时的时间，以毫秒为单位。</param>
        public static void Delay_ms(int time)
        {
            Thread.Sleep(time);
        }

        /// <summary>
        /// 将当前线程挂起指定的时间。
        /// </summary>
        /// <param name="time">要延时的时间，以秒为单位。</param>
        public static void Delay_s(int time)
        {
            Thread.Sleep(time * 1000);
        }

        /// <summary>
        /// 将当前线程挂起指定的时间。
        /// </summary>
        /// <param name="time">要延时的时间，以分钟为单位。</param>
        public static void Delay_min(int time)
        {
            Thread.Sleep(time * 1000 * 60);
        }

        /// <summary>
        /// 将当前线程挂起指定的时间。
        /// </summary>
        /// <param name="time">要延时的时间，以小时为单位。</param>
        public static void Delay_hour(int time)
        {
            Thread.Sleep(time * 1000 * 60 * 60);
        }

        /// <summary>
        /// 将当前线程挂起指定的时间。
        /// </summary>
        /// <param name="time">要延时的时间，以天为单位。</param>
        public static void Delay_day(int time)
        {
            Thread.Sleep(time * 1000 * 60 * 60 * 24);
        }

        #endregion



        #region

        #endregion
    }
}
