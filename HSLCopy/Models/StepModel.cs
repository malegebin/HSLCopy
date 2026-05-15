using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HSLCopy.Models
{
    public partial class StepModel : ObservableObject
    {
        #region 构造函数

        public StepModel()
        {
        }

        public StepModel(StepModel source)
        {
            if (source == null) return;

            _id = source.Id;
            _index = source.Index;
            _stepName = source.StepName;
            _stepType = source.StepType;
            _loopCount = source.LoopCount;
            _loopStartStepId = source.LoopStartStepId;
            _okExpression = source.OkExpression;
            _okGotoStepID = source.OkGotoStepID;
            _ngGotoStepID = source.NgGotoStepID;
            _description = source.Description;
            _isUsed = source.IsUsed;

            if (source.Method != null)
            {
                _method = new MethodModel(source.Method);
            }
            //if (source.SubProgram != null)
            //{
            //    _subProgram = new ProgramModel(source.SubProgram);
            //}
        }

        #endregion

        [ObservableProperty]
        private Guid? _id = Guid.NewGuid();

        [ObservableProperty]
        private bool _isUsed = true;

        [ObservableProperty]
        private int _index;

        [ObservableProperty]
        private string? _stepName;

        [ObservableProperty]
        private string? _stepType;

        [ObservableProperty]
        private MethodModel? _method;

        //[ObservableProperty]
        //private ProgramModel? _subProgram;

        /// <summary>
        /// 仅LoopStart使用
        /// </summary>
        [ObservableProperty]
        private int? _loopCount;

        /// <summary>
        /// 运行时循环计数
        /// </summary>
        [ObservableProperty]
        [JsonIgnore]
        private int? _currentLoopCount;

        /// <summary>
        /// 仅LoopEnd使用，关联LoopStart
        /// </summary>
        [ObservableProperty]
        private Guid? _loopStartStepId;

        /// <summary>
        /// 初始：-1  运行中：0  成功：1  异常：2
        /// </summary>
        [ObservableProperty]
        [property: JsonIgnore]
        private int _result = -1;

        [ObservableProperty]
        [property: JsonIgnore]
        private int? _runTime;

        [ObservableProperty]
        private string? _okExpression;

        /// <summary>
        /// 默认为0/0不跳转，第一位为OK跳转的步骤序号，第二位为NG跳转的步骤序号
        /// </summary>
        [ObservableProperty]
        private string _gotoSettingString = "";

        [ObservableProperty]
        private Guid? _okGotoStepID;

        [ObservableProperty]
        private Guid? _ngGotoStepID;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        [property: JsonIgnore]
        private bool? _isBrokenpoint;
    }
}
