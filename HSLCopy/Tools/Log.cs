using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace HSLCopy.Tools
{
    public static class Log
    {
        private static readonly object _lock = new();

        private static string _currentLogFile = "";

        public static Dispatcher? Dispatcher { get; set; }

        public static event Action<string, SolidColorBrush, int>? LogAdded;

        private static readonly StringBuilder _logBuffer = new();
        private static readonly System.Timers.Timer _flushTimer = new(1000);

        static Log()
        {
            _flushTimer.Elapsed += (s, e) => FlushBuffer();
            _flushTimer.Start();
        }
        private static void FlushBuffer()
        {
            lock (_lock)
            {
                if (_logBuffer.Length > 0)
                {
                    File.AppendAllText(_currentLogFile, _logBuffer.ToString());
                    _logBuffer.Clear();
                }
            }
        }

        public static void Info(string message, int depth = 0)
        {
            Write("INFO", message, Colors.DodgerBlue, depth);
        }

        public static void Success(string message, int depth = 0)
        {
            Write("SUCCESS", message, Colors.LimeGreen, depth);
        }

        public static void Error(string message, int depth = 0)
        {
            Write("ERROR", message, Colors.Red, depth);
        }

        public static void Warning(string message, int depth = 0)
        {
            Write("WARNING", message, Colors.Orange, depth);
        }

        private static void Write(string level, string message, Color color, int depth = 0)
        {
            lock (_lock)
            {
                try
                {
                    // 创建按年月组织的目录
                    string monthDir = Path.Combine(GlobalSystemConfig.Instance.LogFilePath, DateTime.Now.ToString("yyyy-MM"));
                    Directory.CreateDirectory(monthDir);

                    // 创建当天的日志文件
                    _currentLogFile = Path.Combine(monthDir, $"{DateTime.Now:yyyy-MM-dd}.log");

                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";

                    // 写入文件
                    File.AppendAllTextAsync(_currentLogFile, logEntry + Environment.NewLine).ConfigureAwait(false);

                    // 使用 Dispatcher 安全触发事件
                    if (Dispatcher != null && !Dispatcher.CheckAccess())
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogAdded?.Invoke(logEntry, new SolidColorBrush(color), depth);
                        });
                    }
                    else
                    {
                        LogAdded?.Invoke(logEntry, new SolidColorBrush(color), depth);
                    }
                }
                catch
                {
                    // 防止日志写入失败导致程序崩溃
                }
            }
        }
    }

    public class LogEntry
    {
        public string Message { get; }

        public SolidColorBrush Color { get; }

        public int Depth { get; } // 用于缩进显示

        public LogEntry(string message, SolidColorBrush color, int depth = 0)
        {
            Message = new string(' ', depth * 20) + message;
            Color = color;
            Depth = depth;
        }
    }
}
