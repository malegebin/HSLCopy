using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZLGCANAPI;

namespace ZLG_USB_CAN
{
    public class ZLGCAN_DBCParse
    {
        public class ZlgMsgInfo
        {
            public uint MsgID { get; set; }
            public string MsgName { get; set; }
            public int SignalCount { get; set; }
            public List<string> SignalNames { get; set; }
            // 可以根据需要添加更多字段，如 DLC, 周期等

        }
        public class ZlgDbcParser
        {
            // 模拟原先的 MsgDatabase 结构
            public static List<List<ZlgMsgInfo>> ZlgMsgDatabase { get; set; } =
                Enumerable.Range(0, 12).Select(_ => new List<ZlgMsgInfo>()).ToList();

            /// <summary>
            /// 仿造的 ZLG 版 DBC 解析方法
            /// </summary>
            /// <param name="dbcHandle">ZDBC_Init 得到的句柄</param>
            /// <param name="channel">对应的通道号索引</param>
            public static void Parse(uint dbcHandle, int channel)
            {
                if (dbcHandle == 0) return;

                // 1. 清空当前通道旧数据
                ZlgMsgDatabase[channel].Clear();

                // 2. 为 DBCMessage 结构体分配非托管内存
                int msgSize = Marshal.SizeOf(typeof(ZDBC.DBCMessage));
                IntPtr ptrMsg = Marshal.AllocHGlobal(msgSize);

                try
                {
                    // 3. 获取第一条消息
                    if (ZDBC.ZDBC_GetFirstMessage(dbcHandle, ptrMsg))
                    {
                        ExtractAndAdd(ptrMsg, channel);

                        // 4. 循环获取后续消息
                        while (ZDBC.ZDBC_GetNextMessage(dbcHandle, ptrMsg))
                        {
                            ExtractAndAdd(ptrMsg, channel);
                        }
                    }

                    // 5. 排序（可选，按 ID 排序）
                    ZlgMsgDatabase[channel].Sort((a, b) => a.MsgID.CompareTo(b.MsgID));
                }
                finally
                {
                    // 6. 必须释放内存
                    Marshal.FreeHGlobal(ptrMsg);
                }
            }

            /// <summary>
            /// 提取结构体信息并存入 List
            /// </summary>
            private static void ExtractAndAdd(IntPtr ptrMsg, int channel)
            {
                ZDBC.DBCMessage msg = (ZDBC.DBCMessage)Marshal.PtrToStructure(ptrMsg, typeof(ZDBC.DBCMessage));

                ZlgMsgInfo info = new ZlgMsgInfo
                {
                    MsgID = msg.nID,
                    MsgName = ByteToString(msg.strName),
                    SignalCount = (int)msg.nSignalCount,
                    SignalNames = new List<string>()
                };

                // 提取该消息下的所有信号名
                for (int i = 0; i < msg.nSignalCount; i++)
                {
                    string sigName = ByteToString(msg.vSignals[i].strName);
                    info.SignalNames.Add(sigName);
                }

                ZlgMsgDatabase[channel].Add(info);
            }

            /// <summary>
            /// 辅助方法：将固定长度的 byte 数组转为 C# string（处理 \0）
            /// </summary>
            private static string ByteToString(byte[] bytes)
            {
                string fullStr = Encoding.ASCII.GetString(bytes);
                int nullIndex = fullStr.IndexOf('\0');
                return nullIndex >= 0 ? fullStr.Substring(0, nullIndex) : fullStr;
            }
        }
    }
}
