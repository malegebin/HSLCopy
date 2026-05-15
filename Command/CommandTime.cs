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
    [DeviceCategory("时间处理指令")]
    public static class CommandTime
    {
        #region 时间处理
        /// <summary>
        /// 获取给定时间段的总毫秒数。
        /// </summary>
        /// <param name="TimePeriod">要获取总毫秒数的时间段。</param>
        /// <returns>返回时间段的总毫秒数。</returns>
        public static double GetTimePeriodMilliseconds(TimeSpan TimePeriod)
        {
            // 返回时间段的总毫秒数。
            return TimePeriod.TotalMilliseconds;
        }

        /// <summary>
        /// 获取给定时间段的总秒数。
        /// </summary>
        /// <param name="TimePeriod">要获取总秒数的时间段。</param>
        /// <returns>返回时间段的总秒数。</returns>
        public static double GetTimePeriodSeconds(TimeSpan TimePeriod)
        {
            // 返回时间段的总秒数。
            return TimePeriod.TotalSeconds;
        }

        /// <summary>
        /// 获取给定时间段的总分钟数。
        /// </summary>
        /// <param name="TimePeriod">要获取总分钟数的时间段。</param>
        /// <returns>返回时间段的总分钟数。</returns>
        public static double GetTimePeriodMinutes(TimeSpan TimePeriod)
        {
            // 返回时间段的总分钟数。
            return TimePeriod.TotalMinutes;
        }

        /// <summary>
        /// 获取给定时间段的总小时数。
        /// </summary>
        /// <param name="TimePeriod">要获取总小时数的时间段。</param>
        /// <returns>返回时间段的总小时数。</returns>
        public static double GetTimePeriodHours(TimeSpan TimePeriod)
        {
            // 返回时间段的总小时数。
            return TimePeriod.TotalHours;
        }

        /// <summary>
        /// 获取给定时间段的总天数。
        /// </summary>
        /// <param name="TimePeriod">要获取总天数的时间段。</param>
        /// <returns>返回时间段的总天数。</returns>
        public static double GetTimePeriodDays(TimeSpan TimePeriod)
        {
            // 返回时间段的总天数。
            return TimePeriod.TotalDays;
        }

        /// <summary>
        /// 计算两个日期时间之间的时间差。
        /// </summary>
        /// <param name="date1">第一个日期时间。</param>
        /// <param name="date2">第二个日期时间。</param>
        /// <returns>返回两个日期时间之间的时间差。</returns>
        public static TimeSpan GetDateReduce(DateTime date1, DateTime date2)
        {
            // 返回第一个日期时间减去第二个日期时间的时间差。
            return date1 - date2;
        }

        /// <summary>
        /// 获取当前系统时间。
        /// </summary>
        /// <returns>返回当前系统时间。</returns>
        public static DateTime GetNowTime()
        {
            // 返回当前系统时间。
            return DateTime.Now;
        }
        #endregion

    }
}
