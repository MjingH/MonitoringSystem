using MonitoringSystem.Base;

namespace MonitoringSystem.Model
{
    /// <summary>
    /// 报表记录项模型：用于在报表管理中展示一条运行日志记录
    /// </summary>
    public class ReportItemModel : NotifyPropertyBase
    {
        /// <summary>序号</summary>
        public int RowNumber { get; set; }

        /// <summary>发生时间</summary>
        public System.DateTime ReportTime { get; set; }

        /// <summary>设备名称</summary>
        public string DeviceName { get; set; }

        /// <summary>事件描述（如：已启动 / 已关闭 / 报警）</summary>
        public string EventInfo { get; set; }

        /// <summary>日志类型</summary>
        public LogType LogType { get; set; }

        /// <summary>日志详细信息</summary>
        public string Message { get; set; }

        /// <summary>日志类型的中文显示文本</summary>
        public string LogTypeText
        {
            get
            {
                switch (LogType)
                {
                    case LogType.Warn: return "警告";
                    case LogType.Fault: return "故障";
                    default: return "信息";
                }
            }
        }

        /// <summary>用于界面表格显示的时间字符串</summary>
        public string ReportTimeText => ReportTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
