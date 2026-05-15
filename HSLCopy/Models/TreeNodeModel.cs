using System.Collections.ObjectModel;

namespace HSLCopy.Models
{
    public class TreeNodeModel
    {
        // 节点显示的名称
        public string HeaderName { get; set; } = string.Empty;

        // 节点携带的隐藏数据
        public object? Tag { get; set; }

        // ★ 新增：鼠标悬停提示（配合 ViewModel 赋值使用）
        public string? ToolTip { get; set; }

        // 子节点集合
        public ObservableCollection<TreeNodeModel> Children { get; set; } = new();
    }
}