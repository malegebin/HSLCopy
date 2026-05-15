using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Attributes
{
    /// <summary>
    /// 标记可加载到指令集中的类或方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ATSCommandAttribute : Attribute
    {
        /// <summary>
        /// 指令描述（用于工具提示）
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 指令分类（用于树形视图分组）
        /// </summary>
        public string Category { get; set; } = "默认分类";

        public ATSCommandAttribute() { }

        public ATSCommandAttribute(string description)
        {
            Description = description;
        }



        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class DeviceCategoryAttribute : Attribute
        {
            public string Category { get; }

            public DeviceCategoryAttribute(string category)
            {
                Category = category;
            }
        }
    }
}
