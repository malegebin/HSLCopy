using CommunityToolkit.Mvvm.Messaging;
using HSLCopy.Usc;
using HSLCopy.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HSLCopy.Views
{
    /// <summary>
    /// S71200View.xaml 的交互逻辑
    /// </summary>
    public partial class S71200View : UserControl
    {
        public S71200View()
        {
            InitializeComponent();
            InitData();
            InitRegisterMessage();
        }
        private void InitRegisterMessage()
        {       //管理委托的
                //WeakDelegatesManager
                //用于应用程序内不同组件之间的消息通信，实现完全解耦

            IPChangePage.Content = App.Current.Services.GetService<TcpIpControl>();
            WeakReferenceMessenger.Default.Register<string, string>(this, "IpPageChangeToken", (sender, arg) =>
            {
                // 直接从 DI 容器中根据类名获取实例（前提是你在 App.xaml.cs 注册了这些 View）
                // 这里的逻辑：HSLCopy.Views.S71200View
                string fullTypeName = $"HSLCopy.Usc.{arg}";
                Type? type = Type.GetType(fullTypeName);
                if (type != null)
                {
                    //通过IOC获取view
                    var view = App.Current.Services.GetService(type);
                    if (view != null)
                    {
                        IPChangePage.Content = view;
                    }
                }

            });
        }
        private void InitData()
        {
            //要在app里注册(服务器依赖注入)
            DataContext = App.Current.Services.GetService<S71200ViewModel>();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
