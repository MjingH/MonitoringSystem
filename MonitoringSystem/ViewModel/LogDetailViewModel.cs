using MonitoringSystem.Base;
using MonitoringSystem.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonitoringSystem.ViewModel
{
    public class LogDetailViewModel : NotifyPropertyBase
    {
        private ObservableCollection<LogModel> _logList;

        public ObservableCollection<LogModel> LogList
        {
            get => _logList;
            set
            {
                _logList = value;
                RaisePropertyChanged();
            }
        }

        public LogDetailViewModel()
        {

            LogList = GlobalMonitor.AllLogList;

        //var copiedList = new ObservableCollection<LogModel>();
        //foreach (var log in GlobalMonitor.AllLogList)
        //{ 
        //    copiedList.Add(new LogModel
        //    {
        //        RowNumber = log.RowNumber,
        //        DeviceName = log.DeviceName,
        //        LogInfo = log.LogInfo,
        //        LogType = log.LogType,
        //        Message = log.Message,
        //        // 复制其他属性...
        //    });
        //}
        //LogList = copiedList;
    }
    }
}