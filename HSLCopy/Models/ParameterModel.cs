using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSLCopy.Models
{
    public partial class ParameterModel : ObservableObject
    {
        #region 构造函数

        public ParameterModel()
        {
        }

        public ParameterModel(ParameterModel source)
        {
            if (source == null) return;

            _id = source.Id;
            _isVisible = source.IsVisible;
            _name = source.Name;
            _type = source.Type;
            _category = source.Category;
            _isGlobal = source.IsGlobal;
            _value = source.Value;
            _lowerLimit = source.LowerLimit;
            _upperLimit = source.UpperLimit;
            _result = source.Result;
            _isUseVar = source.IsUseVar;
            _isSave = source.IsSave;
            _variableName = source.VariableName;
            _variableID = source.VariableID;
            _isOutputToReport = source.IsOutputToReport;
        }

        #endregion

        // ★ 4. 属性全部变成 private 字段，加上下划线前缀 _，打上标签
        // Toolkit 会自动将 _id 转换成公共属性 ID，并保持原有的默认值

        [ObservableProperty]
        private Guid _id = Guid.NewGuid();

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Type? _type = typeof(string);

        [ObservableProperty]
        private ParameterCategory _category = ParameterCategory.Temp;

        [ObservableProperty]
        private bool _isGlobal;

        [ObservableProperty]
        private object? _value;

        [ObservableProperty]
        private object? _lowerLimit;

        [ObservableProperty]
        private object? _upperLimit;

        [ObservableProperty]
        private bool _result = true;

        [ObservableProperty]
        private bool _isUseVar;

        [ObservableProperty]
        private bool _isSave;

        [ObservableProperty]
        private string? _variableName;

        [ObservableProperty]
        private Guid? _variableID;

        /// <summary>
        /// 是否输出到报告（仅对输出参数有效，默认为false）
        /// </summary>
        [ObservableProperty]
        private bool _isOutputToReport = false;

        public enum ParameterCategory
        {
            Input,
            Output,
            Temp
        }
    }
}
