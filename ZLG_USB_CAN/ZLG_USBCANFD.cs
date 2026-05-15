using Common.Attributes;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks; // 补充引用
using ZLGCANAPI;
using static Common.Attributes.CommandAttribute;

namespace ZLG_USB_CAN
{
    //DbcMessageInfo 类来存储 ID 和其对应的信号名列表
    public class DbcMessageInfo
    {
        public uint MessageId { get; set; }
        public List<string> SignalNames { get; set; } = new List<string>();
    }
    public enum Can_Type
    {
        CAN=0,
        CAN_Fd=1
    }

    public enum Signal_Name
    {
        DigitalCurrent,
        AnalogCurrent
    }
    /// <summary>
    /// ZLG_USBCANFD 设备管理类 每个操作都有执行状态判断，返回1表示成功，0表示失败
    /// </summary>
    [Command]
    [DeviceCategory("ZLGCAN卡驱动")]
    public class ZLG_USBCANFD : IDisposable
    {
        #region 字段
        // 设备句柄
        public static IntPtr devHandle = IntPtr.Zero;
        public static IntPtr[] chnHandle = new IntPtr[10];
        public static int channel_index = 0;
        public static Can_Type Initcan_Type;
        public static uint DBCHandle = 0;                  // DBC句柄
        private static Task<double> _receiveTask; // 替换原有的 Thread
        private static uint _isMerge = 0;
        private static uint _isBusload = 0;

        // ================= 新增：丢包率统计相关字段 =================
        private const uint ERR_FIFO_OVERFLOW = 0x0001;
        private const uint ERR_BUFFER_OVERFLOW = 0x0800;

        private static CancellationTokenSource _lossMonitorCts;
        private static Task _lossMonitorTask;

        private static long _fifoOverflowCount = 0;
        private static long _bufferOverflowCount = 0;
        private static long _totalReceivedFrames = 0;    //累计成功接收处理的帧数
        private static bool _lastFifoStatus = false;
        private static bool _lastBufferStatus = false;

        private static readonly object _lockError = new object();
        // =========================================================
        #endregion

        #region 1. 设备管理方法
        public static bool ConnectFlag { get; set; } = false;
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="deviceType">设备类型，详见头文件zlgcan.h中的宏定义，如 ZLGCAN.ZCAN_USBCANFD_200U</param>
        /// <param name="deviceIndex">设备索引号，多一个设备索引号加一</param>
        /// <returns>成功返回1，失败返回0</returns>
        public static uint OpenDevice(uint deviceType, uint deviceIndex)
        {
            if (ConnectFlag)
            {
                ConnectFlag = false;
                CloseDevice();
            }
            devHandle = ZLGCAN.ZCAN_OpenDevice(deviceType, deviceIndex, 0);
            if (devHandle == IntPtr.Zero)
            {
                Debug.WriteLine("打开设备失败");
                return 0;//打开设备失败
            }
            ConnectFlag = true;
            Console.WriteLine("打开通道成功");
            return 1;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>成功返回1，失败返回0</returns>
        public static uint CloseDevice()
        {
            ConnectFlag = false;

            // ===== 新增：关闭设备前，先停止监控 =====
            StopLossMonitor();

            Task.Delay(100);// 等待线程退出
         
            if (devHandle == IntPtr.Zero)
            {
                Debug.WriteLine("设备未打开");
                return 0;
            }
            uint ret = ZLGCAN.ZCAN_CloseDevice(devHandle);
            if (ret != 1)
            {
                /*Debug.WriteLine("关闭设备失败");*/
                return 0;
            }
            devHandle = IntPtr.Zero;
            /*Debug.WriteLine("关闭设备成功");*/
            return 1;
        }

        /// <summary>
        /// 检测设备是否在线
        /// </summary>
        /// <returns>0-ERR, 1-OK, 2-ONLINE, 3-OFFLINE, 4-UNSUPPORTED, 5-BUFFER_TOO_SMALL</returns>
        public static uint IsDeviceOnline()
        {
            if (devHandle == IntPtr.Zero)
            {
                Debug.WriteLine("设备句柄无效");
                return 0;
            }
            uint status = ZLGCAN.ZCAN_IsDeviceOnLine(devHandle);
            string statusText = status switch
            {
                0 => "ERR",
                1 => "OK",
                2 => "ONLINE",
                3 => "OFFLINE",
                4 => "UNSUPPORTED",
                5 => "BUFFER_TOO_SMALL",
                _ => "UNKNOWN"
            };
            Debug.WriteLine($"设备状态: {statusText}");
            return status;
        }
        #endregion

        #region 2. 通道配置方法
        /// <summary>
        /// 完整初始化并启动通道（合并上述步骤）
        /// </summary>
        /// <param name="chn_idx">通道索引</param>
        /// <param name="can_Type">CAN类型</param>
        /// <param name="arbitrationBaudRate">仲裁域波特率</param>
        /// <param name="dataBaudRate">数据域波特率</param>
        /// <param name="terminalResistance">终端电阻使能</param>
        /// <param name="mode">工作模式</param>
        /// <returns>成功返回1，失败返回0</returns>
        public static IntPtr Init_chn_USB(
            int chn_idx,
            Can_Type can_Type,
            uint arbitrationBaudRate,
            uint dataBaudRate,
            uint terminalResistance,
            uint mode)
        {
            IntPtr temp = IntPtr.Zero;
            uint ret;           // 返回值

            
            // 仲裁域波特率 {0}/canfd_abit_baud_rate 0为设备号 ，默认为0
            string path = String.Format("{0}/canfd_abit_baud_rate", chn_idx);
            ret = ZLGCAN.ZCAN_SetValue(devHandle, path, $"{arbitrationBaudRate}");       // 500k
            if (ret != 1)
            {
                Console.WriteLine("设置仲裁域波特率失败");

                return IntPtr.Zero;
            }

            // 数据域波特率
            path = String.Format("{0}/canfd_dbit_baud_rate", chn_idx);
            ret = ZLGCAN.ZCAN_SetValue(devHandle, path, $"{dataBaudRate}");      // 2M
            if (ret != 1)
            {
                Console.WriteLine("设置数据域波特率失败");

                return IntPtr.Zero;
            }

            // 终端电阻
            path = String.Format("{0}/initenal_resistance", chn_idx);
            string resistance = $"{terminalResistance}";//1-使能 0-禁能
            ret = ZLGCAN.ZCAN_SetValue(devHandle, path, resistance);
            if (ret != 1)
            {
                Console.WriteLine("设置终端电阻失败");

                return IntPtr.Zero;
            }

            // 初始化通道
            ZLGCAN.ZCAN_CHANNEL_INIT_CONFIG InitConfig = new ZLGCAN.ZCAN_CHANNEL_INIT_CONFIG(); // 结构体
            InitConfig.can_type = (uint)can_Type;                // 1 - CANFD (USBCANFD 只能选择CANFD模式)
            InitConfig.config.canfd.mode = (byte)mode;       // 0 - 正常模式，1 - 只听模式

            IntPtr P2InitConfig = Marshal.AllocHGlobal(Marshal.SizeOf(InitConfig));
            Marshal.StructureToPtr(InitConfig, P2InitConfig, true);                         // 转成指针
            chnHandle[chn_idx] = ZLGCAN.ZCAN_InitCAN(devHandle, (uint)chn_idx, P2InitConfig);
            Marshal.FreeHGlobal(P2InitConfig);                                              // 释放内存
            if (chnHandle[chn_idx] == IntPtr.Zero)
            {
                Console.WriteLine("初始化通道失败");

                return IntPtr.Zero;
            }
            channel_index = chn_idx;
            Initcan_Type = can_Type;
            Console.WriteLine("初始化通道成功");

            //// 设置滤波
            //value = "0";            // 0-标准帧 1-扩展帧
            // path = String.Format("{0}/filter_mode", chn_idx);
            // ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, value);

            // value = "0x731";        // 起始 ID
            // path = String.Format("{0}/filter_start", chn_idx);
            // ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, value);

            // value = "0x7DF";        // 结束 ID
            // path = String.Format("{0}/filter_end", chn_idx);
            // ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, value);

            // value = "0";            // 使能滤波
            // path = String.Format("{0}/filter_ack", chn_idx);
            // ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, value);
            // if (ret != 1)
            // {
            //     Console.WriteLine("使能滤波失败");
            //     Console.ReadKey();
            //     return IntPtr.Zero;
            // }

/*            if (_isBusload == 1)    // 通道利用率
            {
                // 通道利用率上报
                path = String.Format("{0}/set_bus_usage_enable", chn_idx);
                ret = ZLGCAN.ZCAN_SetValue(devHandle, path, "1");

                // 总线利用率上报周期
                path = String.Format("{0}/set_bus_usage_period", chn_idx);
                ret = ZLGCAN.ZCAN_SetValue(devHandle, path, "500");
            }*/

            //打开通道
            uint Ret = ZLGCAN.ZCAN_StartCAN(chnHandle[chn_idx]);
            if (Ret != 1)
            {
                Console.WriteLine("打开通道失败");

                return IntPtr.Zero;
            }
            Console.WriteLine("打开通道成功");
            //在通道真正打开成功后，再启动丢包监控
            StartLossMonitor();
            return 1;
        }


        /// <summary>
        /// 复位/停止通道
        /// </summary>
        /// <param name="channel_Index">通道句柄</param>
        /// <returns>成功返回1，失败返回0</returns>
        public static uint ResetChannel(int channel_Index)
        {
            // ===== 新增：复位通道前，停止监控 =====
            StopLossMonitor();
            if (chnHandle[channel_Index] == IntPtr.Zero)
            {
                Debug.WriteLine("通道句柄无效");
                return 0;
            }
            uint ret = ZLGCAN.ZCAN_ResetCAN(chnHandle[channel_Index]);
            if (ret != 1)
            {
                Debug.WriteLine("复位通道失败");
                return 0;
            }
            Debug.WriteLine("复位通道成功");
            return 1;
        }
        public enum Signal_Name
        {
            AnalogCurrent,
            DigitalCurrent

        }


        #endregion



        #region 7.DBC方法
        /// <summary>
        /// 加载DBC文件
        /// </summary>
        /// <summary>
        /// 加载DBC
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="channel"></param>
        /// <param name="databaseID"></param>
        /// <returns></returns>


        [Browsable(false)]
        public static uint LoadDBC(string dbcPath)
        {
            // 1. 如果之前已经加载过，先尝试清理旧句柄（可选，取决于ZDBC库的释放机制）
            if (DBCHandle != IntPtr.Zero)
            {
                // 假设 ZDBC 有对应的释放函数，如果没有则注释掉
                // ZDBC.ZDBC_Release(DBCHandle); 
                DBCHandle = (uint)IntPtr.Zero;
            }

            // 2. 初始化 DBC 句柄
            DBCHandle = ZLGCANAPI.ZDBC.ZDBC_Init();
            if (DBCHandle == IntPtr.Zero)
            {
                Debug.WriteLine("DBC 句柄初始化失败");
                return 0;
            }

            // 3. 将路径字符串转换为非托管内存指针
            IntPtr pPath = Marshal.StringToHGlobalAnsi(dbcPath);

            try
            {
                // 4. 调用加载函数
                bool result = ZDBC.ZDBC_LoadFile(DBCHandle, pPath);

                if (!result)
                {
                    Debug.WriteLine("加载 DBC 文件失败: " + dbcPath);
                    // 加载失败时通常建议清理句柄
                    DBCHandle = (uint)IntPtr.Zero;
                    return 0;
                }

                Debug.WriteLine("加载 DBC 文件成功");
                return 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("加载过程中发生异常: " + ex.Message);
                return 0;
            }
            finally
            {
                // 5. 无论成功与否，必须释放非托管内存
                Marshal.FreeHGlobal(pPath);
            }
        }


        /// <summary>
        /// 解析并获取所有消息的 ID 和 信号名
        /// </summary>
        /// <returns>包含 ID 和 信号名列表的对象集合</returns>
        public static List<DbcMessageInfo> GetAllMessagesAndSignals()
        {
            var resultList = new List<DbcMessageInfo>();
            if (DBCHandle == IntPtr.Zero) return resultList;

            int msgSize = Marshal.SizeOf(typeof(ZDBC.DBCMessage));
            IntPtr ptrMsg = Marshal.AllocHGlobal(msgSize);

            try
            {
                bool hasMessage = ZDBC.ZDBC_GetFirstMessage(DBCHandle, ptrMsg);
                while (hasMessage)
                {
                    var msgStruct = Marshal.PtrToStructure<ZDBC.DBCMessage>(ptrMsg);

                    // 提取 ID 和 信号名
                    var msgInfo = new DbcMessageInfo { MessageId = msgStruct.nID };
                    for (int i = 0; i < msgStruct.nSignalCount; i++)
                    {
                        msgInfo.SignalNames.Add(GetCleanString(msgStruct.vSignals[i].strName));
                    }

                    resultList.Add(msgInfo);
                    hasMessage = ZDBC.ZDBC_GetNextMessage(DBCHandle, ptrMsg);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrMsg);
            }

            return resultList;
        }
 

        /// <summary>
        /// 辅助方法：处理 C 风格字符串
        /// </summary>
        private static string GetCleanString(byte[] bytes)
        {
            if (bytes == null) return string.Empty;
            string str = Encoding.ASCII.GetString(bytes);
            int index = str.IndexOf('\0');
            return index >= 0 ? str.Substring(0, index) : str.Trim();
        }

        /// <summary>
        /// 从指定 ID 的消息中解析并提取所有信号名
        /// </summary>
        /// <param name="dbchandle">DBC句柄</param>
        /// <param name="msgID">消息ID (如 0x36F)</param>
        /// <returns>信号名列表</returns>
        private static List<string> GetSignalNamesFromMessage(uint msgID)
        {
            List<string> signalNames = new List<string>();
            IntPtr ptrMsg = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ZDBC.DBCMessage)));

            try
            {
                if (DBCHandle == IntPtr.Zero)
                {
                    Debug.WriteLine("DBC 句柄初始化失败");
                    throw new Exception("DBC 句柄初始化失败");
                }
                // 1. 获取消息结构
                if (ZDBC.ZDBC_GetMessageById(DBCHandle, msgID, ptrMsg))
                {
                    ZDBC.DBCMessage msg = (ZDBC.DBCMessage)Marshal.PtrToStructure(ptrMsg, typeof(ZDBC.DBCMessage));
                     
                    // 2. 遍历信号并提取名称
                    for (int i = 0; i < msg.nSignalCount; i++)
                    {
                        string rawName = Encoding.ASCII.GetString(msg.vSignals[i].strName);
                        string cleanName = rawName.Contains("\0") ? rawName.Substring(0, rawName.IndexOf('\0')) : rawName.Trim();
                        signalNames.Add(cleanName);
                        Debug.WriteLine($"{i}: {cleanName}");

                    }
                }
                else
                {
                    Debug.WriteLine($"未找到 ID: 0x{msgID:X} 的定义");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"解析并提取所有信号名错误{e.Message}");
            }
            finally
            {
                Marshal.FreeHGlobal(ptrMsg);
            }

            return signalNames;
        }

       

 
 
         
        // 建议改为异步任务，防止阻塞 UI
        public static async Task<double> ReceiveSignalAsync(Signal_Name signalName, int timeoutMs)
        {
            if (timeoutMs <= 0)
            {
                timeoutMs = 3000;
            }
            
            return await Task.Run(() =>
            {
                // 1. 检查状态
                if (devHandle == IntPtr.Zero || chnHandle[channel_index] == IntPtr.Zero || DBCHandle == 0)
                {
                    return double.NaN;
                }
                Can_Type can_Type = Initcan_Type;
                IntPtr cur_chn = (IntPtr)chnHandle[channel_index];
                const int arraySize = 3000;

                // 2. 分配内存
                int canSize = Marshal.SizeOf(typeof(ZLGCAN.ZCAN_Receive_Data));
                int fdSize = Marshal.SizeOf(typeof(ZLGCAN.ZCAN_ReceiveFD_Data));
                IntPtr ptrCanData = Marshal.AllocHGlobal(canSize * arraySize);
                IntPtr ptrCanFDData = Marshal.AllocHGlobal(fdSize * arraySize);
                IntPtr ptrMsg = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ZDBC.DBCMessage)));

                try
                {
                    ZLGCANAPI.ZLGCAN.ZCAN_ClearBuffer(cur_chn);
                    Stopwatch sw = Stopwatch.StartNew();
                    while (ConnectFlag && sw.ElapsedMilliseconds < timeoutMs)
                    {
                        bool hasDataProcessed = false; // 标记本次循环是否处理了数据

                        if (can_Type == Can_Type.CAN)
                        {
                            uint recNum = ZLGCAN.ZCAN_GetReceiveNum(cur_chn, 0);
                            if (recNum > 0)
                            {
                                uint count = ZLGCAN.ZCAN_Receive(cur_chn, ptrCanData, arraySize, 5);

                                // ✅ 新增：CAN 成功读出数据后，累加帧数
                                if (count > 0) AddReceivedFrameCount((int)count);

                                for (int i = 0; i < count; i++)
                                {
                                    IntPtr curPtr = IntPtr.Add(ptrCanData, i * canSize);
                                    if (ZDBC.ZDBC_Decode(DBCHandle, ptrMsg, curPtr, 1, 0))
                                    {
                                        double val = Get_SignalActualValue_Proper(ptrMsg, signalName.ToString());
                                        if (!double.IsNaN(val)) return val;
                                    }
                                }
                                hasDataProcessed = true; // 读到了数据，标记为 true
                            }
                        }
                        else if (can_Type == Can_Type.CAN_Fd)
                        {
                            uint RecNum = ZLGCAN.ZCAN_GetReceiveNum(cur_chn, 1);
                            if (RecNum != 0)
                            {
                                uint ReceiveNum = ZLGCAN.ZCAN_ReceiveFD(cur_chn, ptrCanFDData, arraySize, 10);

                                // ✅ 新增：CANFD 成功读出数据后，累加帧数
                                if (ReceiveNum > 0) AddReceivedFrameCount((int)ReceiveNum);

                                for (int i = 0; i < ReceiveNum; i++)
                                {
                                    IntPtr curPtr = IntPtr.Add(ptrCanFDData, i * Marshal.SizeOf(typeof(ZLGCAN.ZCAN_ReceiveFD_Data)));
                                    if (true == ZDBC.ZDBC_Decode(DBCHandle, ptrMsg, curPtr, 1, 1))
                                    {
                                        double actualValue = Get_SignalActualValue_Proper(ptrMsg, signalName.ToString());
                                        if (!double.IsNaN(actualValue))
                                        {
                                            return actualValue;
                                        }
                                    }
                                }
                            }
                        }

                        // ⭐ 核心修复：如果刚刚读取到了数据，说明缓冲区可能还没清空，立刻继续下一次读取，不休眠！
                        if (hasDataProcessed)
                        {
                            continue;
                        }

                        // 只有当缓冲区空了，确实没数据时，才休眠让出 CPU
                        Thread.Sleep(15);
                    }
 
                    return double.NaN; // 超时或连接断开
                }
                finally
                {
                    // 3. 严格释放
                    Marshal.FreeHGlobal(ptrCanData);
                    Marshal.FreeHGlobal(ptrCanFDData);
                    Marshal.FreeHGlobal(ptrMsg);
                }
            });
        }

        /// <summary>
        /// 直接根据信号名获取物理值（修正版）
        /// </summary>
        /// <param name="ptrMsg">ZDBC_Decode 输出的那个原始 IntPtr</param>
        /// <param name="signalName">信号名</param>
        /// <returns>物理值，失败返回 NaN</returns>
        private static double Get_SignalActualValue_Proper(IntPtr ptrMsg, string signalName)
        {
            ZDBC.DBCMessage msg = (ZDBC.DBCMessage)Marshal.PtrToStructure(ptrMsg, typeof(ZDBC.DBCMessage));

            // 遍历信号
            for (int i = 0; i < msg.nSignalCount; i++)
            {
                ZDBC.DBCSignal curSig = msg.vSignals[i];

                // 更安全的字符串截取方式，防止 IndexOf 找不到 '\0' 导致异常
                string str_name = new string(Encoding.ASCII.GetChars(curSig.strName));
                int nullIndex = str_name.IndexOf('\0');
                string result = nullIndex >= 0 ? str_name.Substring(0, nullIndex) : str_name;

                if (result == signalName)
                {
                    // 【关键点】
                    // 如果你要的是总线上的 16 进制转过来的原样数字，继续用 nRawvalue，但在外层用 ToString("X") 格式化看。
                    // 如果你要的是经过 Factor 和 Offset 计算过的“真正的物理意义的值”（比如转速 3000 rpm），请用 nValue （请核对你的 ZDBCSignal 结构体，通常物理值字段名为 nValue 或 Value）。
                    //实际值 = 原始值 * nFactor + nOffset
                    double realData = (curSig.nRawvalue*curSig.nFactor) + curSig.nOffset;
                    return realData; //实际值
                }
            }
            return double.NaN;
        }

        #endregion

        #region 8. 丢包率统计方法

        /// <summary>
        /// 启动后台丢包监控任务（建议在 Init_chn_USB 成功后调用）
        /// </summary>
        public static void StartLossMonitor()
        {
            StopLossMonitor();
            _fifoOverflowCount = 0;
            _bufferOverflowCount = 0;
            _totalReceivedFrames = 0; // ✅ 新增：启动时同步清零收帧数
            _lastFifoStatus = false;
            _lastBufferStatus = false;

            _lossMonitorCts = new CancellationTokenSource();
            _lossMonitorTask = Task.Run(() => MonitorLossLoop(_lossMonitorCts.Token));
            Debug.WriteLine("丢包率监控已启动。");
        }

        public static void StopLossMonitor()
        {
            if (_lossMonitorCts != null)
            {
                _lossMonitorCts.Cancel();
                try { _lossMonitorTask?.Wait(500); } catch { }
                _lossMonitorTask = null;
                _lossMonitorCts.Dispose();
                _lossMonitorCts = null;
            }
        }

        /// <summary>
        /// 后台轮询错误信息的核心逻辑
        /// </summary>
        private static async Task MonitorLossLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && ConnectFlag)
            {
                try
                {
                    IntPtr curChn = chnHandle[channel_index];
                    if (curChn != IntPtr.Zero)
                    {
                        ZLGCAN.ZCAN_CHANNEL_ERR_INFO errInfo = new ZLGCAN.ZCAN_CHANNEL_ERR_INFO();
                        // 注意：结构体中的数组必须初始化，否则Marshal会报错
                        errInfo.passive_ErrData = new byte[3];

                        uint ret = ZLGCAN.ZCAN_ReadChannelErrInfo(curChn, ref errInfo);
                        if (ret == 1)
                        {
                            // 边缘检测：只有从 0 变 1 的时候才算一次新的丢包事件，避免同一个错误一直置位导致疯狂计数
                            bool isFifoErr = (errInfo.error_code & ERR_FIFO_OVERFLOW) != 0;
                            bool isBufErr = (errInfo.error_code & ERR_BUFFER_OVERFLOW) != 0;

                            if (isFifoErr && !_lastFifoStatus)
                            {
                                Interlocked.Increment(ref _fifoOverflowCount);
                                Debug.WriteLine($"[硬件丢包] 检测到CAN控制器FIFO溢出！累计次数: {_fifoOverflowCount}");
                            }

                            if (isBufErr && !_lastBufferStatus)
                            {
                                Interlocked.Increment(ref _bufferOverflowCount);
                                Debug.WriteLine($"[软件丢包] 检测到PC端缓冲区溢出！累计次数: {_bufferOverflowCount}");
                            }

                            _lastFifoStatus = isFifoErr;
                            _lastBufferStatus = isBufErr;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"监控丢包任务异常: {ex.Message}");
                }

                // 每 50ms 检测一次，频率足够捕捉瞬间的溢出，又不会过度占用CPU
                await Task.Delay(50, token).ContinueWith(t => { }); // 忽略取消时的异常
            }
        }

        /// <summary>
        /// 获取自监控启动以来的累计硬件丢包（FIFO溢出）次数
        /// </summary>
        public static long GetHardwareLossCount()
        {
            return Interlocked.Read(ref _fifoOverflowCount);
        }

        /// <summary>
        /// 获取自监控启动以来的累计软件丢包（缓冲区溢出）次数
        /// </summary>
        public static long GetSoftwareLossCount()
        {
            return Interlocked.Read(ref _bufferOverflowCount);
        }

        // ================= ✅ 新增：供外部直接获取实时丢包率的方法 =================

        /// <summary>
        /// 内部调用：累加成功接收的帧数
        /// </summary>
        private static void AddReceivedFrameCount(int count)
        {
            Interlocked.Add(ref _totalReceivedFrames, count);
        }

        /// <summary>
        /// 获取累计接收到的总帧数
        /// </summary>
        public static long GetTotalReceivedFrames()
        {
            return Interlocked.Read(ref _totalReceivedFrames);
        }

        /// <summary>
        /// 获取实时计算的丢包率百分比 (UI层直接调这个即可)
        /// 公式：丢包次数 / (成功收帧数 + 丢包次数) * 100
        /// </summary>
        /// <returns>0.0 ~ 100.0 的百分比值</returns>
        public static double GetRealTimeLossRate()
        {
            long received = GetTotalReceivedFrames();
            long loss = GetHardwareLossCount() + GetSoftwareLossCount();

            long total = received + loss;
            if (total == 0) return 0.0;

            return (double)loss / total * 100.0;
        }
        #endregion

        #region 私有方法
        // 构造CANID
        private static uint MakeCanId(uint id, int eff, int rtr, int err)
        {
            uint ueff = (uint)(eff != 0 ? 1 : 0); // 0 标准帧，1 扩展帧
            uint urtr = (uint)(rtr != 0 ? 1 : 0); // 0 数据帧，1 远程帧
            uint uerr = (uint)(err != 0 ? 1 : 0); // 0 CAN帧，1 错误帧
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }
        // 辅助方法：将任意长度映射为合法的 CANFD DLC 长度规范
        private static byte GetValidCanFdLength(int length)
        {
            if (length <= 8) return (byte)length;
            if (length <= 12) return 12;
            if (length <= 16) return 16;
            if (length <= 20) return 20;
            if (length <= 24) return 24;
            if (length <= 32) return 32;
            if (length <= 48) return 48;
            return 64;
        }
        #endregion
        public void Dispose()
        {
            CloseDevice();
        }
      
    }
}