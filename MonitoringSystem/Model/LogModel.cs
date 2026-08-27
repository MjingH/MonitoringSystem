using MonitoringSystem.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Model
{
    public class LogModel : NotifyPropertyBase
    {

        public string AlarmTime { get; set; }

        public int RowNumber { get; set; }

        //public List<string> DeviceNameList { get; set; }

        private string _deviceName;

        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; RaisePropertyChanged(); }
        }


        //public string LogInfo { get; set; }
        private string _logInfo;

        public string LogInfo
        {
            get { return _logInfo; }
            set { _logInfo = value; RaisePropertyChanged(); }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }



        //public ObservableCollection<WarningMessageModel> WarningMessageList { get; set; } = new ObservableCollection<WarningMessageModel>();

        //  public LogType LogType { get; set; }
        private LogType _logType;

        public LogType LogType
        {
            get { return _logType; }
            set { _logType = value;RaisePropertyChanged(); }
        }

    }
}
