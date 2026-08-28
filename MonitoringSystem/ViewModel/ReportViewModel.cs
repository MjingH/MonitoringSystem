using Microsoft.Win32;
using MonitoringSystem.Base;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 报表管理 ViewModel：负责报表数据的加载、条件查询、统计汇总与导出
    /// </summary>
    public class ReportViewModel : NotifyPropertyBase
    {
        // ================= 数据源 =================
        private readonly List<ReportItemModel> _allItems = new List<ReportItemModel>();

        // ================= 绑定到界面的集合 =================
        private ObservableCollection<ReportItemModel> _reportList = new ObservableCollection<ReportItemModel>();
        public ObservableCollection<ReportItemModel> ReportList
        {
            get => _reportList;
            set { Set(ref _reportList, value); }
        }

        private ObservableCollection<string> _deviceNames = new ObservableCollection<string>();
        public ObservableCollection<string> DeviceNames
        {
            get => _deviceNames;
            set { Set(ref _deviceNames, value); }
        }

        /// <summary>日志类型下拉选项</summary>
        public List<string> LogTypeOptions { get; } = new List<string>
        {
            "全部类型", "信息", "警告", "故障"
        };

        // ================= 查询条件 =================
        private string _selectedDevice;
        public string SelectedDevice
        {
            get => _selectedDevice;
            set { Set(ref _selectedDevice, value); ApplyFilter();  }
        }

        private string _selectedLogType;
        public string SelectedLogType
        {
            get => _selectedLogType;
            set { Set(ref _selectedLogType, value); ApplyFilter(); }
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            set { Set(ref _startTime, value); ApplyFilter();  }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get => _endTime;
            set { Set(ref _endTime, value); ApplyFilter();  }
        }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set { Set(ref _keyword, value); ApplyFilter(); }
        }

        // ================= 统计信息 =================
        private int _totalCount;
        public int TotalCount { get => _totalCount; set { Set(ref _totalCount, value); } }

        private int _infoCount;
        public int InfoCount { get => _infoCount; set { Set(ref _infoCount, value); } }

        private int _warnCount;
        public int WarnCount { get => _warnCount; set { Set(ref _warnCount, value); } }

        private int _faultCount;
        public int FaultCount { get => _faultCount; set { Set(ref _faultCount, value); } }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { Set(ref _statusMessage, value); }
        }

        // ================= 命令 =================
        public CommandBase QueryCommand { get; set; }
        public CommandBase ResetCommand { get; set; }
        public CommandBase ExportCommand { get; set; }

        public ReportViewModel()
        {

            MonitorSystemBLL.OnNewLogAdded += UpdataData;

            QueryCommand = new CommandBase(o => ApplyFilter());
            ResetCommand = new CommandBase(o => ResetFilter());
            ExportCommand = new CommandBase(o => Export());

            InitDeviceNames();
            InitData();
            ResetFilter();
        }

        private void UpdataData()
        {
            InitDeviceNames();
            InitData();
            ApplyFilter();
        }

        /// <summary>初始化设备名称下拉选项</summary>
        private void InitDeviceNames()
        {
            DeviceNames = new ObservableCollection<string> { "全部设备" };
            if (GlobalMonitor.DeviceList != null)
            {
                foreach (var d in GlobalMonitor.DeviceList.Where(d => !string.IsNullOrEmpty(d.DeviceName)))
                {
                    DeviceNames.Add(d.DeviceName);
                }
            }
        }

        /// <summary>
        /// 初始化报表数据：
        /// 先汇总系统真实运行日志，再生成一段演示数据，便于界面展示与功能验证
        /// </summary>
        private void InitData()
        {
            _allItems.Clear();

            // 1) 汇总系统真实运行日志（AllLogList 与 LogList 存在共享引用，按引用去重）
            var logs = new List<LogModel>();
            if (GlobalMonitor.AllLogList != null) logs.AddRange(GlobalMonitor.AllLogList);
            if (GlobalMonitor.LogList != null) logs.AddRange(GlobalMonitor.LogList);

            var seen = new HashSet<LogModel>();
            foreach (var log in logs)
            {
                if (log == null || !seen.Add(log)) continue;
                _allItems.Add(new ReportItemModel
                {
                    RowNumber = _allItems.Count + 1,
                    DeviceName = string.IsNullOrEmpty(log.DeviceName) ? "未知设备" : log.DeviceName,
                    EventInfo = string.IsNullOrEmpty(log.LogInfo) ? "—" : log.LogInfo,
                    LogType = log.LogType,
                    Message = string.IsNullOrEmpty(log.Message) ? "—" : log.Message,
                    ReportTime = ParseTime(log.AlarmTime)
                });
            }

            // 2) 生成演示数据
            //GenerateDemoData();
        }

        /// <summary>生成演示报表数据（覆盖最近 7 天）</summary>
        //private void GenerateDemoData()
        //{
        //    var devices = new List<string>();
        //    if (GlobalMonitor.DeviceList != null)
        //    {
        //        devices = GlobalMonitor.DeviceList
        //            .Select(d => d.DeviceName)
        //            .Where(n => !string.IsNullOrEmpty(n))
        //            .ToList();
        //    }
        //    if (devices.Count == 0)
        //    {
        //        devices = new List<string>
        //        {
        //            "冷却塔 1#", "冷却塔 2#", "冷却塔 3#",
        //            "冷却泵 1#", "冷却泵 2#", "循环泵 1#"
        //        };
        //    }

        //    var events = new List<(LogType type, string evt, string msg)>
        //    {
        //        (LogType.Info,  "已启动",   "设备正常启动，进入运行状态"),
        //        (LogType.Info,  "数据采集", "采集周期完成，数据写入正常"),
        //        (LogType.Warn,  "液位过低", "液位低于下限阈值，请检查补水"),
        //        (LogType.Warn,  "压力过高", "入口压力超过上限阈值"),
        //        (LogType.Fault, "通讯中断", "Modbus 通讯超时，设备离线"),
        //        (LogType.Fault, "设备故障", "设备异常停机，请及时处理")
        //    };

        //    var rnd = new Random();
        //    var now = DateTime.Now;

        //    for (int i = 0; i < 40; i++)
        //    {
        //        var e = events[rnd.Next(events.Count)];
        //        var device = devices[rnd.Next(devices.Count)];
        //        _allItems.Add(new ReportItemModel
        //        {
        //            RowNumber = _allItems.Count + 1,
        //            DeviceName = device,
        //            EventInfo = e.evt,
        //            LogType = e.type,
        //            Message = e.msg,
        //            ReportTime = now
        //                .AddDays(-rnd.Next(0, 7))
        //                .AddHours(-rnd.Next(0, 23))
        //                .AddMinutes(-rnd.Next(0, 59))
        //        });
        //    }
        //}

        /// <summary>把日志的时间字符串解析为 DateTime，解析失败则返回当前时间</summary>
        private DateTime ParseTime(string text)
        {
            if (DateTime.TryParse(text, out var t)) return t;
            return DateTime.Now;
        }

        /// <summary>重置查询条件并重新查询</summary>
        private void ResetFilter()
        {
            SelectedDevice = "全部设备";
            SelectedLogType = "全部类型";
            StartTime = null;
            EndTime = null;
            Keyword = string.Empty;
            StatusMessage = string.Empty;
            ApplyFilter();
        }

        /// <summary>根据查询条件过滤数据并刷新统计</summary>
        private void ApplyFilter()
        {
            IEnumerable<ReportItemModel> query = _allItems;

            // 设备过滤
            if (!string.IsNullOrEmpty(SelectedDevice) && SelectedDevice != "全部设备")
            {
                query = query.Where(i => i.DeviceName == SelectedDevice);
            }

            // 日志类型过滤
            if (!string.IsNullOrEmpty(SelectedLogType) && SelectedLogType != "全部类型")
            {
                query = query.Where(i => i.LogTypeText == SelectedLogType);
            }

            // 开始时间过滤
            if (StartTime.HasValue)
            {
                query = query.Where(i => i.ReportTime >= StartTime.Value);
            }

            // 结束时间过滤（包含结束日整天）
            if (EndTime.HasValue)
            {
                var end = EndTime.Value.Date.AddDays(1);
                query = query.Where(i => i.ReportTime < end);
            }

            // 关键字过滤（设备名称 / 事件 / 信息）
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                var kw = Keyword.Trim();
                query = query.Where(i =>
                    (i.DeviceName != null && i.DeviceName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (i.EventInfo != null && i.EventInfo.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (i.Message != null && i.Message.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            var list = query.OrderByDescending(i => i.ReportTime).ToList();
            for (int i = 0; i < list.Count; i++) list[i].RowNumber = i + 1;

            ReportList = new ObservableCollection<ReportItemModel>(list);
            UpdateStatistics(list);
            StatusMessage = $"共查询到 {list.Count} 条记录";
        }

        /// <summary>更新统计汇总</summary>
        private void UpdateStatistics(List<ReportItemModel> list)
        {
            TotalCount = list.Count;
            InfoCount = list.Count(i => i.LogType == LogType.Info);
            WarnCount = list.Count(i => i.LogType == LogType.Warn);
            FaultCount = list.Count(i => i.LogType == LogType.Fault);
        }

        /// <summary>导出当前查询结果为 CSV 文件</summary>
        private void Export()
        {
            if (ReportList == null || ReportList.Count == 0)
            {
                MessageBox.Show("当前没有可导出的报表数据。", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "导出报表",
                Filter = "CSV 文件 (*.csv)|*.csv",
                FileName = $"运行报表_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("序号,时间,设备名称,事件,日志类型,日志信息");
                foreach (var item in ReportList)
                {
                    sb.AppendLine(string.Join(",",
                        item.RowNumber,
                        item.ReportTimeText,
                        EscapeCsv(item.DeviceName),
                        EscapeCsv(item.EventInfo),
                        item.LogTypeText,
                        EscapeCsv(item.Message)));
                }

                // UTF-8 带 BOM，便于 Excel 正确识别中文
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));

                StatusMessage = $"导出成功：{dlg.FileName}";
                MessageBox.Show($"报表已成功导出到：\n{dlg.FileName}", "导出成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = "导出失败";
                MessageBox.Show("导出失败：" + ex.Message, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>转义 CSV 字段（处理逗号、引号和换行）</summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
