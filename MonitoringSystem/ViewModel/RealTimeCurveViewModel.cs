using LiveCharts;
using LiveCharts.Wpf;
using MonitoringSystem.Base;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 实时曲线 ViewModel：按设备绘制各监控点位的实时趋势曲线（滑动窗口，自动刷新）
    /// </summary>
    public class RealTimeCurveViewModel : NotifyPropertyBase
    {
        /// <summary>设备列表（全局静态数据）</summary>
        public ObservableCollection<DeviceModel> DeviceList { get;set; }

        public string XTitle { get; set; } = "采样点序号";

        private DeviceModel _selectedDevice;
        public DeviceModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice == value) return;
                _selectedDevice = value;
                RaisePropertyChanged();
                RebuildSeries();
            }
        }

        private SeriesCollection _series = new SeriesCollection();
        public SeriesCollection Series
        {
            get => _series;
            set { _series = value; RaisePropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set { Set(ref _statusMessage, value); } }

        /// <summary>每个系列使用的颜色（按点位轮换）</summary>
        private static readonly Brush[] Palette =
        {
            Brushes.Orange, Brushes.DeepSkyBlue, Brushes.LimeGreen,
            Brushes.Gold, Brushes.MediumOrchid, Brushes.Coral,
            Brushes.Turquoise, Brushes.HotPink
        };

        public RealTimeCurveViewModel()
        {
            DeviceList = new ObservableCollection<DeviceModel>(GlobalMonitor.DeviceList);
            //InitDeviceNames();
            SelectedDevice = DeviceList[0];
            

        }

        /// <summary>根据选中设备重建曲线系列（系列直接引用实时 Values，自动刷新）</summary>
        private void RebuildSeries()
        {
            var collection = new SeriesCollection();

            if (SelectedDevice != null)
            {
                int index = 0;
                foreach (var mv in SelectedDevice.MonitorValueList)
                {
                    collection.Add(new LineSeries
                    {
                        Title = string.IsNullOrEmpty(mv.ValueName) ? $"点位 {index + 1}" : mv.ValueName,
                        Values = mv.Values,
                        Stroke = Palette[index % Palette.Length],
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2,
                        PointGeometrySize = 6,
                        LineSmoothness = 0.3
                    });
                    index++;
                }

                StatusMessage = $"当前设备「{SelectedDevice.DeviceName}」共 {index} 个监控点位，实时刷新最近 60 个采样点";
            }
            else
            {
                StatusMessage = "暂无设备数据";
            }

            Series = collection;
        }
    }
}
