using HSLCopy.Models;
using HSLCopy.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace HSLCopy.Views
{
    public partial class CommandView : UserControl
    {
        public CommandView()
        {
            InitializeComponent();

            // 直接从 DI 容器获取 ViewModel 并赋值给 DataContext
            this.DataContext = App.Current.Services.GetRequiredService<CommandViewModel>();
        }
 

        private void AddMethosCommand()
        {

        }

        private void AddToStepsRunning_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(InitCommandTreeView.SelectedItem is TreeNodeModel selectedNode)
            {
                // 2. 拿到 ViewModel，直接调用它的命令
                if (DataContext is CommandViewModel commandViewModel)
                {
                    commandViewModel.AddToStepsCommand.Execute(selectedNode);
                }
            }

        }
    }
}