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
    /// 直流电源——阿美泰克直流电源
    /// </summary>
    [Command]
    [DeviceCategory("阿美泰克直流电源")]
    public class AMETEKSGX : Tcp
    {
        /// <summary>
        /// 信号量锁
        /// </summary>
        public SemaphoreSlim semaphoreSlimLock { get; set; } = new(1, 1);

        /// <summary>
        /// 电源开关状态变更事件（供 UI 层订阅同步状态）
        /// </summary>
        public Action<bool>? PowerStateChanged;



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
                await SendAsync($"SYSTem:LOCAL OFF\n", ct);
                await Set_Power_OFF_NoLock(ct);
                PowerStateChanged?.Invoke(false);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        private Task Set_Power_OFF_NoLock(CancellationToken ct = default)
        {
            return SendAsync($"OUTPut:STATe OFF\n", ct);

        }



        /// <summary>
        /// 切换为远程模式 阿美泰克直流电源
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_RemoteMode(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct); // 已修复：补全锁等待
            try
            {
                await SendAsync($"SYSTem:LOCAL OFF\n", ct);
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
                await SendAsync($"SYSTem:LOCAL ON\n", ct);
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
        /// 设定输出电压值
        /// </summary>
        /// <param name="voltage">输出电压</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_OutputVoltage(int voltage, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"SOURce:VOLTage {voltage}\n", ct);
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

/*        /// <summary>
        /// 返回AMETEK自身的电压值
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>返回值是：AMETEK自身的电流值 (Amps)</returns>
        public async Task<string> QueryVoltage(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                //查询语法与命令语法相同，只需在命令后添加“？”即可
                return await WriteRead($"SOURce:VOLTage?\r\n", "\n", ct: ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }*/

        /// <summary>
        /// 设置输出电流
        /// </summary>
        /// <param name="current">输出电流</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_OutputCurrent(float current, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {              
                await SendAsync($"SOURce:CURRent {(uint)current}\r\n", ct);
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
        /// 获取实际输出功率
        /// </summary>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task<double> Get_ActualPOWer(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                var resultStr = await WriteRead($"MEASure:POWer?\r\n", "\n", ct: ct);
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
        /// 设置最大电流(不能超过1200A)
        /// </summary>
        /// <param name="current">输出频率</param>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_MaxCurrent(float current, CancellationToken ct = default)
        {
            if (current > 1200)
            {
                throw new Exception("超过规定的最大电流");
            }
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"SOURce:CURRent:LIMit {current}\n", ct);
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
        /// 设置电源的开启
        /// </summary>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_Power_ON(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                //1/0 或 ON/OFF都可以
                await SendAsync($"OUTPut:STATe ON\r\n", ct);
                PowerStateChanged?.Invoke(true);
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
        /// 设置电源的关闭
        /// </summary>
        /// <param name="ct">支持中途取消发送指令</param>
        /// <returns></returns>
        public async Task Set_Power_OFF(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"OUTPut:STATe OFF\n", ct);
                PowerStateChanged?.Invoke(false);
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
    }
}