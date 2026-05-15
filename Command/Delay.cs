using Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Common.Attributes.ATSCommandAttribute;

namespace Command
{
    [ATSCommand]
    [DeviceCategory("延时指令")]
    public static class Delay
    {
        /// <summary>
        /// 等待_毫秒
        /// </summary>
        /// <param name="millisecond"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task Delay_ms(int millisecond, CancellationToken ct)
        {
            await Task.Delay(millisecond, ct);
        }

        /// <summary>
        /// 等待_秒
        /// </summary>
        /// <param name="second"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task Delay_s(float second, CancellationToken ct)
        {
            await Task.Delay((int)second * 1000, ct);
        }

        /// <summary>
        /// 等待_分钟
        /// </summary>
        /// <param name="minnute"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task Delay_m(float minnute, CancellationToken ct)
        {
            await Task.Delay((int)minnute * 60 * 1000, ct);
        }

        /// <summary>
        /// 等待_小时
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task Delay_h(float hour, CancellationToken ct)
        {
            await Task.Delay((int)hour* 60 * 60 * 1000, ct);
        }
    }
}
