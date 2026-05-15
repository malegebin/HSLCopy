using CommunityToolkit.Mvvm.ComponentModel;
using HSLCopy.Models;

namespace HSLCopy.ViewModels
{
    public partial class ProductEditViewModel : ObservableObject
    {
        // 建议加上 readonly 防止在外面被意外替换整个实例
        public RunProgramModel RunProgram { get; }

        // ★ 通过构造函数要白板
        public ProductEditViewModel(RunProgramModel runProgram)
        {
            RunProgram = runProgram ?? new RunProgramModel();
        }
    }
}