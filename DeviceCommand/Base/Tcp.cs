using Common.Attributes;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Common.Attributes.CommandAttribute;

namespace DeviceCommand.Base
{
    
    [DeviceCategory("全部驱动")]
    public class Tcp : IDisposable
    {
        public string IPAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public int SendTimeout { get; set; } = 3000;
        public int ReceiveTimeout { get; set; } = 3000;

        [JsonIgnore]
        public TcpClient TcpClient { get; set; }

        public Tcp CreateDevice(string ipAddress, int port, int sendTimeout = 3000, int receiveTimeout = 3000)
        {
            Close(); // 统一调用实例的Close

            TcpClient = new TcpClient();
            IPAddress = ipAddress;
            Port = port;
            SendTimeout = sendTimeout;
            ReceiveTimeout = receiveTimeout;

            return this;
        }

        /// <summary>
        /// 修改TCP连接参数
        /// </summary>
        public void ChangeDeviceConfig(string ipAddress, int port, int sendTimeout = 3000, int receiveTimeout = 3000)
        {
            IPAddress = ipAddress;
            Port = port;
            if (sendTimeout > 0) SendTimeout = sendTimeout;
            if (receiveTimeout > 0) ReceiveTimeout = receiveTimeout;
        }

        // 增加一个属性，方便指令逻辑判断
        [JsonIgnore]
        public bool IsConnected => TcpClient != null && TcpClient.Connected;

        /// <summary>
        /// 连接TCP设备
        /// </summary>
        public async Task<bool> ConnectAsync(CancellationToken ct = default)
        {
            try
            {
                // 1. 如果当前已经连接，且配置没变，直接复用
                if (IsConnected)
                {
                    if (TcpClient.Client.RemoteEndPoint is IPEndPoint remote &&
                        remote.Address.ToString() == IPAddress && remote.Port == Port)
                    {
                        // 探测底层链路是否真正存活
                        bool isDead = TcpClient.Client.Poll(1000, SelectMode.SelectRead) && TcpClient.Client.Available == 0;
                        if (!isDead) return true;
                    }
                }

                // 2. 关键修复：只要准备重新连接，就必须彻底清理掉旧的、可能已 Dispose 的 TcpClient
                Close();

                // 3. 重新创建实例（这是解决“无法再次连接”的关键）
                TcpClient = new TcpClient();
                TcpClient.SendTimeout = SendTimeout;
                TcpClient.ReceiveTimeout = ReceiveTimeout;

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (SendTimeout > 0) cts.CancelAfter(SendTimeout);

                // 4. 执行异步连接
                await TcpClient.ConnectAsync(IPAddress, Port, cts.Token);
                return IsConnected;
            }
            catch (Exception ex)
            {
                Close();
                throw new TimeoutException($"TCP连接到 {IPAddress}:{Port} 失败: {ex.Message}");
            }

        }

        /// <summary>
        /// 彻底关闭并释放资源
        /// </summary>
        public void Close()
        {
            if (TcpClient != null)
            {
                try
                {
                    // 先关闭流，再关闭客户端
                    TcpClient.GetStream()?.Close();
                    TcpClient.Close();
                    TcpClient.Dispose();
                }
                catch { /* 忽略网络层释放异常 */ }
                finally
                {
                    TcpClient = null; // ❗️必须置空，确保下次 ConnectAsync 进入重新创建逻辑
                }
            }
        }

        /// <summary>
        /// 发送字节数组到TCP设备
        /// </summary>
         public async Task SendAsync(byte[] bytes, CancellationToken ct = default)
        {
            if (TcpClient?.Connected != true) throw new InvalidOperationException("设备未连接");

            var stream = TcpClient.GetStream();
            var sendTask = stream.WriteAsync(bytes, 0, bytes.Length, ct) as Task;   

            if (SendTimeout > 0)
            {
                var timeoutTask = Task.Delay(SendTimeout, ct);
                // 竞速等待：如果超时先完成，抛出异常，但决不破坏 stream
                if (await Task.WhenAny(sendTask, timeoutTask) == timeoutTask)
                {
                    throw new TimeoutException($"TCP通讯异常：写入操作在 {SendTimeout} ms内未完成");
                }
            }
            await sendTask; // 确保捕获真实的写入异常
        }

        /// <summary>
        /// 发送字符串到TCP设备
        /// </summary>
        public async Task SendAsync(string str, CancellationToken ct = default)
        {
            await SendAsync(Encoding.UTF8.GetBytes(str), ct);
        }

        /// <summary>
        /// 接收指定长度的字节数组 (采用 DataAvailable 软超时，绝对不断开连接)
        /// </summary>
        public async Task<byte[]> ReadAsync(byte[] buffer, CancellationToken ct = default)
        {
            if (TcpClient?.Connected != true) return null;

            NetworkStream stream = TcpClient.GetStream();
            int bytesRead = 0;
            DateTime startTime = DateTime.Now;

            while (bytesRead < buffer.Length)
            {
                ct.ThrowIfCancellationRequested(); // 响应外部手动停止

                // 【软超时判定】
                if (ReceiveTimeout > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeout)
                {
                    throw new TimeoutException($"TCP通讯异常：读取操作在 {ReceiveTimeout} ms内未完成");
                }

                // 只有当缓冲区确实有数据时才去读，避免线程阻塞在底层 Socket 上
                if (stream.DataAvailable)
                {
                    int read = await stream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, ct);
                    if (read == 0) return null; // 被远端正常关闭
                    bytesRead += read;
                }
                else
                {
                    await Task.Delay(10, ct); // 休眠释放CPU
                }
            }
            return buffer;
        }

        /// <summary>
        /// 接收字符串直到遇到分隔符 (采用 DataAvailable 软超时，绝对不断开连接)
        /// </summary>
        public async Task<string> ReadAsync(string delimiter = "\n", CancellationToken ct = default)
        {
            if (TcpClient?.Connected != true) return null;

            delimiter ??= "\n";
            byte[] delimiterBytes = Encoding.UTF8.GetBytes(delimiter);

            NetworkStream stream = TcpClient.GetStream();
            using MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[1];

            DateTime startTime = DateTime.Now;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // 【软超时判定】
                if (ReceiveTimeout > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeout)
                {
                    throw new TimeoutException($"TCP通讯异常：读取操作在 {ReceiveTimeout} ms内未完成");
                }

                if (stream.DataAvailable)
                {
                    int read = await stream.ReadAsync(buffer, 0, 1, ct);
                    if (read == 0) break; // 流被关闭

                    ms.WriteByte(buffer[0]);

                    if (ms.Length >= delimiterBytes.Length)
                    {
                        byte[] currentData = ms.ToArray();
                        bool isMatch = true;

                        for (int i = 0; i < delimiterBytes.Length; i++)
                        {
                            if (currentData[currentData.Length - delimiterBytes.Length + i] != delimiterBytes[i])
                            {
                                isMatch = false;
                                break;
                            }
                        }

                        if (isMatch)
                        {
                            int count = currentData.Length - delimiterBytes.Length;
                            return Encoding.UTF8.GetString(currentData, 0, count).Trim();
                        }
                    }
                }
                else
                {
                    await Task.Delay(10, ct); // 等待数据
                }
            }
            return null;
        }

        public async Task<string> WriteRead(string str, string endstr, CancellationToken ct = default)
        {
            await SendAsync(str, ct);
            return await ReadAsync(endstr, ct);
        }
        /// <summary>
        /// 实现 IDisposable，允许使用 using 语法自动释放
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}