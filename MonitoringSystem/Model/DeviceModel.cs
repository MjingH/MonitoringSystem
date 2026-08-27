using MonitoringSystem.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Model
{
    public class DeviceModel : NotifyPropertyBase
    {
        // 定义事件
        public Action<int,bool> RunningStateChanged;

        public int DeviceId { get; set; }
        public string DeviceName { get; set; }


        public LogType LogType { get; set; }
        //public bool IsRuning { get; set; }

        private bool _isRuning;

        public bool IsRuning
        {
            get { return _isRuning; }
            set 
            { 
                _isRuning = value;
                /* foreach (var lt in GlobalMonitor.LogList.Where(m => m.RowNumber == DeviceId))
                 {
                     lt.LogInfo = value?"已启动":"已关闭";
                 }*/

                // 触发状态变更事件，把当前实例传出去
                RunningStateChanged?.Invoke(DeviceId, value);

                this.RaisePropertyChanged();
            }
        }

        //public bool IsWarning { get; set; } = false;

        private bool _isWarning;

        public bool IsWarning
        {
            get { return _isWarning; }
            set 
            { 
                _isWarning = value; 
                this.RaisePropertyChanged();
            }
        }


        public ObservableCollection<MonitorValueModel> MonitorValueList { get; set; } = 
            new ObservableCollection<MonitorValueModel>();

        public ObservableCollection<WarningMessageModel> WarningMessageList { get; set; } = 
            new ObservableCollection<WarningMessageModel>();
    }
}
