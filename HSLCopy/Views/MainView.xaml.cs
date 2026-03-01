using CommunityToolkit.Mvvm.Messaging;
using HSLUILearnDome.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HSLCopy.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitData();
            InitRegisterMessage();
        }
        /// <summary>
        /// 注册跳转页面，当点击时
        /// </summary>
        /*   this.container.Content = App.Current.Services.GetService<LoginView>();
             //登陆成功，则跳转到主界面(来自LoginViewModel的消息注册)
             WeakReferenceMessenger.Default.Register<LoginMessage>(this, (sender, arg) =>
             {
                container.Content = App.Current.Services.GetService<MainView>();
             });*/
        private void InitRegisterMessage()
        {
            // 默认首页
            Page.Content = App.Current.Services.GetService<IndexView>();

            // 注册接收字符串消息
            WeakReferenceMessenger.Default.Register<string>(this, (sender, viewName) =>
            {
                // 直接从 DI 容器中根据类名获取实例（前提是你在 App.xaml.cs 注册了这些 View）
                // 这里的逻辑：HSLCopy.Views.S71200View
                string fullTypeName = $"HSLCopy.Views.{viewName}";
                Type? type = Type.GetType(fullTypeName);
                if (type != null)
                {
                    //通过IOC获取view
                    var view = App.Current.Services.GetService(type);
                    if (view != null)
                    {
                        Page.Content = view;
                    }
                }
            });
        }
        private void InitData()
        {
            //要在app里注册(服务器依赖注入)
            DataContext = App.Current.Services.GetService<MainViewModel>();

        }

    }
}
