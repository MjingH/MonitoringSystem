using Communication1;
using MonitoringSystem.Base;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 系统操作 ViewModel：设备/用户/存储区管理总览，以及串口参数设置与保存
    /// </summary>
    public class SystemOperationViewModel : NotifyPropertyBase
    {
        // ================= 数据源（直接引用全局静态数据） =================
        public List<DeviceModel> DeviceList => GlobalMonitor.DeviceList ?? new List<DeviceModel>();
        public List<UserModel> UserList => GlobalMonitor.UserList ?? new List<UserModel>();
        public List<StorageModel> StorageList => GlobalMonitor.StorageList ?? new List<StorageModel>();

        private DeviceModel _selectedDevice;
        public DeviceModel SelectedDevice
        {
            get => _selectedDevice;
            set { Set(ref _selectedDevice, value); }
        }

        // ================= 统计 =================
        private int _deviceCount;
        public int DeviceCount { get => _deviceCount; set { Set(ref _deviceCount, value); } }

        private int _userCount;
        public int UserCount { get => _userCount; set { Set(ref _userCount, value); } }

        private int _storageCount;
        public int StorageCount { get => _storageCount; set { Set(ref _storageCount, value); } }

        private string _currentUsername;
        public string CurrentUsername { get => _currentUsername; set { Set(ref _currentUsername, value); } }

        // ================= 串口设置（可编辑） =================
        private string _portName;
        public string PortName { get => _portName; set { Set(ref _portName, value); } }

        private int _baudRate;
        public int BaudRate { get => _baudRate; set { Set(ref _baudRate, value); } }

        private int _dataBit;
        public int DataBit { get => _dataBit; set { Set(ref _dataBit, value); } }

        private string _parity;
        public string Parity { get => _parity; set { Set(ref _parity, value); } }

        private string _stopBits;
        public string StopBits { get => _stopBits; set { Set(ref _stopBits, value); } }

        public List<int> BaudRateOptions { get; } = new List<int> { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        public List<int> DataBitOptions { get; } = new List<int> { 5, 6, 7, 8 };
        public List<string> ParityOptions { get; } = new List<string> { "None", "Odd", "Even", "Mark", "Space" };
        public List<string> StopBitsOptions { get; } = new List<string> { "None", "One", "Two", "OnePointFive" };

        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set { Set(ref _statusMessage, value); } }

        // ================= 命令 =================
        public CommandBase RefreshCommand { get; set; }
        public CommandBase SaveSerialCommand { get; set; }

        public SystemOperationViewModel()
        {
            RefreshCommand = new CommandBase(o => Refresh());
            SaveSerialCommand = new CommandBase(o => SaveSerialSettings());

            Refresh();
        }

        /// <summary>从全局静态数据刷新统计、串口参数，并保证有默认选中设备</summary>
        public void Refresh()
        {
            DeviceCount = DeviceList.Count;
            UserCount = UserList.Count;
            StorageCount = StorageList.Count;
            CurrentUsername = GlobalMonitor.CurrentUsername ?? "未登录";

            LoadSerialSettings();

            if (SelectedDevice == null && DeviceList.Count > 0)
                SelectedDevice = DeviceList.First();

            StatusMessage = string.Empty;
        }

        /// <summary>优先从内存中的串口信息加载，否则回退到配置文件</summary>
        private void LoadSerialSettings()
        {
            var si = GlobalMonitor.SerialInfo;
            if (si != null)
            {
                PortName = si.PortName;
                BaudRate = si.BaudRate;
                DataBit = si.DataBit;
                Parity = si.Parity.ToString();
                StopBits = si.StopBits.ToString();
                return;
            }

            // 回退：直接读取 App.config
            PortName = ConfigurationManager.AppSettings["port"] ?? "COM1";
            BaudRate = int.TryParse(ConfigurationManager.AppSettings["baud"], out var b) ? b : 9600;
            DataBit = int.TryParse(ConfigurationManager.AppSettings["data_bit"], out var d) ? d : 8;
            Parity = ConfigurationManager.AppSettings["parity"] ?? "None";
            StopBits = ConfigurationManager.AppSettings["stopbit"] ?? "One";
        }

        /// <summary>保存串口参数到 App.config，并同步更新内存中的 SerialInfo</summary>
        private void SaveSerialSettings()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                SetOrAdd(config.AppSettings.Settings, "port", PortName);
                SetOrAdd(config.AppSettings.Settings, "baud", BaudRate.ToString());
                SetOrAdd(config.AppSettings.Settings, "data_bit", DataBit.ToString());
                SetOrAdd(config.AppSettings.Settings, "parity", Parity);
                SetOrAdd(config.AppSettings.Settings, "stopbit", StopBits);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                // 同步内存中的串口信息（重启程序后读取配置生效）
                if (GlobalMonitor.SerialInfo != null)
                {
                    GlobalMonitor.SerialInfo.PortName = PortName;
                    GlobalMonitor.SerialInfo.BaudRate = BaudRate;
                    GlobalMonitor.SerialInfo.DataBit = DataBit;
                    GlobalMonitor.SerialInfo.Parity = (Parity)Enum.Parse(typeof(Parity), Parity);
                    GlobalMonitor.SerialInfo.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits);
                }

                StatusMessage = "串口设置已保存，重启程序后生效";
            }
            catch (Exception ex)
            {
                StatusMessage = "保存失败：" + ex.Message;
            }
        }

        private void SetOrAdd(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] != null)
                settings[key].Value = value;
            else
                settings.Add(key, value);
        }
    }
}
