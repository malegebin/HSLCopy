using Common.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Attributes.ATSCommandAttribute;

namespace Command
{

    [ATSCommand]
    [DeviceCategory("数组指令")]
    public static class CommandArray
    {
        #region 数组操作
        /// <summary>
        /// 从数组中获取指定位置的元素。
        /// </summary>
        /// <param name="Array">要索引的数组。</param>
        /// <param name="Index">要获取的元素的位置。</param>
        /// <returns>指定位置的元素。</returns>
        public static object IndexArray(object Array, int Index)
        {
            return ((dynamic)Array)[Index];
        }

        /// <summary>
        /// 设置数组中指定位置的元素为给定值。
        /// </summary>
        /// <param name="Array">要设置元素的数组。</param>
        /// <param name="Location">要设置的元素的位置。</param>
        /// <param name="Value">要设置的值。</param>
        /// <returns>修改后的数组。</returns>
        //public static object SetArrayElement(object Array, int Location, object Value)
        //{
        //    ((dynamic)Array)[Location] = (dynamic)Value;
        //    return Array;
        //}

        /// <summary>
        /// 设置数组中指定位置的元素为给定值。
        /// </summary>
        /// <param name="Array">要设置元素的数组。</param>
        /// <param name="Location">要设置的元素的位置（从0开始）。</param>
        /// <param name="Value">要设置的值。</param>
        /// <returns>修改后的数组。</returns>
        /// <exception cref="ArgumentNullException">如果Array为null。</exception>
        /// <exception cref="ArgumentException">如果Array不是数组类型或为字符串。</exception>
        /// <exception cref="IndexOutOfRangeException">如果Location超出数组边界。</exception>
        public static object SetArrayElement(object Array, int Location, object Value)
        {
            // 检查输入是否为null
            if (Array == null)
            {
                throw new ArgumentNullException(nameof(Array), "Array cannot be null.");
            }

            // 检查输入是否为数组类型，但排除字符串
            if (Array is string)
            {
                throw new ArgumentException("Input 'Array' cannot be a string. Strings are immutable.", nameof(Array));
            }

            if (!Array.GetType().IsArray)
            {
                throw new ArgumentException("Input 'Array' must be an array type.", nameof(Array));
            }

            // 将对象转换为数组
            Array arr = (Array)Array;

            // 检查索引是否有效
            if (Location < 0 || Location >= arr.Length)
            {
                throw new IndexOutOfRangeException($"Location {Location} is out of range for array of length {arr.Length}.");
            }

            // 检查Value的类型是否与数组元素类型兼容
            Type arrayElementType = arr.GetType().GetElementType();
            if (Value != null && !arrayElementType.IsAssignableFrom(Value.GetType()))
            {
                // 尝试进行类型转换，如果失败则抛出异常
                try
                {
                    Value = Convert.ChangeType(Value, arrayElementType);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Value '{Value}' cannot be converted to type '{arrayElementType.Name}'.", nameof(Value), ex);
                }
            }

            // 设置元素
            arr.SetValue(Value, Location);

            // 返回修改后的数组
            return Array;
        }
        /// <summary>
        /// 将给定数组的长度设置为指定值，并返回一个新的数组。
        /// 如果指定的长度小于原始数组的长度，则截取原始数组的一部分作为新数组。
        /// </summary>
        /// <param name="Arr">要设置长度的数组。</param>
        /// <param name="Length">新数组的长度。</param>
        /// <returns>新的数组，或者原始数组（如果输入参数不是数组类型）。</returns>
        public static object SetArrayLength(object Arr, int Length)
        {
            if (Arr is Array)
            {
                // 创建一个新的数组实例来存储结果
                Array temp = Array.CreateInstance(Arr.GetType()!.GetElementType()!, Length);
                // 确定复制的长度，取原始数组长度和目标长度的最小值
                int CopyLength = Math.Min(((Array)Arr).Length, Length);
                // 复制原始数组的元素到新数组中
                Array.Copy((Array)Arr, temp, CopyLength);
                return temp;
            }
            else
            {
                // 如果输入参数不是数组类型，则返回原始数组
                return Arr;
            }
        }

        /// <summary>
        /// 在指定位置插入元素到数组中。
        /// </summary>
        /// <param name="Arr">要插入元素的数组。</param>
        /// <param name="Location">要插入元素的位置。</param>
        /// <param name="InsertElement">要插入的元素。</param>
        /// <returns>插入元素后的数组。</returns>
        public static object SetArrayInsertElement(object Arr, int Location, object InsertElement)
        {
            // 将数组转换为可编辑列表
            var temp = Enumerable.ToList((dynamic)Arr);

            // 在指定位置插入元素
            temp.Insert(Location, (dynamic)InsertElement);

            // 如果原数组是数组类型，则将可编辑列表转换回数组并返回
            if (Arr is Array)
            {
                return Enumerable.ToArray(temp);
            }

            // 返回插入元素后的可编辑列表
            return temp;
        }
        /// <summary>
        /// 从数组中删除指定的元素。
        /// </summary>
        /// <param name="Arr">要删除元素的数组。</param>
        /// <param name="DeleteElement">要从数组中删除的元素。</param>
        /// <returns>删除元素后的数组。</returns>
        public static object SetArrayDeleteElement(object Arr, object DeleteElement)
        {
            // 将数组转换为可编辑列表
            var temp = Enumerable.ToList((dynamic)Arr);

            // 从列表中移除指定元素
            temp.Remove((dynamic)DeleteElement);

            // 如果原数组是数组类型，则将可编辑列表转换回数组并返回
            if (Arr is Array)
            {
                return Enumerable.ToArray(temp);
            }

            // 返回删除元素后的可编辑列表
            return temp;
        }
        /// <summary>
        /// 从数组中删除指定位置的元素。
        /// </summary>
        /// <param name="Arr">要删除元素的数组。</param>
        /// <param name="DeleteElementLocation">要从数组中删除的元素的位置。</param>
        /// <returns>删除元素后的数组。</returns>
        public static object SetArrayDeleteLocationElement(object Arr, int DeleteElementLocation)
        {
            var temp = Enumerable.ToList((dynamic)Arr);
            temp.RemoveAt(DeleteElementLocation);
            if (Arr is Array)
            {
                return Enumerable.ToArray(temp);
            }
            return temp;
        }
        /// <summary>
        /// 在数组中寻找第一个匹配的元素，并返回其索引位置。
        /// </summary>
        /// <param name="Arr">要搜索的数组。</param>
        /// <param name="QueryElement">要寻找的元素。</param>
        /// <returns>要寻找的元素在数组中的第一个匹配项的索引；如果未找到匹配项，则为 -1。</returns>
        public static int ArrayQueryFirstMatchElement(object Arr, object QueryElement)
        {
            if (Arr is Array)
            {
                return Array.IndexOf((dynamic)Arr, (dynamic)QueryElement);
            }
            return ((dynamic)Arr).IndexOf((dynamic)QueryElement);
        }
        /// <summary>
        /// 替换数组中第一个匹配的元素为指定的新元素。
        /// </summary>
        /// <param name="Arr">要进行替换操作的数组。</param>
        /// <param name="FirstMatchElement">要替换的元素。</param>
        /// <param name="ReplaceElement">要替换为的新元素。</param>
        /// <returns>替换后的数组。</returns>
        public static object ArrayReplaceFirstMatchElement(object Arr, object FirstMatchElement, object ReplaceElement)
        {
            var temp = Enumerable.ToList((dynamic)Arr);
            int Location = ArrayQueryFirstMatchElement(Arr, FirstMatchElement);
            temp[Location] = (dynamic)ReplaceElement;
            return temp;
        }
        /// <summary>
        /// 将数组中所有匹配的元素替换为指定的新元素。
        /// </summary>
        /// <param name="Arr">要进行替换操作的数组。</param>
        /// <param name="MatchElement">要替换的元素。</param>
        /// <param name="ReplaceElement">要替换为的新元素。</param>
        /// <returns>替换后的数组。</returns>
        public static object ArrayReplaceAllMatchElement(object Arr, object MatchElement, object ReplaceElement)
        {
            var temp = Enumerable.ToList((dynamic)Arr);
            int 长度 = temp.Count;

            for (int i = 0; i < 长度; i++)
            {
                if (((dynamic)Arr)[i] == (dynamic)MatchElement)
                {
                    ((dynamic)Arr)[i] = (dynamic)ReplaceElement;
                }
            }
            return Arr;
        }
        /// <summary>
        /// 反转数组中元素的顺序。
        /// </summary>
        /// <param name="Arr">要反转的数组。</param>
        /// <returns>反转后的数组。</returns>
        public static object ArrayReverse(object Arr)
        {
            var temp = Enumerable.ToList((dynamic)Arr);
            temp.Reverse();
            if (Arr is Array)
            {
                return Enumerable.ToArray(temp);
            }
            return temp;
        }
        /// <summary>
        /// 对数组进行排序。
        /// </summary>
        /// <param name="Arr">要排序的数组。</param>
        /// <returns>排序后的数组。</returns>
        public static object ArraySorting(object Arr)
        {
            var temp = Enumerable.ToList((dynamic)Arr);
            temp = Enumerable.ToList(Enumerable.Order(temp));
            if (Arr is Array)
            {
                return Enumerable.ToArray(temp);
            }
            return temp;
        }
        /// <summary>
        /// 获取数组的长度。
        /// </summary>
        /// <param name="Arr">要获取长度的数组。</param>
        /// <returns>数组的长度。</returns>
        public static int GetArrayLength(object Arr)
        {
            if (Arr is Array)
            {
                return ((Array)Arr).Length;
            }
            return ((ICollection)Arr).Count;

        }
        /// <summary>
        /// 将对象转换为数组。
        /// </summary>
        /// <param name="Arr">要转换的对象。</param>
        /// <returns>转换后的数组。</returns>
        public static object objectConvertToArray(object Arr)
        {
            return Enumerable.ToArray((dynamic)Arr);
        }
        /// <summary>
        /// 将对象转换为列表。
        /// </summary>
        /// <param name="Arr">要转换的对象。</param>
        /// <returns>转换后的列表。</returns>
        public static object objectConvertToList(object Arr)
        {
            return Enumerable.ToList((dynamic)Arr);
        }
        /// <summary>
        /// 从数组中截取指定长度的子数组。
        /// </summary>
        /// <param name="Arr">要截取的数组。</param>
        /// <param name="StartLocation">截取开始的位置。</param>
        /// <param name="Length">要截取的长度。</param>
        /// <returns>截取得到的子数组。</returns>
        public static object ArrayCapture(object Arr, int StartLocation, int Length)
        {
            var temp = Enumerable.ToList((dynamic)Arr);
            temp = Enumerable.ToList(Enumerable.Take(Enumerable.Skip(temp, StartLocation), Length));
            if (Arr is Array)
            {
                return Enumerable.ToArray(temp);
            }
            return temp;
        }

        ///<summary>
        ///计算数组的平均值
        /// </summary>
        /// <param name="Arr">要计算平均值的数组</param>
        public static object? GetArrayAverage(object Arr)
        {
            try
            {
                return Enumerable.Average((dynamic)Arr);
            }
            catch
            {
                return null;
            }
        }

        ///<summary>
        ///获得数组的最大值
        /// </summary>
        /// <param name="Arr">要获得最大值的数组</param>
        public static object? GetArrayMax(object Arr)
        {
            try
            {
                return Enumerable.Max((dynamic)Arr);
            }
            catch
            {
                return null;
            }
        }

        ///<summary>
        ///获得数组的最小值
        /// </summary>
        /// <param name="Arr">要获得最小值的数组</param>
        public static object? GetArrayMin(object Arr)
        {
            try
            {
                return Enumerable.Min((dynamic)Arr);
            }
            catch
            {
                return null;
            }
        }



        #endregion

    }
}
