using HSLCopy.Config;
using HSLCopy.Models;
using HSLCopy.Usc;
using HSLCopy.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace HSLCopy
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; private set; }

        public App()
        {
            Services = ConfigureService();
            this.InitializeComponent();           
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ★★★ 核心修复：在显示主窗口前，先把全局配置加载出来 ★★★
            var assemblyConfig = Services.GetRequiredService<AssemblyConfig>();
            await assemblyConfig.InitializeAsync(); // 等待DLL加载完毕

        }

        private IServiceProvider? ConfigureService()
        {
            var services = new ServiceCollection();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // ViewModel 全部单例
            var viewModels = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel"));
            foreach (var vm in viewModels) services.AddSingleton(vm);

            // View 全部瞬态（每次新建）
            var views = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("View"));
            foreach (var view in views) services.AddTransient(view);

            services.AddSingleton<MainWindow>();
            //单例复用
            services.AddSingleton<ProductEditView>();
            //注册全局共享的数据模型
            services.AddSingleton(sp => new RunProgramModel());
            services.AddSingleton<AssemblyConfig>();
            

            return services.BuildServiceProvider();
        }
    }
}