using Common.Attributes;
using DeviceCommand.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static Common.Attributes.CommandAttribute;
using static System.Formats.Asn1.AsnWriter;

namespace DeviceCommand.Device
{
    /// <summary>
    /// 示波器（型号：RTM3004）
    /// </summary>
    [Command]
    [DeviceCategory("示波器")]
    public class RTM3004 : Tcp
    {
        /// <summary>
        /// 信号量锁
        /// </summary>
        public SemaphoreSlim semaphoreSlimLock { get; set; } = new(1, 1);

        #region 枚举定义（严格遵循手册规范）
        /// <summary>
        /// 单个通道名称枚举
        /// </summary>
        public enum ChannelName_Single_Enum
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
            CH3 = 3,
            /// <summary>
            /// 通道4
            /// </summary>
            CH4 = 4
        }

        /// <summary>
        /// 测量项枚举，RTM3004支持8个独立的测量项
        /// </summary>
        public enum Measurement_Enum
        {
            /// <summary>测量项1</summary>
            MEAS1 = 1,
            /// <summary>测量项2</summary>
            MEAS2 = 2,
            /// <summary>测量项3</summary>
            MEAS3 = 3,
            /// <summary>测量项4</summary>
            MEAS4 = 4,
            /// <summary>测量项5</summary>
            MEAS5 = 5,
            /// <summary>测量项6</summary>
            MEAS6 = 6,
            /// <summary>测量项7</summary>
            MEAS7 = 7,
            /// <summary>测量项8</summary>
            MEAS8 = 8
        }

        /// <summary>
        /// 测量类型
        /// </summary>
        public enum Measurement_Type
        {
            /// <summary>频率</summary>
            FREQuency,
            /// <summary>周期</summary>
            PERiod,
            /// <summary>峰峰值</summary>
            PEAK,
            /// <summary>高电平</summary>
            HIGH,
            /// <summary>低电平</summary>
            LOW,
            /// <summary>正脉宽</summary>
            PPWidth,
            /// <summary>负脉宽</summary>
            NPWidth,
            /// <summary>周期RMS</summary>
            RMS
        }
        /// <summary>
        /// 门控状态
        /// </summary>
        public enum Gate_State
        {
            ON,
            OFF
        }

        /// <summary>
        /// 通道耦合模式枚举
        /// </summary>
        public enum CouplingMode
        {
            /// <summary>直流限制耦合</summary>
            DCLimit,
            /// <summary>交流限制耦合</summary>
            ACLimit,
            /// <summary>接地耦合，用于参考电平</summary>
            GND,
            /// <summary>直流耦合</summary>
            DC
        }

        /// <summary>
        /// 采集状态（手册 ACQuire:STATE 支持值）
        /// </summary>
        public enum AcquisitionState
        {
            /// <summary>运行中，持续采集波形</summary>
            RUN,
            /// <summary>采集完成后停止采集</summary>
            STOPping,
            /// <summary>采集完成（设置：不可用）</summary>
            COMPlete,
            /// <summary>立即中断当前采集</summary>
            BREak
        }

        /// <summary>
        /// 带宽限制（手册:BANdwidth 支持值）
        ///</summary>
        public enum BandwidthLimit
        {
            /// <summary>使用全部带宽，无限制</summary>
            FULL,
            /// <summary>频率限制为20 MHz，高频信号被移除以降低噪声</summary>
            B20
        }

        /// <summary>
        /// 触发模式（手册 TRIGger:A:MODe 支持值）
        /// </summary>
        public enum TriggerMode
        {
            /// <summary>自动模式，无触发时自动扫描</summary>
            AUTO,
            /// <summary>正常模式，仅在触发时采集</summary>
            NORMal,
        }

        /// <summary>
        /// 触发类型（对应手册 TRIGger:A:TYPE 支持值）
        /// </summary>
        public enum TriggerType
        {
            /// <summary>边沿触发</summary>
            EDGE,
            /// <summary>脉宽触发</summary>
            WIDTh,
            /// <summary>视频触发</summary>
            TV,
            /// <summary>总线触发（需串行总线协议选件）</summary>
            BUS,
            /// <summary>逻辑/码型触发</summary>
            LOGic,
            /// <summary>电源线触发</summary>
            LINE,
            /// <summary>上升/下降时间（跳变）触发</summary>
            RISetime,
            /// <summary>欠幅脉冲触发</summary>
            RUNT
        }

        /// <summary>
        /// 触发斜率/边沿枚举
        /// </summary>
        public enum Slope_Enum
        {
            /// <summary>上升沿触发，检测电压正向变化</summary>
            POSitive,
            /// <summary>下降沿触发，检测电压负向变化</summary>
            NEGative,
            /// <summary>上升和下降沿都触发</summary>
            EITHer
        }

        /// <summary>
        /// 光标测量类型
        /// </summary>
        public enum CursorFucation_Enum
        {
            /// <summary>水平</summary>
            HORizontal,
            /// <summary>垂直</summary>
            VERTical,
            /// <summary>垂直水平</summary>
            HVERtical
        }
        #endregion

        //罗德与施瓦茨RTM3000示波器没有专门的"切换本地/远程"SCPI命令,自动切换

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
                await SendAsync($"*RST\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        ///// <summary>
        ///// 复位通讯
        ///// </summary>
        ///// <param name="channelName">通道名称</param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //public async Task Comm_PRESet(ChannelName_Single_Enum channelName, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        await SendAsync($"*CLS\n", ct);
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}

        /// <summary>
        /// 指定通道的电流源
        /// </summary>
        /// <param name="channelName">通道名称</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Channel_CurrentSource(ChannelName_Single_Enum channelName, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"POWer:SOURce:CURRent CH{(int)channelName}\n", ct); // 手册 POWer 命令规范
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 指定通道的电压源
        /// </summary>
        /// <param name="channelName">通道</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Channel_VoltageSource(ChannelName_Single_Enum channelName, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"POWer:SOURce:VOLTage CH{(int)channelName}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        #region 测量
        /// <summary>
        /// 启动自动测量(AUTO/NORMal)
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Start_AutoMeasurement(Measurement_Enum measurement, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:ON\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 停止自动测量
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Stop_AutoMeasurement(Measurement_Enum measurement, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:OFF\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 指定测量的通道
        /// </summary>
        /// <param name="channelName">通道</param>
        /// <param name="measurement">测试项</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_Source(ChannelName_Single_Enum channelName, Measurement_Enum measurement, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:SOURce CH{(int)channelName}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 指定测量项的类型
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="measurement_Type">测试类型</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_Type(Measurement_Enum measurement, Measurement_Type measurement_Type, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:MAIN {measurement_Type}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }


        /// <summary>
        /// 指定测量项门控的开关
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="gate_State">门控的开关/param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_Gate(Measurement_Enum measurement, Gate_State gate_State, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:GATE {gate_State}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 指定测量项门控的绝对起始时间(单位默认为s)
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="start_time">门控的起始时间/param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_GateStartTime(Measurement_Enum measurement, double start_time, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:GATE:ABSolute:STARt {start_time}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }



        /// <summary>
        /// 指定测量项门控的绝对停止时间(单位默认为s)
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="stop_time">门控的停止时间(/param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_GateStopTime(Measurement_Enum measurement, double stop_time, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"MEASurement{(int)measurement}:GATE:ABSolute:STOP {stop_time}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }



        /// <summary>
        /// 返回指定测量项的实际测量结果
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="ct"></param>
        /// <returns>指定测试项该类型测量结果</returns>
        public async Task<double> Return_MeasureACTualResult(Measurement_Enum measurement, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                string resultStr = await WriteRead($"MEASurement{(int)measurement}:RESult:ACTual?\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 返回指定测量项的最大测量结果
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="ct"></param>
        /// <returns>最大测量结果</returns>
        public async Task<double> Return_Max_Measure_Result(Measurement_Enum measurement, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                string resultStr = await WriteRead($"MEASurement{(int)measurement}:RESult:PPEak?\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 返回指定测量项的最小测量结果
        /// </summary>
        /// <param name="measurement">测试项</param>
        /// <param name="ct"></param>
        /// <returns>最大测量结果</returns>
        public async Task<double> Return_Min_Measure_Result(Measurement_Enum measurement, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {

                string resultStr = await WriteRead($"MEASurement{(int)measurement}:RESult:NPEak?\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }


        /// <summary>
        /// 用于设置所有测试项的参考电平的高电平的百分比
        /// </summary>
        /// <param name="level">参考电平的百分比</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_RELativeUPPer(float level, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"REFLevel:RELative:UPPer {level}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 用于设置所有测试项的参考电平的中间电平的百分比
        /// </summary>
        /// <param name="level">参考电平的百分比</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_RELativeMIDDle(float level, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"REFLevel:RELative:MIDDle {level}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 用于设置所有测试项的参考电平的低电平的百分比
        /// </summary>
        /// <param name="level">参考电平的百分比</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Measurement_RELativeLOWer(float level, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"REFLevel:RELative:LOWer {level}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
        #endregion

        #region 光标设置
        /// <summary>
        /// 打开光标
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>最大测量结果</returns>
        public async Task Open_CURSor(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURSor:STATe ON\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 关闭光标
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Close_CURSor(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURSor:STATe OFF\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 关闭光标
        /// </summary>
        /// <param name="fucation_Enum">光标类型</param>
        /// <param name="ct"></param>
        /// <returns>最大测量结果</returns>
        public async Task Set_CURSorFuction(CursorFucation_Enum fucation_Enum, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURSor:FUNCtion {fucation_Enum}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置光标测量源
        /// </summary>
        /// <param name=" channel">光标测量源</param>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        public async Task Set_CURSorSource(ChannelName_Single_Enum channel, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                // 文档说明<m>后缀无关，可固定为1或省略
                await SendAsync($"CURSor:SOURce {(int)channel}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 自动设置光标线位置（CURSor:SWAVe）
        /// 根据所选光标类型，将光标线自动定位到波形典型点：
        /// - 水平（电压）测量：光标线置于波形上下峰值
        /// - 垂直（时间）测量：光标线置于连续两个同极性脉冲边沿
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        public async Task Set_CURSorAutoSet(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                // 文档说明<m>后缀无关，固定使用CURSor1:SWAVe
                await SendAsync("CURSor:SWAVe\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置垂直光标X1位置（CURSor:X1Position）
        /// 用于指定X轴（时间/FFT频率）上第一条垂直光标线的位置
        /// </summary>
        /// <param name="xPosition">X1位置值，默认单位为秒(s)</param>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        public async Task Set_CURSorX1Position(double xPosition, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                // <m>后缀无关，固定使用CURSor1
                await SendAsync($"CURSor:X1Position {xPosition}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置垂直光标X2位置（CURSor:X2Position）
        /// 用于指定X轴（时间/FFT频率）上第二条垂直光标线的位置
        /// </summary>
        /// <param name="xPosition">X2位置值，默认单位为秒(s)</param>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        public async Task Set_CURSorX2Position(double xPosition, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURSor:X2Position {xPosition}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置水平光标Y1位置（CURSor:Y1Position）
        /// 用于指定Y轴（电压/电流/FFT电平）上第一条水平光标线的位置
        /// </summary>
        /// <param name="yPosition">Y1位置值</param>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        public async Task Set_CURSorY1Position(double yPosition, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURSor:Y1Position {yPosition}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置水平光标Y2位置（CURSor:Y2Position）
        /// 用于指定Y轴（电压/电流/FFT电平）上第二条水平光标线的位置
        /// </summary>
        /// <param name="yPosition">Y2位置值</param>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        public async Task Set_CURSorY2Position(double yPosition, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CURSor:Y2Position {yPosition}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 查询垂直光标时间差（CURSor:XDELTa:VALue?）
        /// 返回两条垂直光标线之间的时间差(Δt)，默认单位为秒(s)
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>时间差结果（字符串格式，可后续解析为double）</returns>
        public async Task<double> Query_CURSorXDeltaValue(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                var resultStr = await WriteRead($"CURSor:XDELTa:VALue?\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 查询水平光标Y轴差值（CURSor:YDELTa:VALue?）
        /// 返回两条水平光标线在Y方向上的差值（如电压差、电流差或FFT电平差）
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>Y轴差值结果（字符串格式，可后续解析为double）</returns>
        public async Task<double> Query_CURSorYDeltaValue(CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                // <m>后缀无关，固定使用CURSor1
                var resultStr = await WriteRead($"CURSor:YDELTa:VALue?\r\n", "\n", ct: ct);
                return double.Parse(resultStr.Trim());
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
        #endregion

        #region 触发
        /// <summary>
        /// 指定触发的通道 (AUTO/NORMal)
        /// </summary>
        /// <param name="channelName">通道</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_ATrigger_SOURce(ChannelName_Single_Enum channelName, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"TRIGger:A:SOURce CH{(int)channelName}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置触发的模式 (AUTO/NORMal)
        /// </summary>
        /// <param name="mode">触发模式</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_ATrigger_Mode(TriggerMode mode, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                string modeStr = mode.ToString().ToUpper();
                await SendAsync($"TRIGger:A:MODE {modeStr}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置触发的类型
        /// </summary>
        /// <param name="type">触发类型</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_ATrigger_Type(TriggerType type, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                string modeStr = type.ToString().ToUpper();
                await SendAsync($"TRIGger:A:TYPE {modeStr}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置触发的触发电平(取决于垂直标度)
        /// </summary>
        /// <param name="channelName">通道</param>
        /// <param name="level">阈值电压</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_ATrigger_Level(ChannelName_Single_Enum channelName, double level, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {                
                await SendAsync($"TRIGger:A:LEVel{(int)channelName} {level}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
        #endregion

        #region 设置通道配置
        /// <summary>
        /// 设置指定通道的输入耦合模式
        /// </summary>
        /// <param name="channelName">通道名称</param>
        /// <param name="couplingMode">耦合模式</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_ChannelCoupling(ChannelName_Single_Enum channelName, CouplingMode couplingMode, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                string modeStr = couplingMode.ToString().ToUpper();
                await SendAsync($"CHANnel{(int)channelName}:COUPling{couplingMode}\n", ct); // 直接使用通道枚举值（如 CHANnel 1:COUPLING AC）
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置指定通道的硬件带宽限制
        /// </summary>
        /// <param name="channelName">通道名称</param>
        /// <param name="bandwidth">带宽限制</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_ChannelBandwidth(ChannelName_Single_Enum channelName, BandwidthLimit bandwidth, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                string bwStr = $"{(int)bandwidth}E6";
                await SendAsync($"CHANnel{(int)channelName}:BANDwidth{bwStr}\n", ct); // 如 CHANnel 1:BANDwidth FULL
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 启用指定通道
        /// </summary>
        /// <param name="channelName">通道名称</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Start_Channel(ChannelName_Single_Enum channelName, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CHANnel{(int)channelName}:STATe ON\n", ct); // 手册标准通道启用命令
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 关闭指定通道
        /// </summary>
        /// <param name="channelName">通道名称</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Stop_Channel(ChannelName_Single_Enum channelName, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CHANnel{(int)channelName}:STATe OFF\n", ct); // 手册标准通道启用命令
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
        #endregion

        #region 设置水平和垂直刻度配置
        /// <summary>
        /// 设置指定通道的垂直标度 (Volts/Div)
        /// </summary>
        /// <param name="channelName">通道名称</param>
        /// <param name="scale">标度值（单位默认：V/div 设置mv则为0.1v）</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_Channel_Scale(ChannelName_Single_Enum channelName, float scale, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"CHANnel{(int)channelName}:SCALe {scale}\n", ct); // 如 CH1:SCALE 1.0（单位 V/div）
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        /// <summary>
        /// 设置所有通道及数学波形的水平比例尺(每格时间，秒/格)
        /// </summary>
        /// <param name="scale">时基标度（单位：s/div）</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Set_AllChannel_Timebase_Scale(double scale, CancellationToken ct = default)
        {
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                await SendAsync($"TIMebase:SCALe {scale}\n", ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }

        ///// <summary>
        ///// 设置水平触发位置 (相对于屏幕中心的偏移，百分比)
        ///// </summary>
        ///// <param name="position">位置百分比</param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //public async Task Set_Timebase_Position(double position, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        await SendAsync($"TIMebase:POSition {position}\n", ct); // 位置单位为百分比
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}
        #endregion

        ///// <summary>
        ///// 保存波形数据（保存全通道到CSV文件）
        ///// </summary>
        ///// <param name="channelName">指定通道（默认：D:/BDUWAVE）</param>
        ///// <param name="filePath">保存路径（默认：D:/BDUWAVE）</param>
        ///// <param name="fileName">保存波形文件名（默认：WAVE_当前时间.PNG)</param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //public async Task Save_Waveform(ChannelName_Single_Enum channelName, string filePath, string fileName, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        if (!Directory.Exists(filePath))
        //        {
        //            filePath = $"D:/WAVE";
        //            Directory.CreateDirectory(filePath);
        //        }
        //        if (string.IsNullOrEmpty(fileName))
        //        {
        //            fileName = $"WAVE_{DateTime.Now.ToString()}";
        //        }

        //        await SendAsync($"EXPort:WAVeform:SOURce CH{(int)channelName}\n", ct); // 设置待导出的波形源
        //        await SendAsync($"EXPort:WFMSave:DESTination{filePath}\n", ct); // 设置待导出的目录
        //        await SendAsync("FORMAT CSV\n", ct); // 设置文件格式
        //        await SendAsync($"EXPort:WAVeform:NAME\"{fileName}\"\n", ct); //设置文件名字
        //        await SendAsync($"EXPort:WAVeform:SAVE\n", ct);// 执行保存操作
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}

        ///// <summary>
        ///// 保存屏幕截图（指定格式为PNG）
        ///// </summary>
        ///// <param name="filePath">保存路径（默认：D:/BDUSCREEN）</param>
        ///// <param name="fileName">保存波形文件名（默认：SCREEN_当前时间.PNG)</param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //public async Task Save_Screen_Image(string filePath, string fileName, CancellationToken ct = default)
        //{
        //    await semaphoreSlimLock.WaitAsync(ct);
        //    try
        //    {
        //        if (!Directory.Exists(filePath))
        //        {
        //            filePath = $"D:/WAVE";
        //            Directory.CreateDirectory(filePath);
        //        }
        //        if (string.IsNullOrEmpty(fileName))
        //        {
        //            fileName = $"WAVE_{DateTime.Now.ToString()}";
        //        }
        //        await SendAsync($"EXPort:SCRSave:DESTination{filePath}\n", ct); // 设置待导出的目录
        //        await SendAsync("FORMAT PNG\n", ct); // 设置文件格式
        //        await SendAsync($"MMEMory:NAME\"{fileName}\"\n", ct); //设置文件名字
        //        await SendAsync($"HCOPy:IMMediate\n", ct);// 执行保存操作
        //    }
        //    finally
        //    {
        //        semaphoreSlimLock.Release();
        //    }
        //}

        /// <summary>
        /// 发送自定义SCPI命令
        /// </summary>
        /// <param name="command">SCPI命令字符串</param>
        /// <param name="ct"></param>
        /// <returns>命令执行结果</returns>
        public async Task<string> Send_CustomCommand(string command, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));
            await semaphoreSlimLock.WaitAsync(ct);
            try
            {
                return await WriteRead($"{command}\r", "\n", ct: ct);
            }
            finally
            {
                semaphoreSlimLock.Release();
            }
        }
    }
}