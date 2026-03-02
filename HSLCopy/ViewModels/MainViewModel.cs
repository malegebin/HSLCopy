using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HSLCopy.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSLCopy.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        /// <summary>
        ///按钮被点击需导航到固定页面
        /// </summary>
        /// <param name="menu"></param>

        //tag 是传进来的参数Tag=""的值
        [RelayCommand]
        void Navigation(string tag)
        {
            WeakReferenceMessenger.Default.Send<string, string>(tag, "MainPageChangeToken");
        }

    }
}
