using LiveCharts;
using LiveCharts.Defaults;
using MonitoringSystem.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Model
{
    public class MonitorValueModel : NotifyPropertyBase
    {
        public Action<MonitorValueState, string,string> ValueStorageChanged;

        public string ValueId { get; set; }
        public string ValueName { get; set; }
        public string StorageAreaId { get; set; }
        public int StartAddress { get; set; }
        public string DataType { get; set; }
        public bool IsAlarm { get; set; }
        public double LoLoAlarm { get; set; }
        public double LowAlarm { get; set; }
        public double HighAlarm { get; set; }
        public double HiHiAlarm  { get; set; }
        public int DeviceId { get; set; }

        public string Unit { get; set; }


        private double _currentValue;

        public double CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = value;
                MonitorValueState state = MonitorValueState.OK;
                if (IsAlarm)
                {
                    string msg = ValueDesc;
                    if (value < LoLoAlarm)
                    { msg += "极低"; state = MonitorValueState.LoLo; }
                    else if (value < LowAlarm)
                    { msg += "过低"; state = MonitorValueState.Low; }
                    else if (value > HiHiAlarm)
                    { msg += "极高"; state = MonitorValueState.HiHi; }
                    else if (value > HighAlarm)
                    { msg += "过高"; state = MonitorValueState.High; } 
                    ValueStorageChanged(state, msg + "。当前值:" + value.ToString(),ValueId);

                 
                }
                RaisePropertyChanged();
                Values.Add(new ObservableValue(value));
                if (Values.Count > 60) Values.RemoveAt(0);
            }
        }

        public ChartValues<ObservableValue> Values { get; set; } = new ChartValues<ObservableValue>();  
        public String ValueDesc { get; set; }

    }
}
