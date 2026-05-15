using Common.Attributes;
using DeviceCommand.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Common.Attributes.CommandAttribute;

namespace DeviceCommand.Device
{
    /// <summary>
    /// 直流电源——三通道可编程直流电源设备（型号：IT-N6322B）
    /// </summary>
    [Command]
    [DeviceCategory("三通道可编程直流电源")]
    public class IT_N6322B : Tcp
    {
        /// <summary>
        /// 信号量锁
        /// </summary>
        public SemaphoreSlim semaphoreSlimLock { get; set; } = new(1, 1);

        /// <summary>
        /// 电源状态变更事件（携带通道号和目标状态）
        /// </summary>
        public Action<ChannelList_Enum, bool>? PowerStateChanged;

        /// <summary>
        /// 通道枚举
        /// </summary>
        public enum ChannelList_Enum
        {
            /// <summary>
            /// 通道1
            /// </summary>
            CH1 = 1,
            /// <summary>
            /// 通道2
            /// </summary>
            CH2 = 2,
            /// <summary>
            /// 通道3
            /// </summary>
            CH3 = 3
        }


        /// <summary>
        /// 复位仪器
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task SYSTem_PRESet(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"SYST:REM\n", ct);
                await SendAsync($"OUTP:ALL OFF\n", ct);
                // 分别触发事件，让外层 UI 去停监控
                PowerStateChanged?.Invoke(ChannelList_Enum.CH1, false);
                PowerStateChanged?.Invoke(ChannelList_Enum.CH2, false);
                PowerStateChanged?.Invoke(ChannelList_Enum.CH3, false);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
        /// <summary>
        /// 切换为远程模式 三通道可编程直流电源
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_RemoteMode(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"SYST:REM\n", ct);
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
        /// 切换为本地模式
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_LocalMode(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"SYST:LOC\n", ct);
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
        /// 设置指定通道当前输出电压
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="voltage">电压值</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_OutputVoltage(ChannelList_Enum channel, float voltage, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"VOLT {voltage},(@{(int)channel})\n", ct);
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
        /// 设置指定通道的输出电流
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="current">电流值</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_OutputCurrent(ChannelList_Enum channel, float current, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURR {current},(@{(int)channel})\n", ct);
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
        /// 设置指定通道的最大电压
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="voltage">电压值</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_MaxVoltage(ChannelList_Enum channel, float voltage, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"VOLT:LIM {voltage},(@{(int)channel})\n", ct);
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
        /// 设置指定通道的最大电流
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="current">电流值</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_MaxCurrent(ChannelList_Enum channel, float current, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURR:LIM {current},(@{(int)channel})\n", ct);
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
        /// 设置电源指定通道最大功率
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="power">功率</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_MaxPower(ChannelList_Enum channel, float power, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"POW:LIM {power},(@{(int)channel})\n", ct);
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
        /// 返回指定通道的实际输出电压
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task<double> GET_ActualVoltage(ChannelList_Enum channel,CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                var resultStr = await WriteRead($"MEAS:VOLT?(@{(int)channel})\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
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
        /// 设置指定通道的实际输出电流
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task<double> GET_ActualCurrent(ChannelList_Enum channel,CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                var resultStr = await WriteRead($"MEAS:CURR? (@{(int)channel})\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
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




        ///// <summary>
        ///// 设置指定通道输出的电压和电流值
        ///// </summary>
        ///// <param name="channel">通道</param>
        ///// <param name="voltage">电压值</param>
        ///// <param name="current">电流值</param>
        ///// <param name="ct">支持中途取消发送指令</param>
        ///// <returns></returns>
        //public async Task Set_CurrentAndVoltage(ChannelList_Enum channel, float voltage, float current, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        await SendAsync($"APPL {voltage},{current},(@{(int)channel})\n", ct);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}

        ///// <summary>
        ///// 返回指定通道电源输出的实际电压值
        ///// </summary>
        ///// <param name="channel">通道</param>
        ///// <param name="ct">取消令牌</param>
        ///// <returns>返回值是：定通道电源输出的实际电压值 (Amps)</returns>
        //public async Task<string> Query_Voltage(ChannelList_Enum channel, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        return await WriteRead($"MEAS:VOLT?,(@{(int)channel})\r\n", "\n", ct: ct);
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}

        ///// <summary>
        ///// 返回指定通道电源输出的实际电流值
        ///// </summary>
        ///// <param name="channel">通道</param>
        ///// <param name="ct">取消令牌</param>
        ///// <returns>返回值是：设置的电流值 (Amps)</returns>
        //public async Task<string> Query_Current(ChannelList_Enum channel, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        return await WriteRead($"MEAS:CURR?,(@{(int)channel})\r\n", "\n", ct: ct); // 使用缩写
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}

        /// <summary>
        /// 设置源指定通道的开启
        /// </summary>
        public async Task Set_Power_ON(ChannelList_Enum channel, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"OUTP ON,(@{(int)channel})\n", ct);
                // ✅ 新增：通知外部该通道已开启
                PowerStateChanged?.Invoke(channel, true);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { semaphoreSlimLock.Release(); }
        }

        /// <summary>
        /// 设置源指定通道的关闭
        /// </summary>
        public async Task Set_Power_OFF(ChannelList_Enum channel, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"OUTP OFF,(@{(int)channel})\n", ct);
                // ✅ 新增：通知外部该通道已关闭
                PowerStateChanged?.Invoke(channel, false);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { semaphoreSlimLock.Release(); }
        }

        /// <summary>
        /// 设置源指定通道的关闭
        /// </summary>
        public async Task Set_AllPower_OFF(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"OUTP:ALL OFF\n", ct);
                //通知外部该通道已关闭
                PowerStateChanged?.Invoke(ChannelList_Enum.CH1, false);
                PowerStateChanged?.Invoke(ChannelList_Enum.CH2, false);
                PowerStateChanged?.Invoke(ChannelList_Enum.CH3, false);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { semaphoreSlimLock.Release(); }
        }
    }
}