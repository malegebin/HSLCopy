using Common.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Attributes.ATSCommandAttribute;

namespace Command
{

    [ATSCommand]
    [DeviceCategory("进制转换指令")]
    public static class CommandRadixChange
    {
        #region 进制转换
        /// <summary>
        /// 二进制 转换成 八进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"101010"   返回值:"52"</example>
        /// <returns></returns>
        public static string TwoRadix_ConvertTo_EightRadix(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 2),8);
        }

        /// <summary>
        /// 二进制 转换成 十进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"101010"   返回值:"42"</example>
        /// <returns></returns>
        public static string TwoRadix_ConvertTo_TenRadix(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 2));
        }

        /// <summary>
        /// 二进制 转换成 十六进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"101010"   返回值:"2A"</example>
        /// <returns></returns>
        public static string TwoRadix_ConvertTo_SixteenRadix(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 2), 16);
        }

        /// <summary>
        /// 八进制 转换成 二进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"88"   返回值:"1001000"</example>
        /// <returns></returns>
        public static string EightRadix_ConvertTo_TwoRadix(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 8), 2);
        }

        /// <summary>
        /// 八进制 转换成 十进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"88"   返回值:"72"</example>
        /// <returns></returns>
        public static double EightRadix_ConvertTo_TenRadix(string Param1)
        {
            double Value = Convert.ToInt32(Param1, 8);
            return Value;
        }

        /// <summary>
        /// 八进制 转换成 十六进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"88"   返回值:"48"</example>
        /// <returns></returns>
        public static string EightRadix_ConvertTo_SixteenRadix(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 8), 16);
        }

        /// <summary>
        /// 十进制 转换成 二进制
        /// </summary>
        /// <param name="Param1">传入值：整型数值</param>
        /// <example>Param1:60   返回值:"111100"</example>
        /// <returns></returns>
        public static string TenRadix_ConvertTo_TwoRadix(int Param1)
        {
            return Convert.ToString(Param1, 2);
        }

        /// <summary>
        /// 十进制 转换成 八进制
        /// </summary>
        /// <param name="Param1">传入值：整型数值</param>
        /// <example>Param1:60   返回值:"74"</example>
        /// <returns></returns>
        public static string TenRadix_ConvertTo_EightRadix(int Param1)
        {
            return Convert.ToString(Param1, 8);
        }

        /// <summary>
        /// 十进制 转换成 十六进制
        /// </summary>
        /// <param name="Param1">传入值：整型数值</param>
        /// <example>Param1:60   返回值:"3C"</example>
        /// <returns></returns>
        public static string TenRadix_ConvertTo_SixteenRadix(int Param1)
        {
            return Convert.ToString(Param1, 16);
        }

        /// <summary>
        /// 十六进制 转换成 二进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"6F"   返回值:"1101111"</example>
        /// <returns></returns>
        public static string SixteenRadix_ConvertTo_Radix2(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 16), 2);
        }

        /// <summary>
        /// 十六进制 转换成 八进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"6F"   返回值:"157"</example>
        /// <returns></returns>
        public static string SixteenRadix_ConvertTo_EightRadix8(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 16), 8);
        }

        /// <summary>
        /// 十六进制 转换成 十进制
        /// </summary>
        /// <param name="Param1">传入值：字符类型</param>
        /// <example>Param1:"6F"   返回值:"111"</example>
        /// <returns></returns>
        public static string SixteenRadix_ConvertTo_TenRadix(string Param1)
        {
            return Convert.ToString(Convert.ToInt32(Param1, 16));
        }


        #endregion

    }
}
