using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using HSLCopy.Config;
using HSLCopy.Models;
using HSLCopy.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Reflection;
using static HSLCopy.Models.ParameterModel;

namespace HSLCopy.ViewModels
{
    public partial class CommandViewModel : ObservableObject
    {
        private readonly AssemblyConfig _assemblyConfig;

        // ★ 注入运行视图的 ViewModel，用来获取选中索引
        private readonly StepsRunningViewModel _stepsRunningVm;

        [ObservableProperty]
        private ObservableCollection<TreeNodeModel> _nodes = new();

        // ★ 彻底修改：通过构造函数注入，不要自己去 Service 掏
        // 注意：这里不要加 [ObservableProperty]，因为不需要外部改变它的引用，只用来读取操作集合
        private readonly RunProgramModel _runProgram;

        // ★ 构造函数注入所有依赖
        public CommandViewModel(AssemblyConfig assemblyConfig, RunProgramModel runProgram, StepsRunningViewModel stepsRunningVm)
        {
            _assemblyConfig = assemblyConfig;
            _runProgram = runProgram;
            _stepsRunningVm = stepsRunningVm;
            LoadTreeData();
        }

        private void LoadTreeData()
        {
            var tempList = new List<TreeNodeModel>();

            // 1. 固定的系统根节点
            var systemRootNode = new TreeNodeModel { HeaderName = "系统指令", Tag = "ControlRoot" };
            systemRootNode.Children.Add(new TreeNodeModel { HeaderName = "循环开始", Tag = "循环开始" });
            systemRootNode.Children.Add(new TreeNodeModel { HeaderName = "循环结束", Tag = "循环结束" });
            tempList.Add(systemRootNode);

            tempList.Add(new TreeNodeModel { HeaderName = "子程序", Tag = "SubProgramRoot" });

            // 2. 动态加载的指令根节点（解析三层嵌套字典）
            var commandTypesByCategory = _assemblyConfig.CommandTypesByCategory;

            // 第一层循环：遍历【分类】（比如："运动控制"、"无分类"）
            foreach (var categoryKvp in commandTypesByCategory)
            {
                var categoryNode = new TreeNodeModel
                {
                    HeaderName = categoryKvp.Key,
                    Tag = $"Category_{categoryKvp.Key}"
                };
                tempList.Add(categoryNode);

                // 第二层循环：遍历该分类下的【类】
                foreach (var classKvp in categoryKvp.Value)
                {
                    Type type = classKvp.Key;
                    var typeNode = new TreeNodeModel
                    {
                        HeaderName = type.Name,
                        Tag = type,
                        ToolTip = type.FullName
                    };
                    categoryNode.Children.Add(typeNode);

                    // 第三层循环：遍历该类下的【方法】
                    foreach (var method in classKvp.Value)
                    {
                        var parameters = method.GetParameters();
                        var paramText = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));

                        var methodNode = new TreeNodeModel
                        {
                            HeaderName = $"{method.Name}({paramText})",
                            Tag = method, // 如果你有 MethodInfoWrapper 可以替换为 new MethodInfoWrapper(method)
                            ToolTip = $"{type.FullName}.{method.Name}"
                        };
                        typeNode.Children.Add(methodNode);
                    }
                }
            }

            // 整体替换，触发 UI 刷新
            Nodes = new ObservableCollection<TreeNodeModel>(tempList);
        }

        [RelayCommand]
        public async Task ReloadAsync()
        {
            await _assemblyConfig.InitializeAsync(); // 假设 AssemblyConfig 里的 ReloadAsync 叫这个名字
            LoadTreeData();
        }

        [RelayCommand]
        private void AddToSteps(TreeNodeModel selectedNodeModel)
        {
            if(selectedNodeModel == null) return;
            // ★ 修复：把 insertIndex 提取到外面，所有分支都能访问到
            int insertIndex = -1;

            // ★ 修复：不再依赖 View 单例，而是通过注入的 ViewModel 获取状态
            if (_stepsRunningVm.SelectedIndex >= 0)
            {
                insertIndex = _stepsRunningVm.SelectedIndex + 1;
            }
            if (selectedNodeModel.Tag is string tagName)
            {

                if (tagName == "循环开始")
                {
                    //AddLoopStartStep(insertIndex);
                }
                else if (tagName == "循环结束")
                {
                    //AddLoopEndStep(insertIndex);
                }

            }
            else if (selectedNodeModel.Tag is MethodInfo methodInfo)
            {
                AddMethodToProgram(methodInfo, insertIndex);
            }


            //else if (selectedNodeModel.Tag is SubProgramItem subProgram)
            //{
            //    // ★ 删除原来的弹窗逻辑，直接调用添加方法
            //    AddSubProgramToProgram(subProgram, insertIndex);
            //}

            //StepsManager.Instance.SelectedIndex = insertIndex;
            //UnselectTreeViewItems(InstructionTreeView);
        }

        private void AddMethodToProgram(MethodInfo methodInfo, int insertIndex)
        {
            try
            {
                var newStepModel = new StepModel()
                {
                    StepName = methodInfo.Name,
                    StepType = "方法",
                    Method = new MethodModel()
                    {
                        FullName = methodInfo.DeclaringType?.FullName,
                        Name = methodInfo.Name
                    }


                };
                //添加方法输入参数
                foreach(var param in methodInfo.GetParameters())
                {
                    newStepModel.Method.Parameters.Add(new ParameterModel
                    {
                        Name = param.Name!,
                        Type = param.ParameterType,
                        Category = ParameterModel.ParameterCategory.Input,
                    });
                }
                //
                Type returnType = methodInfo.ReturnType;
                if (returnType == typeof(Task))
                {
                    // 不添加输出参数（无返回值）
                }
                //returnType.IsGenericType 作用判断这个方法的返回值，是不是即泛型类型
                //returnType.GetGenericTypeDefinition()==typeof(Task<>)精准判断这个返回值到底是不是一个“泛型的 Task”
                else if (returnType.IsGenericType&&
                    returnType.GetGenericTypeDefinition()==typeof(Task<>))
                {
                    // 提取实际返回类型（如 Task<bool> -> bool）
                    //尖括号 < > 里第一个类型提取出来
                    Type actualType = returnType.GetGenericArguments()[0];
                    newStepModel.Method.Parameters.Add(new ParameterModel
                    {
                        Name = "Result",
                        Type = actualType, // 使用实际类型
                        Category = ParameterCategory.Output
                    });
                }
                //public int Read() -> int != void 成立 -> 放行，生成 Result (Int32)
                //返回值为为空，不生成
                else if (returnType != typeof(void))
                {
                    // 同步方法正常添加
                    newStepModel.Method.Parameters.Add(new ParameterModel
                    {
                        Name = "Result",
                        Type = returnType,
                        Category = ParameterCategory.Output
                    });
                }
                // 添加到程序
               
                if (insertIndex >= 0 && insertIndex <= _runProgram.StepCollection.Count)
                {
                    _runProgram.StepCollection.Insert(insertIndex, newStepModel);
                }
                else
                {
                    _runProgram.StepCollection.Add(newStepModel);
                }

                ReOrderProgramList();
            }
            catch (Exception ex)
            {
                Log.Error($"添加方法失败: {methodInfo.Name} - {ex.Message}");
            }

        }
        private void ReOrderProgramList()
        {
            for (int i = 0; i < _runProgram.StepCollection.Count; i++)
            {
                _runProgram.StepCollection[i].Index = i + 1;
            }
        }
    }
}