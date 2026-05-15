using CommunityToolkit.Mvvm.ComponentModel;
using HSLCopy.Models;

namespace HSLCopy.ViewModels
{
    public partial class StepsRunningViewModel : ObservableObject
    {
        // 通过构造函数注入 RunProgramModel，不要自己去 Service 里掏
        private readonly RunProgramModel _runProgram;
        public RunProgramModel RunProgram => _runProgram;

        public StepsRunningViewModel(RunProgramModel runProgram)
        {
            _runProgram = runProgram ?? new();
        }

        // ★ 替代原来的 Title 属性
        [ObservableProperty]
        private string _title = "主程序  NewProgram.ats";

        // ★ 替代原来 View 里的 SelectedItem 属性
        [ObservableProperty]
        private object? _selectedItem;

        // ★ 替代原来 View 里的 SelectedIndex 属性
        [ObservableProperty]
        private int _selectedIndex = -1;

        // ★ 替代原来依赖 MainWindow.Instance 的文件路径
        [ObservableProperty]
        private string? _currentFilePath;

        ///// <summary>
        ///// 更新 Title 显示
        ///// </summary>
        //public void UpdateTitle(string? subProgramName = null)
        //{
        //    if (!string.IsNullOrEmpty(subProgramName))
        //    {
        //        Title = $"子程序  {subProgramName}";
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(CurrentFilePath))
        //        {
        //            var tmp = CurrentFilePath.Split("\\");
        //            Title = "主程序  " + tmp[^1];
        //        }
        //        else
        //        {
        //            Title = "主程序  NewProgram.ats";
        //        }
        //    }
        //}
    }
}