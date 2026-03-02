using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSLCopy.ViewModels
{
    public partial class S71200ViewModel : ObservableObject
    {
        /// <summary>
        ///按钮被点击需导航到固定页面
        /// </summary>
        [RelayCommand]
        void WithIpNavigation(string tag)
        {
            WeakReferenceMessenger.Default.Send<string, string>(tag, "IpPageChangeToken");
        }
    }
}
