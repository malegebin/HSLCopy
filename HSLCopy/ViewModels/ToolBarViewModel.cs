using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace HSLCopy.ViewModels
{
    public partial class ToolBarViewModel : ObservableObject
    {
        // =====================================================
        // 属性定义
        // 注意：使用 [ObservableProperty] 时，字段名必须是小驼峰命名(首字母小写)，
        // 框架会自动生成大驼峰命名的公共属性 (如 IsAdmin, RunState 等)
        // =====================================================

        [ObservableProperty]
        private bool isAdmin = true; // 默认赋值测试用

        [ObservableProperty]
        private string runState = "运行";

        // 删除了原来的 PackIconKind runIcon，改用下面两个布尔值控制图标显隐，彻底解决绑定报错
        [ObservableProperty]
        private bool isPlaying = true;

        [ObservableProperty]
        private bool isStopped = false;

        [ObservableProperty]
        private bool canRun = true;

        [ObservableProperty]
        private bool canStop = false;

        [ObservableProperty]
        private bool canReset = true;

        // 用于控制“导出报告”按钮的底层状态
        [ObservableProperty]
        private bool canReport = false;

        // 计算属性：当 CanReport 为 false 时，CanExportReport 为 true（替代原来的 BoolInverseConverter）
        public bool CanExportReport => !CanReport;


        // =====================================================
        // 命令定义
        // 注意：方法名会自动生成对应的 Command 属性，例如 NavigateBack 会生成 NavigateBackCommand
        // =====================================================

        [RelayCommand]
        private void NavigateBack()
        {
            // TODO: 实现返回上级逻辑
            Debug.WriteLine("执行：返回上级程序");
        }

        [RelayCommand]
        private void FileNew()
        {
            // TODO: 实现新建逻辑
            Debug.WriteLine("执行：新建");
        }

        [RelayCommand]
        private void FileOpen()
        {
            // TODO: 实现打开逻辑
            Debug.WriteLine("执行：打开");
        }

        [RelayCommand]
        private void FileSave()
        {
            // TODO: 实现保存逻辑
            Debug.WriteLine("执行：保存");
        }

        [RelayCommand]
        private void FileSaveAsOther()
        {
            // TODO: 实现另存为逻辑
            Debug.WriteLine("执行：另存为");
        }

        [RelayCommand]
        private void SetDefaultProgram()
        {
            // TODO: 实现设置默认程序逻辑
            Debug.WriteLine("执行：设置默认程序");
        }

        [RelayCommand]
        private void SystemConfig()
        {
            // TODO: 实现系统设置逻辑
            Debug.WriteLine("执行：系统设置");
        }

        [RelayCommand]
        private void DeviceManage()
        {
            // TODO: 实现设备管理逻辑
            Debug.WriteLine("执行：设备管理");
        }

        [RelayCommand]
        private void ProgramRun()
        {
            // 切换逻辑变得更纯粹
            if (RunState == "运行")
            {
                RunState = "停止";
                CanRun = false;
                CanStop = true;
                IsPlaying = false; // 隐藏播放图标
                IsStopped = true;  // 显示停止图标
            }
            else
            {
                RunState = "运行";
                CanRun = true;
                CanStop = false;
                IsPlaying = true;  // 显示播放图标
                IsStopped = false; // 隐藏停止图标
            }
        }

        [RelayCommand]
        private void SingleStepExecution()
        {
            // TODO: 实现单步执行逻辑
            Debug.WriteLine("执行：单步执行");
        }

        [RelayCommand]
        private void Terminate()
        {
            // TODO: 实现强制停止逻辑
            Debug.WriteLine("执行：强制停止");
            RunState = "运行";
            CanRun = true;
            CanStop = false;
            IsPlaying = true;  // 恢复播放图标
            IsStopped = false; // 隐藏停止图标
        }

        [RelayCommand]
        private void Reset()
        {
            // TODO: 实现复位逻辑
            Debug.WriteLine("执行：复位");
            RunState = "运行";
            CanRun = true;
            CanStop = false;
            IsPlaying = true;  // 恢复播放图标
            IsStopped = false; // 隐藏停止图标
        }

        [RelayCommand]
        private void ExportReport()
        {
            // TODO: 实现导出报告逻辑
            Debug.WriteLine("执行：导出报告");
        }
    }
}