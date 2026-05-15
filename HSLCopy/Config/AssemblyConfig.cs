using CommandAttribute;
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static CommandAttribute.MarkIsCommandAttribute;
 

namespace HSLCopy.Config
{
    public class AssemblyConfig
    {
        // 1. 去掉 static，改为实例属性。交给 DI 容器管理生命周期
        private readonly List<Assembly> _assemblies = new List<Assembly>();

        // 如果外部需要遍历程序集，提供只读集合
        public IReadOnlyList<Assembly> Assemblies => _assemblies.AsReadOnly();

        // 分类 -> (类 -> 该类的方法列表)
        public Dictionary<string, Dictionary<Type, List<MethodInfo>>> CommandTypesByCategory { get; private set; } = new Dictionary<string, Dictionary<Type, List<MethodInfo>>>();

        // 2. 构造函数必须是无参且轻量级的，绝对不能放耗时的 LoadFrom
        public AssemblyConfig()
        {
        }

        /// <summary>
        /// 核心初始化方法（异步，不卡 UI）
        /// </summary>
        public async Task InitializeAsync()
        {
            // 将磁盘 IO 和反射操作扔到后台线程
            await Task.Run(() =>
            {
                LoadAssembliesFromDisk();
                BuildCommandCategories();
            });
        }

        private void LoadAssembliesFromDisk()
        {
            _assemblies.Clear();
            var dllPath = @"D:\HSLCopy\指令\";

            if (!Directory.Exists(dllPath)) return;

            foreach (var path in Directory.GetFiles(dllPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(path);
                    _assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    // 强烈建议替换为你的 Log.Error
                    Console.WriteLine($"[AssemblyConfig] 加载DLL失败: {path}, 错误: {ex.Message}");
                }
            }
        }

        private void BuildCommandCategories()
        {
            CommandTypesByCategory.Clear();

            foreach (var assembly in _assemblies)
            {
                var allTypes = assembly.GetTypes()
                    .Where(type => type.IsClass && type.IsPublic && !type.IsNested && !type.IsInterface)
                    .Where(type => !type.IsAbstract || type.IsSealed) // 包含普通类和静态类
                    .Where(type => type.GetCustomAttribute<MarkIsCommandAttribute>()!=null); // 性能优化：不实例化特性

                foreach (var type in allTypes)
                {
                    var categoryAttrs = type.GetCustomAttributes<DeviceCategoryAttribute>().ToArray();

                    // 获取分类名称，没有则归入"无分类"
                    var categoryNames = categoryAttrs.Length > 0
                        ? categoryAttrs.Select(c => c.CategoryName)
                        : new[] { "无分类" };
                    List<MethodInfo> methodInfos = new List<MethodInfo>();
                    GetMethodByType(type, methodInfos);
                    // 3. 将数据填入 嵌套字典 中
                    foreach (var categoryName in categoryNames)
                    {
                        // 第一步：确保外层字典有这个分类
                        if (!CommandTypesByCategory.ContainsKey(categoryName))
                        {
                            CommandTypesByCategory[categoryName] = new Dictionary<Type, List<MethodInfo>>();
                        }

                        var innerDict = CommandTypesByCategory[categoryName];

                        // 第二步：确保内层字典有这个 Type，并将方法列表赋值进去
                        // 如果一个类同时被两个分类标注，这里不会报错，而是会正确添加
                        if (!innerDict.ContainsKey(type))
                        {
                            innerDict[type] = methodInfos;
                        }
                    }

                }
            }
        }
        /// <summary>
        /// 引用类型:只要不在方法内部再new，传入后会自动改变传入的类型的值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodInfos"></param>
        private void GetMethodByType(Type type,List<MethodInfo> methodInfos)
        {
            //BindingFlags是一个位标志枚举（[Flags]），用来控制 Type.GetMethods、Type.GetFields 等反射 API 找成员时的范围和条件
            var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(method=> !method.IsSpecialName && method.DeclaringType!=typeof(object))
                .ToList();
            //按照“方法签名（方法名 + 参数类型）”进行分组，把名字和参数完全一样的方法抓到同一个组里
            //Key: { Name="Move", Parameters="System.Int32,System.Int32"}
            var groupedMethods = allMethods.GroupBy(m=>new {m.Name,Parameters = string.Join(",",m.GetParameters().Select(p=>p.ParameterType.FullName))});
            foreach (var group in groupedMethods)
            {
                MethodInfo? selectedMethod = null;
                int minDepth = int.MaxValue;
                foreach (var method in group)
                {
                    int depth = 0;
                    Type? current = type;
                    Type declaringType = method.DeclaringType!;
                    while (current != null && current != declaringType)
                    {
                        depth++;
                        current = current.BaseType;
                    }

                    if (current == declaringType)
                    {
                        if (selectedMethod == null || depth < minDepth)
                        {
                            selectedMethod = method;
                            minDepth = depth;
                        }
                    }
                }

                if (selectedMethod != null)
                {
                    methodInfos.Add(selectedMethod);
                }        
 
            }

        }

        /// <summary>
        /// 运行时热重载
        /// </summary>
        public async Task ReloadAsync()
        {
            await InitializeAsync();
        }
    }
}