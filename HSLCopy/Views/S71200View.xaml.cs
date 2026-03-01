using HSLUILearnDome.ViewModels;
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
        }
        private void InitData()
        {
            //要在app里注册(服务器依赖注入)
            DataContext = App.Current.Services.GetService<MainViewModel>();

        }
    }
}
