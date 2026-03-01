using HSLCopy.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace HSLCopy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //toolkit Ioc 控制反转
        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; private set; }


        public App()
        {
            Services = ConfigureService();
            this.InitializeComponent();
        }

        //启动项
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

        }
        //做所有依赖注入的事情spring@Autowired	.net构造函数注入
        private IServiceProvider? ConfigureService()
        {
            var services = new ServiceCollection();
            //配置类实现
            //ConfigureJsonByBinder(services);
            //配置GlobalConfig
            //services.AddSingleton<GlobalConfig>();

            // 1. 手动注入全局单例基础服务
            //services.AddSingleton<UserSession>();

            // 2. 获取当前程序集
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // --- 自动注册 ViewModels ---
            // 扫描所有以 "ViewModel" 结尾的类
            var viewModels = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel"));

            foreach (var vm in viewModels)
            {
                services.AddSingleton(vm);
            }

            // --- 自动注册 Views ---
            // 扫描所有在 .Views 命名空间下的类（或者以 "View" 结尾的类）
            var views = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("View"));

            foreach (var view in views)
            {
                // View 建议使用 AddTransient (瞬时)，
                // 这样切换用户或重新打开页面时会创建新实例，避免视觉树冲突
                services.AddTransient(view);
            }

            // 特殊处理：ShellView 作为主壳通常还是单例比较好
            //services.AddSingleton<ShellView>();

            return services.BuildServiceProvider();
        }

        //将配置文件（JSON）中的数据“注入”到你定义的 C# 实体类
        private void ConfigureJsonByBinder(ServiceCollection services)
        {

        }
    }

}
