using HSLCopy.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace HSLCopy.Views
{
    public partial class StepsRunningView : UserControl
    {
        public StepsRunningView()
        {
            InitializeComponent();
            this.DataContext = App.Current.Services.GetRequiredService<StepsRunningViewModel>();
        }
    }
}