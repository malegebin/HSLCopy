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
    public enum ComparisonFuncEnum
    {
        Big,
        BigOrEqual,
        Small,
        SmallOrEqual,
        Equal,
        NotEqual
    }

    [ATSCommand]
    [DeviceCategory("计算指令")]
    public static class CommandMath
    {
        #region 返回值是Double类型数据
        /// <summary>
        /// 两数相加(Param1 + Param2)
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <param name="Param2">传入值：实数2</param>
        /// <example>Param1:9   Param2:2   返回值:11</example>
        /// <returns></returns>
        public static double ParamAdd(double Param1, double Param2)
        {
            return Param1 + Param2;
        }

        /// <summary>
        /// 两数相减(Param1 - Param2)
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <param name="Param2">传入值：实数2</param>
        /// <example>Param1:9   Param2:2   返回值:7</example>
        /// <returns></returns>
        public static double ParamReduce(double Param1, double Param2)
        {
            return Param1 - Param2;
        }

        /// <summary>
        /// 两数相乘(Param1 * Param2)
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <param name="Param2">传入值：实数2</param>
        /// <example>Param1:9   Param2:2   返回值:18</example>
        /// <returns></returns>
        public static double ParamMult(double Param1, double Param2)
        {
            return Param1 * Param2;
        }

        /// <summary>
        /// 两数相除(Param1 / Param2)
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <param name="Param2">传入值：实数2</param>
        /// <param name="Param3">传入值：保留几位小数</param>
        /// <example>Param1:10   Param2:3   Param3:3   返回值：3.333</example>
        /// <example>Param1:11   Param2:3   Param3:2   返回值：3.67</example>
        /// <returns></returns>
        public static double ParamDivide(double Param1, double Param2, int Param3)
        {
            return Math.Round((Param1 / Param2), Param3);
        }

        /// <summary>
        /// 两数相除求余(Param1 % Param2)
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <param name="Param2">传入值：实数2</param>
        /// <example>Param1:9   Param2:2   返回值:1</example>
        /// <returns></returns>
        public static double ParamRemainder(double Param1, double Param2)
        {
            return Param1 % Param2;
        }

        /// <summary>
        /// 实数开根号
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <example>Param1:16   返回值：4</example>
        /// <returns></returns>
        public static double ParamSquareRoot(double Param1)
        {
            return Math.Sqrt(Param1);
        }

        /// <summary>
        /// 实数的幂运算
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <param name="Param2">传入值：实数1需要做几次方</param>
        /// <example>Param1:3   Param2：2   返回值：9</example>
        /// <example>Param1:2   Param2：3   返回值：8</example>
        /// <example>Param1:5   Param2：4   返回值：625</example>
        /// <returns></returns>
        public static double ParamPow(double Param1, double Param2)
        {
            return Math.Pow(Param1, Param2);
        }

        /// <summary>
        /// 实数的绝对值
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <example>Param1:-3.7   返回值：3.7</example>
        /// <returns></returns>
        public static double ParamAbs(double Param1)
        {
            return Math.Abs(Param1);
        }

        /// <summary>
        /// 实数的向下取整
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <example>Param1:3.7   返回值：3</example>
        /// <returns></returns>
        public static double ParamFloor(double Param1)
        {
            return Math.Floor(Param1);
        }

        /// <summary>
        /// 实数的向上取整
        /// </summary>
        /// <param name="Param1">传入值：实数1</param>
        /// <example>Param1:3.7   返回值：4</example>
        /// <returns></returns>
        public static double ParamCeiling(double Param1)
        {
            return Math.Ceiling(Param1);
        }
        #endregion


        #region 返回值是String类型的数据
        /// <summary>
        /// A与B转换为double后比较,返回A比较B的结果
        /// </summary>
        /// <param name="a">比较数1</param>
        /// <param name="b">比较数2</param>
        /// <param name="比较方法">比较方法</param>
        /// <returns>返回A比较B</returns>
        public static bool Comparison(string a, string b, ComparisonFuncEnum 比较方法)
        {
            var aa = Convert.ToDouble(a);
            var bb = Convert.ToDouble(b);

            switch (比较方法)
            {
                case ComparisonFuncEnum.Big:
                    return aa > bb;
                case ComparisonFuncEnum.BigOrEqual:
                    return aa >= bb;
                case ComparisonFuncEnum.Small:
                    return aa < bb;
                case ComparisonFuncEnum.SmallOrEqual:
                    return aa <= bb;
                case ComparisonFuncEnum.Equal:
                    return aa == bb;
                case ComparisonFuncEnum.NotEqual:
                    return aa != bb;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 加法指令,转换为double后返回o1+o2
        /// </summary>
        /// <param name="o1">运算数1</param>
        /// <param name="o2">运算数2</param>
        /// <returns>返回o1+o2</returns>
        public static string TwoNumberAdd(string o1, string o2)
        {
            return (Convert.ToDecimal(o1) + Convert.ToDecimal(o2)).ToString();
        }
        /// <summary>
        /// 减法指令,转换为double后返回o1-o2
        /// </summary>
        /// <param name="o1">运算数1</param>
        /// <param name="o2">运算数2</param>
        /// <returns>返回o1-o2</returns>
        public static string TwoNumberReduce(string o1, string o2)
        {
            return (Convert.ToDecimal(o1) - Convert.ToDecimal(o2)).ToString();
        }
        /// <summary>
        /// 乘法指令,转换为double后返回o1*o2
        /// </summary>
        /// <param name="o1">运算数1</param>
        /// <param name="o2">运算数2</param>
        /// <returns>返回o1*o2</returns>
        public static string TwoNumberMult(string o1, string o2)
        {
            return (Convert.ToDecimal(o1) * Convert.ToDecimal(o2)).ToString();
        }
        /// <summary>
        /// 除法指令,转换为double后返回o1/o2
        /// </summary>
        /// <param name="o1">运算数1</param>
        /// <param name="o2">运算数2</param>
        /// <returns>返回o1/o2</returns>
        public static string TwoNumberDivide1(string o1, string o2)
        {
            return (Convert.ToDecimal(o1) / Convert.ToDecimal(o2)).ToString();
        }
        /// <summary>
        /// 转换为double后返回o1/o2
        /// </summary>
        /// <param name="o1">运算数1</param>
        /// <param name="o2">运算数2</param>
        /// <param name="o3">保留几位小数</param>
        /// <returns>返回o1/o2</returns>
        public static string TwoNumberDivide2(string o1, string o2, int o3)
        {
            return (Math.Round((Convert.ToDecimal(o1) / Convert.ToDecimal(o2)), o3)).ToString();
        }
        /// <summary>
        /// 返回o1%o2
        /// </summary>
        /// <param name="o1">运算数1</param>
        /// <param name="o2">运算数2</param>
        /// <returns>返回o1%o2</returns>
        public static string TwoNumberRemainder(string o1, string o2)
        {
            return (Convert.ToDecimal(o1) % (Convert.ToDecimal(o2))).ToString();
        }
        /// <summary>
        /// 计算两个数字的平方。
        /// </summary>
        /// <param name="o1">第一个数字的字符串表示。</param>
        /// <param name="o2">第二个数字的字符串表示。</param>
        /// <example>Param1:3   Param2：2   返回值：9</example>
        /// <example>Param1:2   Param2：3   返回值：8</example>
        /// <example>Param1:5   Param2：4   返回值：625</example>
        /// <returns>返回两个数字的平方的字符串表示。</returns>
        public static string NumberPow(string o1, string o2)
        {
            // 将输入的字符串转换为双精度浮点数，并计算它们的平方
            // 最后将结果转换为字符串并返回
            return Math.Pow(Convert.ToDouble(o1), Convert.ToDouble(o2)).ToString();
        }
        /// <summary>
        /// 计算一个数的平方根。
        /// </summary>
        /// <param name="o1">要计算平方根的数字的字符串表示。</param>
        /// <returns>返回输入数字的平方根的字符串表示。</returns>
        public static string NumberSqrt(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其平方根。
            // 最后将结果转换为字符串并返回。
            return Math.Sqrt(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个数的绝对值。
        /// </summary>
        /// <param name="o1">要计算绝对值的数字的字符串表示。</param>
        /// <returns>返回输入数字的绝对值的字符串表示。</returns>
        public static string NumberAbs(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其绝对值。
            // 最后将结果转换为字符串并返回。
            return Math.Abs(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个角度的正弦值。
        /// </summary>
        /// <param name="o1">要计算正弦值的角度的字符串表示（单位为弧度）。</param>
        /// <returns>返回输入角度的正弦值的字符串表示。</returns>
        public static string NumberSin(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其正弦值。
            // 最后将结果转换为字符串并返回。
            return Math.Sin(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个角度的余弦值。
        /// </summary>
        /// <param name="o1">要计算余弦值的角度的字符串表示（单位为弧度）。</param>
        /// <returns>返回输入角度的余弦值的字符串表示。</returns>
        public static string NumberCos(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其余弦值。
            // 最后将结果转换为字符串并返回。
            return Math.Cos(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个角度的正切值。
        /// </summary>
        /// <param name="o1">要计算正切值的角度的字符串表示（单位为弧度）。</param>
        /// <returns>返回输入角度的正切值的字符串表示。</returns>
        public static string NumberTan(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其正切值。
            // 最后将结果转换为字符串并返回。
            return Math.Tan(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个数的反正弦值。
        /// </summary>
        /// <param name="o1">要计算反正弦值的数的字符串表示。</param>
        /// <returns>返回输入数的反正弦值的字符串表示（单位为弧度）。</returns>
        public static string NumberASin(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其反正弦值。
            // 最后将结果转换为字符串并返回。
            return Math.Asin(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个数的反余弦值。
        /// </summary>
        /// <param name="o1">要计算反余弦值的数的字符串表示。</param>
        /// <returns>返回输入数的反余弦值的字符串表示（单位为弧度）。</returns>
        public static string NumberACos(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其反余弦值。
            // 最后将结果转换为字符串并返回。
            return Math.Acos(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个数的反正切值。
        /// </summary>
        /// <param name="o1">要计算反正切值的数的字符串表示。</param>
        /// <returns>返回输入数的反正切值的字符串表示（单位为弧度）。</returns>
        public static string NumberATan(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其反正切值。
            // 最后将结果转换为字符串并返回。
            return Math.Atan(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算 e 的给定次幂。
        /// </summary>
        /// <param name="o1">e 的指数的字符串表示。</param>
        /// <returns>返回 e 的给定次幂的字符串表示。</returns>
        public static string eIndex(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算 e 的给定次幂。
            // 最后将结果转换为字符串并返回。
            return Math.Exp(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个数的自然对数。
        /// </summary>
        /// <param name="o1">要计算自然对数的数的字符串表示。</param>
        /// <returns>返回输入数的自然对数的字符串表示。</returns>
        public static string NumberNaturalLogarithm(string o1)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其自然对数。
            // 最后将结果转换为字符串并返回。
            return Math.Log(Convert.ToDouble(o1)).ToString();
        }
        /// <summary>
        /// 计算一个数在指定基数下的对数。
        /// </summary>
        /// <param name="o1">要计算对数的数的字符串表示。</param>
        /// <param name="Base">对数的基数的字符串表示。</param>
        /// <returns>返回输入数在指定基数下的对数的字符串表示。</returns>
        public static string NumberLogarithm(string o1, string Base)
        {
            // 将输入的字符串转换为双精度浮点数，并计算其在指定基数下的对数。
            // 最后将结果转换为字符串并返回。
            return Math.Log(Convert.ToDouble(o1), Convert.ToDouble(Base)).ToString();
        }
        /// <summary>
        /// 对输入数字进行四舍五入取整。
        /// </summary>
        /// <param name="n1">要进行取整操作的数字的字符串表示。</param>
        /// <returns>返回经过四舍五入取整后的结果的字符串表示。</returns>
        public static string NumberRoundToNearest(string n1)
        {
            // 将输入的字符串转换为双精度浮点数，并进行四舍五入取整。
            // 最后将结果转换为字符串并返回。
            return Math.Round(Convert.ToDouble(n1)).ToString();
        }

        /// <summary>
        /// 对输入数字进行向上舍入取整。
        /// </summary>
        /// <param name="n1">要进行取整操作的数字的字符串表示。</param>
        /// <returns>返回经过向上舍入取整后的结果的字符串表示。</returns>
        public static string NumberRoundUp(string n1)
        {
            // 将输入的字符串转换为双精度浮点数，并进行向上舍入取整。
            // 最后将结果转换为字符串并返回。
            return Math.Ceiling(Convert.ToDouble(n1)).ToString();
        }

        /// <summary>
        /// 对输入数字进行向下舍入取整。
        /// </summary>
        /// <param name="n1">要进行取整操作的数字的字符串表示。</param>
        /// <returns>返回经过向下舍入取整后的结果的字符串表示。</returns>
        public static string NumberRoundDown(string n1)
        {
            // 将输入的字符串转换为双精度浮点数，并进行向下舍入取整。
            // 最后将结果转换为字符串并返回。
            return Math.Floor(Convert.ToDouble(n1)).ToString();
        }

        /// <summary>
        /// 获取圆周率π的字符串表示。
        /// </summary>
        /// <returns>返回圆周率π的字符串表示。</returns>
        public static string PI()
        {
            // 返回圆周率π的字符串表示。
            return Math.PI.ToString();
        }

        //public static Random random1 = new Random();
        /// <summary>
        /// 生成一个随机整数。
        /// </summary>
        /// <returns>返回生成的随机整数的字符串表示。</returns>
        public static string RandomNumber_Int()
        {
            Random random = new Random();
            // 使用随机数生成器生成一个随机整数，并将其转换为字符串表示。
            return random.Next().ToString();
        }

        /// <summary>
        /// 生成一个指定范围内的随机整数。
        /// </summary>
        /// <param name="最小">随机数生成范围的最小值。</param>
        /// <param name="最大">随机数生成范围的最大值（不包含）。</param>
        /// <returns>返回生成的指定范围内的随机整数的字符串表示。</returns>
        public static string RandomNumber_RangeInt(int 最小, int 最大)
        {
            Random random = new Random();
            // 使用随机数生成器生成一个指定范围内的随机整数，并将其转换为字符串表示。
            return random.Next(最小, 最大).ToString();
        }

        /// <summary>
        /// 生成一个随机小数。
        /// </summary>
        /// <returns>返回生成的随机小数的字符串表示。</returns>
        public static string RandomNumber_Decimal()
        {
            Random random = new Random();
            // 使用随机数生成器生成一个随机小数，并将其转换为字符串表示。
            return random.NextDouble().ToString();
        }

        /// <summary>
        /// 计算给定算式的结果。
        /// </summary>
        /// <param name="s">要计算的算式的字符串表示。</param>
        /// <returns>返回计算结果的字符串表示。</returns>
        public static string FormulaCalculation(string s)
        {
            // 创建一个 DataTable 实例来进行算式的计算。
            DataTable dataTable = new DataTable();
            // 使用 DataTable 的 Compute 方法计算给定的算式，并将结果转换为字符串表示。
            return dataTable.Compute(s, "").ToString();
        }

        /// <summary>
        /// 给double类型的变量赋值。
        /// </summary>
        /// <param name="Param1">被赋值的数值。</param>
        /// <param name="Param2">数值。</param>
        /// <returns>返回赋值后的值。</returns>
        public static double AssignValue(double Param1, double Param2)
        {
            Param1 = Param2;
            return Param1;
        }


        #endregion
    }
}
