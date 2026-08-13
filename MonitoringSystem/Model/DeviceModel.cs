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
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }

        //public bool IsRuning { get; set; }

        private bool _isRuning;

        public bool IsRuning
        {
            get { return _isRuning; }
            set 
            { 
                _isRuning = value;
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
