using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HSLCopy.Models
{
    public partial class RunProgramModel : ObservableObject
    {
        public RunProgramModel()
        {
        }

        public RunProgramModel(RunProgramModel source)
        {
            _id = source.Id;
            StepCollection = new(source.StepCollection.Select(p => new StepModel(p)));
            //Devices = new(source.Devices.Select(p => new DeviceModel(p)));
            //Parameters = new(source.Parameters.Select(p => new ParameterModel(p)));
        }

        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private ObservableCollection<StepModel> _stepCollection = [];
        //public ObservableCollection<ParameterModel> Parameters { get; set; } = [];

        //public ObservableCollection<DeviceModel> Devices { get; set; } = [];
    }
}