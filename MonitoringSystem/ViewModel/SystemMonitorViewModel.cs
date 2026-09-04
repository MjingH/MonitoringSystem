using MonitoringSystem.Base;
using MonitoringSystem.Model;
using MonitoringSystem.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;  // ★ 添加这一行

namespace MonitoringSystem.ViewModel
{
    public class SystemMonitorViewModel : NotifyPropertyBase
    {
       
        public ObservableCollection<LogModel> LogList { get; set; } = new ObservableCollection<LogModel>();

        public DeviceModel TestDevice { get; set; }

        public CommandBase conponentCommand { get; set; }

        // private List<string> _deviceNameList;

        public bool IsRunning { get; set; }

        private DeviceModel _currentDevice;

        public DeviceModel CurrentDevice
        {
            get { return _currentDevice; }
            set
            {
                _currentDevice = value;
                this.RaisePropertyChanged();
            }
        }

        private bool _isShowDetail = false;
            
        public bool IsShowdatil
        {
            get { return _isShowDetail; }
            set 
            { 
                _isShowDetail = value;       
                this.RaisePropertyChanged();
            }
        }
        // 详情控件
        public CommandBase ShowDetailCommand { get; set; }

        private void DoTowerCommand(object param)
        {
            CurrentDevice = param as DeviceModel;
            
            IsShowdatil = true;
        }

      

        public SystemMonitorViewModel()
        {
            InitLogInfo();

            this.LogList = GlobalMonitor.LogList;

            this.ShowDetailCommand = new CommandBase(new Action<object>(OnShowDetail));
            this.conponentCommand = new CommandBase(new Action<object>(DoTowerCommand));
        }

        private void OnShowDetail(object param)
        {
            // 打开日志详情窗口
            var detailWindow = new LogDetailWindow();
            detailWindow.Owner = Application.Current.MainWindow;
            //detailWindow.DataContext = this;
            detailWindow.ShowDialog();
        }



        void InitLogInfo()
        {


            //this.LogList.Add(new LogModel { RowNumber = 1,DeviceNameList=GlobalMonitor.DeviceNameList ,LogInfo = "已启动", LogType = Base.LogType.Info });

            //测试数据
            TestDevice = new DeviceModel();
            TestDevice.DeviceName = "冷却塔 1#";
            TestDevice.IsRuning = true;
            TestDevice.IsWarning = true;
            TestDevice.MonitorValueList.Add(new MonitorValueModel
            {
                ValueId = "1",
                ValueName = "液位",
                Unit = "m",
                CurrentValue = 45,
                Values = new LiveCharts.ChartValues<LiveCharts.Defaults.ObservableValue> { new LiveCharts.Defaults.ObservableValue(0),
                new LiveCharts.Defaults.ObservableValue(0) }
            });
            TestDevice.MonitorValueList.Add(new MonitorValueModel
            {
                ValueId = "1",
                ValueName = "入口压力",
                Unit = "Mpa",
                CurrentValue = 34,
                Values = new LiveCharts.ChartValues<LiveCharts.Defaults.ObservableValue> { new LiveCharts.Defaults.ObservableValue(0),
                new LiveCharts.Defaults.ObservableValue(0) }
            });
            TestDevice.WarningMessageList.Add(new WarningMessageModel { Message = "冷却塔1#液位极低，当前值：0" });
            TestDevice.WarningMessageList.Add(new WarningMessageModel { Message = "冷却塔2#液位极低，当前值：0" });
        }
    }
}
