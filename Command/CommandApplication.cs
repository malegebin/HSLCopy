using Common.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Attributes.ATSCommandAttribute;

namespace Command
{

    [ATSCommand]
    [DeviceCategory("应用指令")]
    public static class CommandApplication
    {
        /// <summary>
        /// 打开外部应用程序
        /// </summary>
        /// <param name="route">程序路径</param>
        /// <param name="waitShutdown">是否等待关闭</param>
        /// <param name="AdminRun">是否请求管理员运行</param>
        public static void OpenApplication(string route, bool waitShutdown, bool AdminRun)
        {

            // 创建一个新的进程实例
            Process process = new Process();

            try
            {
                // 指定要启动的程序路径
                process.StartInfo.FileName = route;

                // 指定是否等待程序关闭
                process.StartInfo.UseShellExecute = !waitShutdown;
                if (AdminRun) process.StartInfo.Verb = "runas";
                // 启动程序
                process.Start();

                // 如果需要等待程序关闭，则等待程序退出
                if (waitShutdown)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误!: " + ex.Message);
            }
            finally
            {
                // 确保进程对象被释放
                process.Dispose();
            }

        }
        /// <summary>
        /// 使用默认方式打开一个文件
        /// </summary>
        /// <param name="route">文件路径</param>
        public static void OpenFile(string route)
        {
            // 创建一个新的进程实例
            Process process = new Process();

            try
            {
                // 指定要启动的程序路径
                process.StartInfo.FileName = route;
                process.StartInfo.UseShellExecute = true;
                // 启动程序
                process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误!: " + ex.Message);
            }
            finally
            {
                // 确保进程对象被释放
                process.Dispose();
            }

        }



        // Python引擎相关字段
        private static Process _pythonProcess;
        private static string _pythonPath = string.Empty;
        private static bool _isPythonEngineInitialized = false;

        /// <summary>
        /// 初始化python引擎
        /// </summary>
        /// <param name="py路径">Python解释器路径</param>
        public static void Initialization_Python(string py路径)
        {
            try
            {
                // 验证Python路径是否存在
                if (string.IsNullOrEmpty(py路径) || !File.Exists(py路径))
                {
                    throw new FileNotFoundException($"Python解释器路径不存在: {py路径}");
                }

                // 验证是否为Python可执行文件
                var fileInfo = new FileInfo(py路径);
                if (!fileInfo.Name.ToLower().Contains("python"))
                {
                    Console.WriteLine("警告: 指定的文件可能不是Python解释器");
                }

                // 测试Python是否能正常运行
                var testProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = py路径,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                testProcess.Start();
                testProcess.WaitForExit(5000); // 5秒超时

                if (testProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Python解释器验证失败，退出码: {testProcess.ExitCode}");
                }

                _pythonPath = py路径;
                _isPythonEngineInitialized = true;

                Console.WriteLine($"Python引擎初始化成功，版本: {testProcess.StandardOutput.ReadToEnd().Trim()}");
            }
            catch (Exception ex)
            {
                _isPythonEngineInitialized = false;
                _pythonPath = string.Empty;
                Console.WriteLine($"初始化Python引擎失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 运行python脚本
        /// </summary>
        /// <param name="路径">Python脚本路径</param>
        /// <returns>脚本执行结果</returns>
        public static string Run_Python_Script(string 路径)
        {
            if (!_isPythonEngineInitialized)
            {
                throw new InvalidOperationException("Python引擎未初始化，请先调用初始化python引擎");
            }

            if (string.IsNullOrEmpty(路径) || !File.Exists(路径))
            {
                throw new FileNotFoundException($"Python脚本路径不存在: {路径}");
            }

            Process process = null;
            try
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _pythonPath,
                        Arguments = $"\"{路径}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                var output = new StringBuilder();
                var error = new StringBuilder();

                // 设置输出和错误处理
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        error.AppendLine(e.Data);
                };

                process.Start();

                // 异步读取输出
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                var outputResult = output.ToString().Trim();
                var errorResult = error.ToString().Trim();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Python脚本执行失败，退出码: {process.ExitCode}, 错误: {errorResult}");
                }

                return string.IsNullOrEmpty(outputResult) ? "执行成功" : outputResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"运行Python脚本失败: {ex.Message}");
                throw;
            }
            finally
            {
                process?.Dispose();
            }
        }

        /// <summary>
        /// 释放python引擎
        /// </summary>
        /// <param name="py">Python解释器路径（可选，用于验证）</param>
        public static void Release_Python(string py = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(py) && py != _pythonPath)
                {
                    Console.WriteLine($"警告: 释放的Python路径与当前初始化路径不匹配");
                }

                // 如果有正在运行的Python进程，尝试终止它
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    try
                    {
                        _pythonProcess.Kill();
                        _pythonProcess.WaitForExit(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"终止Python进程时出错: {ex.Message}");
                    }
                }

                _pythonPath = string.Empty;
                _isPythonEngineInitialized = false;
                _pythonProcess = null;

                Console.WriteLine("Python引擎已释放");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"释放Python引擎失败: {ex.Message}");
                throw;
            }
        }

        #region LabVIEW相关功能

        // LabVIEW引擎相关字段
        private static Process _labviewProcess;
        private static string _labviewPath = string.Empty;
        private static bool _isLabVIEWEngineInitialized = false;

        /// <summary>
        /// 初始化LabVIEW引擎
        /// </summary>
        /// <param name="labview路径">LabVIEW可执行文件路径</param>
        public static void Initialization_LabVIEW(string labview路径)
        {
            try
            {
                // 验证LabVIEW路径是否存在
                if (string.IsNullOrEmpty(labview路径) || !File.Exists(labview路径))
                {
                    throw new FileNotFoundException($"LabVIEW可执行文件路径不存在: {labview路径}");
                }

                // 验证是否为LabVIEW可执行文件
                var fileInfo = new FileInfo(labview路径);
                if (!fileInfo.Name.ToLower().Contains("labview") && !fileInfo.Name.ToLower().Contains("vi"))
                {
                    Console.WriteLine("警告: 指定的文件可能不是LabVIEW相关文件");
                }

                // 测试LabVIEW是否能正常运行
                var testProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = labview路径,
                        Arguments = "--help", // 使用帮助参数进行快速测试
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                testProcess.Start();
                testProcess.WaitForExit(5000); // 5秒超时

                _labviewPath = labview路径;
                _isLabVIEWEngineInitialized = true;

                Console.WriteLine($"LabVIEW引擎初始化成功，路径: {labview路径}");
            }
            catch (Exception ex)
            {
                _isLabVIEWEngineInitialized = false;
                _labviewPath = string.Empty;
                Console.WriteLine($"初始化LabVIEW引擎失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 运行LabVIEW VI
        /// </summary>
        /// <param name="路径">LabVIEW VI文件路径</param>
        /// <param name="参数">传递给VI的参数</param>
        /// <returns>运行结果</returns>
        public static string Run_LabVIEW_VI(string 路径, string 参数 = "")
        {
            if (!_isLabVIEWEngineInitialized && string.IsNullOrEmpty(_labviewPath))
            {
                // 如果没有初始化，尝试使用默认LabVIEW路径
                var defaultLabVIEWPath = FindDefaultLabVIEWPath();
                if (string.IsNullOrEmpty(defaultLabVIEWPath))
                {
                    throw new InvalidOperationException("LabVIEW引擎未初始化，且未找到默认LabVIEW路径，请先调用初始化LabVIEW引擎");
                }
                _labviewPath = defaultLabVIEWPath;
            }

            if (string.IsNullOrEmpty(路径) || !File.Exists(路径))
            {
                throw new FileNotFoundException($"LabVIEW VI文件路径不存在: {路径}");
            }

            Process process = null;
            try
            {
                var viFileInfo = new FileInfo(路径);
                if (!viFileInfo.Extension.ToLower().Equals(".vi"))
                {
                    Console.WriteLine("警告: 指定的文件可能不是VI文件");
                }

                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = string.IsNullOrEmpty(_labviewPath) ? FindDefaultLabVIEWPath() : _labviewPath,
                        Arguments = $"\"{路径}\" {参数}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false // LabVIEW VI通常需要窗口显示
                    }
                };

                var output = new StringBuilder();
                var error = new StringBuilder();

                // 设置输出和错误处理
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        error.AppendLine(e.Data);
                };

                process.Start();

                // 异步读取输出
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                var outputResult = output.ToString().Trim();
                var errorResult = error.ToString().Trim();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"LabVIEW VI执行失败，退出码: {process.ExitCode}, 错误: {errorResult}");
                }

                return string.IsNullOrEmpty(outputResult) ? "执行成功" : outputResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"运行LabVIEW VI失败: {ex.Message}");
                throw;
            }
            finally
            {
                process?.Dispose();
            }
        }

        /// <summary>
        /// 运行LabVIEW项目
        /// </summary>
        /// <param name="路径">LabVIEW项目文件路径</param>
        /// <param name="启动VI">要启动的VI名称</param>
        /// <param name="参数">传递给VI的参数</param>
        /// <returns>运行结果</returns>
        public static string Run_LabVIEW_Project(string 路径, string 启动VI = "", string 参数 = "")
        {
            if (!_isLabVIEWEngineInitialized && string.IsNullOrEmpty(_labviewPath))
            {
                // 如果没有初始化，尝试使用默认LabVIEW路径
                var defaultLabVIEWPath = FindDefaultLabVIEWPath();
                if (string.IsNullOrEmpty(defaultLabVIEWPath))
                {
                    throw new InvalidOperationException("LabVIEW引擎未初始化，且未找到默认LabVIEW路径，请先调用初始化LabVIEW引擎");
                }
                _labviewPath = defaultLabVIEWPath;
            }

            if (string.IsNullOrEmpty(路径) || !File.Exists(路径))
            {
                throw new FileNotFoundException($"LabVIEW项目文件路径不存在: {路径}");
            }

            Process process = null;
            try
            {
                var projectFileInfo = new FileInfo(路径);
                if (!projectFileInfo.Extension.ToLower().Equals(".lvproj"))
                {
                    Console.WriteLine("警告: 指定的文件可能不是LabVIEW项目文件");
                }

                string arguments = $"\"{路径}\"";
                if (!string.IsNullOrEmpty(启动VI))
                {
                    arguments += $" /vi:{启动VI}";
                }
                if (!string.IsNullOrEmpty(参数))
                {
                    arguments += $" {参数}";
                }

                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = string.IsNullOrEmpty(_labviewPath) ? FindDefaultLabVIEWPath() : _labviewPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false
                    }
                };

                var output = new StringBuilder();
                var error = new StringBuilder();

                // 设置输出和错误处理
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        error.AppendLine(e.Data);
                };

                process.Start();

                // 异步读取输出
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                var outputResult = output.ToString().Trim();
                var errorResult = error.ToString().Trim();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"LabVIEW项目执行失败，退出码: {process.ExitCode}, 错误: {errorResult}");
                }

                return string.IsNullOrEmpty(outputResult) ? "执行成功" : outputResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"运行LabVIEW项目失败: {ex.Message}");
                throw;
            }
            finally
            {
                process?.Dispose();
            }
        }

        /// <summary>
        /// 释放LabVIEW引擎
        /// </summary>
        /// <param name="labview">LabVIEW路径（可选，用于验证）</param>
        public static void Release_LabVIEW(string labview = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(labview) && labview != _labviewPath)
                {
                    Console.WriteLine($"警告: 释放的LabVIEW路径与当前初始化路径不匹配");
                }

                // 如果有正在运行的LabVIEW进程，尝试终止它
                if (_labviewProcess != null && !_labviewProcess.HasExited)
                {
                    try
                    {
                        _labviewProcess.Kill();
                        _labviewProcess.WaitForExit(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"终止LabVIEW进程时出错: {ex.Message}");
                    }
                }

                _labviewPath = string.Empty;
                _isLabVIEWEngineInitialized = false;
                _labviewProcess = null;

                Console.WriteLine("LabVIEW引擎已释放");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"释放LabVIEW引擎失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 查找默认LabVIEW安装路径
        /// </summary>
        /// <returns>LabVIEW可执行文件路径</returns>
        private static string FindDefaultLabVIEWPath()
        {
            // 常见的LabVIEW安装路径
            var possiblePaths = new[]
            {
                @"C:\Program Files\National Instruments\LabVIEW 2025\LabVIEW.exe",
                @"C:\Program Files\National Instruments\LabVIEW 2024\LabVIEW.exe",
                @"C:\Program Files\National Instruments\LabVIEW 2023\LabVIEW.exe",
                @"C:\Program Files\National Instruments\LabVIEW 2022\LabVIEW.exe",
                @"C:\Program Files\National Instruments\LabVIEW 2021\LabVIEW.exe",
                @"C:\Program Files\National Instruments\LabVIEW 2020\LabVIEW.exe",
                @"C:\Program Files (x86)\National Instruments\LabVIEW 2025\LabVIEW.exe",
                @"C:\Program Files (x86)\National Instruments\LabVIEW 2024\LabVIEW.exe",
                @"C:\Program Files (x86)\National Instruments\LabVIEW 2023\LabVIEW.exe",
                @"C:\Program Files (x86)\National Instruments\LabVIEW 2022\LabVIEW.exe",
                @"C:\Program Files (x86)\National Instruments\LabVIEW 2021\LabVIEW.exe",
                @"C:\Program Files (x86)\National Instruments\LabVIEW 2020\LabVIEW.exe"

            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        #endregion
    }
}