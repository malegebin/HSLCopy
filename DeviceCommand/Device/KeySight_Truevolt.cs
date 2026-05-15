using Common.Attributes;
using DeviceCommand.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Attributes.CommandAttribute;

namespace DeviceCommand.Device
{
    /// <summary>
    /// 万用表（型号：Truevolt 系列）
    /// </summary>
    [Command]
    [DeviceCategory("万用表")] // 添加分类属性
    public class KeySight_Truevolt : Tcp
    {

        public SemaphoreSlim semaphoreSlimLock { get; set; } = new(1, 1);

        /// <summary>
        /// 耦合模式（AC DC）
        /// </summary>
        public enum Coupling_Mode
        {
            AC,
            DC,
        }

        /// <summary>
        /// 万用表电压量程枚举（单位：伏特/V）
        /// 枚举值为整数形式，实际量程 = 枚举值 / 10.0（例如：1 → 0.1V，10 → 1V）
        /// </summary>
        public enum Voltage_Range_Mode
        {
            /// <summary>
            /// 0.1伏特（100毫伏）
            /// </summary>
            Range_100MV = 1,

            /// <summary>
            /// 1伏特
            /// </summary>
            Range_1V = 10,

            /// <summary>
            /// 10伏特
            /// </summary>
            Range_10V = 100,

            /// 100伏特
            /// </summary>
            Range_100V = 1000,

            /// <summary>
            /// 500伏特（常用量程上限）
            /// </summary>
            Range_700V = 7500
        }

        /// <summary>
        /// 切换为为本地模式(默认连接上就是远程模式)
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_LocalMode(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"SYSTem:LOCal\r\n", ct);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 测量切换为DC电压
        /// </summary>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task SetConfig_VoltageDC(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                //CONFigure[:VOLTage]:{AC|DC}[{<range>|AUTO|MIN|MAX|DEF}[, {<resolution>|MIN|MAX|DEF}]]
                await SendAsync($"CONFigure:VOLTage:DC\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        #region 测量电压
        /// <summary>
        /// 复位仪器
        /// </summary>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task SYSTem_PRESet(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                //这个万用表的指令都要/r/n
                await SendAsync($"SYSTem:PRESet\r\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        ///   设置DC电压量程（交流电压AC）
        /// </summary>
        /// <param name="rangeMode">量程</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_VoltageACRange(Voltage_Range_Mode rangeMode, CancellationToken ct = default)
        {
            double range = (double)rangeMode / 10.0;

            if (range > 1000)
            {
                throw new ArgumentNullException($"{range}超出规定的最大量程");
            }
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                //CONFigure[:VOLTage]:{AC|DC}[{<range>|AUTO|MIN|MAX|DEF}[, {<resolution>|MIN|MAX|DEF}]]
                await SendAsync($"VOLTage:AC:RANGe {range}\r\n", ct); // 使用 \n 作为 SCPI 命令结束符
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        ///   设置DC电压量程（直流电压DC）
        /// </summary>
        /// <param name="rangeMode">量程</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_VoltageDCRange(Voltage_Range_Mode rangeMode, CancellationToken ct = default)
        {
            double range = (double)rangeMode / 10.0;

            if (range > 1000)
            {
                throw new ArgumentNullException($"{range}超出规定的最大量程");
            }
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                //CONFigure[:VOLTage]:{AC|DC}[{<range>|AUTO|MIN|MAX|DEF}[, {<resolution>|MIN|MAX|DEF}]]
                await SendAsync($"VOLTage:DC:RANGe {range}\r\n", ct); // 使用 \n 作为 SCPI 命令结束符
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 触发DC并读取结果
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>/returns>
        public async Task<double> Read_Voltage(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {

                var resultStr = await WriteRead("MEASure:VOLTage:DC?\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
        #endregion
    }
}