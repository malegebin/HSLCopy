using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HSLCopy.Models
{
    public partial class MethodModel : ObservableObject
    {
        public MethodModel()
        {
        }

        public MethodModel(MethodModel source)
        {
            if (source == null) return;

            _name = source.Name;
            _fullName = source.FullName;

            //Parameters = new ObservableCollection<ParameterModel>(
            //    source.Parameters.Select(p => new ParameterModel(p)));
        }

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _fullName;

        [ObservableProperty]
        private ObservableCollection<ParameterModel> _parameters = [];
    }
}