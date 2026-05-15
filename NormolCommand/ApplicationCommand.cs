using CommandAttribute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommandAttribute.MarkIsCommandAttribute;

namespace NormolCommand
{
    [MarkIsCommand]
    [DeviceCategory("应用指令")]
    public class ApplicationCommand
    {
        [MarkIsCommand("应用指令", Category = "111")]
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
    }
}
