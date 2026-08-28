using MonitoringSystem.Base;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
using MonitoringSystem.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 报警管理 ViewModel：汇总设备报警状态与报警日志，
    /// 支持按设备/类型/关键字过滤、刷新、查看日志详情以及清除报警
    /// </summary>
    public class AlarmManegerViewModel : NotifyPropertyBase, IDisposable
    {
        // ================= 集合 =================
        /// <summary>设备列表（直接引用全局静态数据，报警状态实时联动）</summary>
        public List<DeviceModel> DeviceList => GlobalMonitor.DeviceList ?? new List<DeviceModel>();

        private ObservableCollection<LogModel> _alarmLogList = new ObservableCollection<LogModel>();
        public ObservableCollection<LogModel> AlarmLogList
        {
            get => _alarmLogList;
            set { Set(ref _alarmLogList, value); }
        }

        private DeviceModel _selectedDevice;
        public DeviceModel SelectedDevice
        {
            get => _selectedDevice;
            set { Set(ref _selectedDevice, value); }
        }

        // ================= 统计 =================
        private int _activeAlarmCount;
        public int ActiveAlarmCount { get => _activeAlarmCount; set { Set(ref _activeAlarmCount, value); } }

        private int _totalAlarmCount;
        public int TotalAlarmCount { get => _totalAlarmCount; set { Set(ref _totalAlarmCount, value); } }

        private int _warnCount;
        public int WarnCount { get => _warnCount; set { Set(ref _warnCount, value); } }

        private int _faultCount;
        public int FaultCount { get => _faultCount; set { Set(ref _faultCount, value); } }

        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set { Set(ref _statusMessage, value); } }

        // ================= 过滤条件 =================
        public ObservableCollection<string> DeviceNameOptions { get; } = new ObservableCollection<string>();

        public List<string> LogTypeOptions { get; } = new List<string> { "全部类型", "警告", "故障" };

        private string _selectedDeviceName = "全部设备";
        public string SelectedDeviceName { get => _selectedDeviceName; set { Set(ref _selectedDeviceName, value); } }

        private string _selectedLogType = "全部类型";
        public string SelectedLogType { get => _selectedLogType; set { Set(ref _selectedLogType, value); } }

        private string _keyword;
        public string Keyword { get => _keyword; set { Set(ref _keyword, value); } }

        // ================= 命令 =================
        public CommandBase QueryCommand { get; set; }
        public CommandBase ResetCommand { get; set; }
        public CommandBase RefreshCommand { get; set; }
        public CommandBase ShowDetailCommand { get; set; }
        public CommandBase ClearAlarmCommand { get; set; }

        public AlarmManegerViewModel()
        {
            QueryCommand = new CommandBase(o => ApplyFilter());
            ResetCommand = new CommandBase(o => ResetFilter());
            RefreshCommand = new CommandBase(o => Refresh());
            ShowDetailCommand = new CommandBase(o => OpenLogDetail(o));
            ClearAlarmCommand = new CommandBase(o => ClearAlarm(o));

            // 有新报警日志产生时，刷新统计与列表
            MonitorSystemBLL.OnNewLogAdded += OnNewLogAdded;

            InitDeviceOptions();
            Refresh();
        }

        /// <summary>初始化设备下拉选项</summary>
        private void InitDeviceOptions()
        {
            DeviceNameOptions.Clear();
            DeviceNameOptions.Add("全部设备");
            foreach (var d in DeviceList.Where(d => d != null && !string.IsNullOrEmpty(d.DeviceName)))
            {
                DeviceNameOptions.Add(d.DeviceName);
            }
        }

        /// <summary>刷新统计、过滤结果，并保证有默认选中设备</summary>
        public void Refresh()
        {
            ApplyFilter();

            if (DeviceList == null || DeviceList.Count == 0)
            {
                SelectedDevice = null;
                return;
            }

            if (SelectedDevice == null || !DeviceList.Contains(SelectedDevice))
            {
                SelectedDevice = DeviceList.First();
            }
        }

        /// <summary>按条件过滤报警日志并更新统计</summary>
        private void ApplyFilter()
        {
            IEnumerable<LogModel> query = GlobalMonitor.AllLogList ?? new ObservableCollection<LogModel>();

            // 报警管理只展示报警/故障，不展示普通信息
            query = query.Where(l => l != null && l.LogType != LogType.Info);

            if (!string.IsNullOrEmpty(SelectedDeviceName) && SelectedDeviceName != "全部设备")
            {
                query = query.Where(l => l.DeviceName == SelectedDeviceName);
            }

            if (!string.IsNullOrEmpty(SelectedLogType) && SelectedLogType != "全部类型")
            {
                var expect = SelectedLogType == "警告" ? LogType.Warn : LogType.Fault;
                query = query.Where(l => l.LogType == expect);
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                var kw = Keyword.Trim();
                query = query.Where(l =>
                    (l.DeviceName != null && l.DeviceName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (l.Message != null && l.Message.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // 全局日志列表本身按“最新在前”维护，保持原始顺序即可
            AlarmLogList = new ObservableCollection<LogModel>(query.ToList());
            UpdateStatistics();
        }

        private void ResetFilter()
        {
            SelectedDeviceName = "全部设备";
            SelectedLogType = "全部类型";
            Keyword = string.Empty;
            ApplyFilter();
        }

        /// <summary>更新统计卡片与底部状态文案</summary>
        private void UpdateStatistics()
        {
            var devices = DeviceList ?? new List<DeviceModel>();
            ActiveAlarmCount = devices.Count(d => d != null && d.IsWarning);

            var logs = GlobalMonitor.AllLogList;
            if (logs != null)
            {
                TotalAlarmCount = logs.Count(l => l != null && l.LogType != LogType.Info);
                WarnCount = logs.Count(l => l != null && l.LogType == LogType.Warn);
                FaultCount = logs.Count(l => l != null && l.LogType == LogType.Fault);
            }
            else
            {
                TotalAlarmCount = 0;
                WarnCount = 0;
                FaultCount = 0;
            }

            StatusMessage = $"当前 {ActiveAlarmCount} 台设备处于报警状态，共 {TotalAlarmCount} 条报警记录";
        }

        /// <summary>打开日志详情窗口</summary>
        private void OpenLogDetail(object o)
        {
            var detailWindow = new LogDetailWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            detailWindow.ShowDialog();
        }

        /// <summary>清除选中设备（或参数传入设备）的报警</summary>
        private void ClearAlarm(object o)
        {
            var device = o as DeviceModel ?? SelectedDevice;
            if (device == null) return;

            device.WarningMessageList.Clear();
            device.IsWarning = false;

            // 对应设备的实时日志恢复为信息类型
            foreach (var lt in GlobalMonitor.LogList.Where(m => m.RowNumber == device.DeviceId))
            {
                lt.LogType = LogType.Info;
                lt.Message = null;
            }

            Refresh();
        }

        /// <summary>新报警产生时刷新（事件已在 UI 线程触发，此处做线程兜底）</summary>
        private void OnNewLogAdded()
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            if (app.Dispatcher.CheckAccess())
                Refresh();
            else
                app.Dispatcher.Invoke(Refresh);
        }

        public void Dispose()
        {
            MonitorSystemBLL.OnNewLogAdded -= OnNewLogAdded;
        }
    }
}
