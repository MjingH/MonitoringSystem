using MonitoringSystem.Base;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MonitoringSystem.ViewModel
{
    public class SystemMonitorViewModel : NotifyPropertyBase
    {
       
        public ObservableCollection<LogModel> LogList { get; set; } = new ObservableCollection<LogModel>();

        public DeviceModel TestDevice { get; set; }

        public CommandBase conponentCommand { get; set; }


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

        private void DoTowerCommand(object param)
        {
            CurrentDevice = param as DeviceModel;
            IsShowdatil = true;
        }
        public SystemMonitorViewModel()
        {
            InitLogInfo();

            this.conponentCommand = new CommandBase(new Action<object>(DoTowerCommand));
        }
        void InitLogInfo()
        {
            this.LogList.Add(new LogModel { RowNumber = 1, DeviceName = "冷却塔 1#", LogInfo = "已启动", LogType = Base.LogType.Info });
            this.LogList.Add(new LogModel { RowNumber = 2, DeviceName = "冷却塔 2#", LogInfo = "已启动", LogType = Base.LogType.Info });
            this.LogList.Add(new LogModel { RowNumber = 3, DeviceName = "冷却塔 3#", LogInfo = "液位极低", LogType = Base.LogType.Warn });
            this.LogList.Add(new LogModel { RowNumber = 4, DeviceName = "循环水泵 1#", LogInfo = "频率过大", LogType = Base.LogType.Warn });
            this.LogList.Add(new LogModel { RowNumber = 5, DeviceName = "循环水泵 2#", LogInfo = "已启动", LogType = Base.LogType.Info });
            this.LogList.Add(new LogModel { RowNumber = 6, DeviceName = "循环水泵 3#", LogInfo = "已启动", LogType = Base.LogType.Info });

            //  测试数据
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
