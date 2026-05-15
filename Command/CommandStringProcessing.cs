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
    [DeviceCategory("字符串处理指令")]
    public static class CommandStringProcessing
    {
        ///<summary>
        ///获取值
        ///</summary>
        ///<param name="obj">要获取的值。</param>
        ///returns>获取值（字符串）。</returns>
        public static string GetValue(object obj)
        {
            return Convert.ToString(obj) ?? string.Empty;
        }

        /// <summary>
        /// 在原字符串中查找指定字符的位置。
        /// </summary>
        /// <param name="str">要查找的字符串。</param>
        /// <param name="findStr">要查找的字符。</param>
        /// <returns>字符在字符串中的位置，如果未找到则返回 -1。</returns>
        public static int FindStr(string str, string findStr)
        {
            return str.IndexOf(findStr);
        }

        /// <summary>
        /// 将原字符串中的指定字符替换为新字符。
        /// </summary>
        /// <param name="str">要替换的字符串。</param>
        /// <param name="ReplaceStr">要被替换的字符。</param>
        /// <param name="ReplacedStr">替换后的新字符。</param>
        /// <returns>替换后的字符串。</returns>
        public static string ReplaceStr(string str, string ReplaceStr, string ReplacedStr)
        {
            return str.Replace(ReplaceStr, ReplacedStr);
        }

        /// <summary>
        /// 从原字符串中移除指定位置开始的指定个数的字符。
        /// </summary>
        /// <param name="str">要操作的字符串。</param>
        /// <param name="startLocation">开始移除字符的位置。</param>
        /// <param name="removeQty">要移除的字符个数。</param>
        /// <returns>移除后的字符串。</returns>
        public static string RemoveStr(string str, int startLocation, int removeQty)
        {
            return str.Remove(startLocation, removeQty);
        }

        /// <summary>
        /// 在原字符串的指定位置插入新字符串。
        /// </summary>
        /// <param name="str">要操作的字符串。</param>
        /// <param name="startLocation">要插入字符串的位置。</param>
        /// <param name="newStr">要插入的字符串。</param>
        /// <returns>插入后的字符串。</returns>
        public static string InsertStr(string str, int startLocation, string newStr)
        {
            return str.Insert(startLocation, newStr);
        }

        /// <summary>
        /// 从原字符串中截取指定位置开始的指定个数的字符。
        /// </summary>
        /// <param name="str">要操作的字符串。</param>
        /// <param name="startLocation">要截取的起始位置。</param>
        /// <param name="qty">要截取的字符个数。</param>
        /// <returns>截取后的字符串。</returns>
        public static string CaptureStr(string str, int startLocation, int qty)
        {
            return str.Substring(startLocation, qty);
        }

        /// <summary>
        /// 将字符串转换为小写形式。
        /// </summary>
        /// <param name="str">要转换的字符串。</param>
        /// <returns>转换为小写后的字符串。</returns>
        public static string StrConvertToLower(string str)
        {
            return str.ToLower();
        }

        /// <summary>
        /// 将字符串转换为大写形式。
        /// </summary>
        /// <param name="str">要转换的字符串。</param>
        /// <returns>转换为大写后的字符串。</returns>
        public static string StrConvertToUpper(string str)
        {
            return str.ToUpper();
        }

        /// <summary>
        /// 获取字符串的长度。
        /// </summary>
        /// <param name="str">要获取长度的字符串。</param>
        /// <returns>字符串的长度。</returns>
        public static int GetStrLength(string str)
        {
            return str.Length;
        }

        /// <summary>
        /// 去除字符串两端的空白字符。
        /// </summary>
        /// <param name="str">要去除空白字符的字符串。</param>
        /// <returns>去除空白字符后的字符串。</returns>
        public static string StrRemoveLeadingAndTrailingWhitespaces(string str)
        {
            return str.Trim();
        }

        /// <summary>
        /// 将字节数组转换为字符串，使用 UTF-8 编码。
        /// </summary>
        /// <param name="arr">要转换的字节数组。</param>
        /// <returns>转换后的字符串。</returns>
        public static string ByteArrayToStr_UTF8(byte[] arr)
        {
            return Encoding.UTF8.GetString(arr);
        }

        /// <summary>
        /// 将字节数组转换为字符串，使用 GB18030 编码。
        /// </summary>
        /// <param name="arr">要转换的字节数组。</param>
        /// <returns>转换后的字符串。</returns>
        public static string ByteArrayToStr_GB18030(byte[] arr)
        {
            return Encoding.GetEncoding("gb18030").GetString(arr);
        }

        /// <summary>
        /// 将字节数组转换为字符串，使用自选的编码类型。
        /// </summary>
        /// <param name="arr">要转换的字节数组。</param>
        /// <param name="encodingType">要使用的编码类型。</param>
        /// <returns>转换后的字符串。</returns>
        public static string ByteArrayToStr_SelfSelectedEncoding(byte[] arr, string encodingType)
        {
            return Encoding.GetEncoding(encodingType).GetString(arr);
        }

        /// <summary>
        /// 将字节数组转换为16进制字符串
        /// </summary>
        /// <param name="str">要转换的字节数组。</param>
        /// <returns>转换后的字符串。</returns>
        public static string ByteArrayTo_HexStr(byte[] str)
        {
            return Convert.ToInt32(str.ToString(),2).ToString("X2");
        }

        /// <summary>
        /// 将字符串转换为字节数组，使用 UTF-8 编码。
        /// </summary>
        /// <param name="str">要转换的字符串。</param>
        /// <returns>转换后的字节数组。</returns>
        public static byte[] StrToByteArray_UTF8(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        /// <summary>
        /// 将字符串转换为字节数组，使用 GB18030 编码。
        /// </summary>
        /// <param name="str">要转换的字符串。</param>
        /// <returns>转换后的字节数组。</returns>
        public static byte[] StrToByteArray_GB18030(string str)
        {
            return Encoding.GetEncoding("gb18030").GetBytes(str);
        }

        /// <summary>
        /// 将字符串转换为字节数组，使用自选的编码类型。
        /// </summary>
        /// <param name="str">要转换的字符串。</param>
        /// <param name="encodingType">要使用的编码类型。</param>
        /// <returns>转换后的字节数组。</returns>
        public static byte[] StrToByteArray_SelfSelectedEncoding(string str, string encodingType)
        {
            return Encoding.GetEncoding(encodingType).GetBytes(str);
        }

    }
}
