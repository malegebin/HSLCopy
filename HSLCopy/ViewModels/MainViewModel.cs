using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HSLCopy.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSLUILearnDome.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        /// <summary>
        ///按钮被点击需导航到固定页面
        /// </summary>
        /// <param name="menu"></param>

        [RelayCommand]
        void Navigation(string tag)
        {
            WeakReferenceMessenger.Default.Send(tag);
        }

    }
}
