using Communication1;
using MonitoringSystem.Base;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Windows;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 系统操作 ViewModel：设备/用户/存储区管理总览，以及串口参数设置与保存
    /// </summary>
    public class SystemOperationViewModel : NotifyPropertyBase
    {
        // ================= 数据源（直接引用全局静态数据） =================
        public List<DeviceModel> DeviceList => GlobalMonitor.DeviceList ?? new List<DeviceModel>();
        public ObservableCollection<UserModel> UserList => GlobalMonitor.UserList ?? new ObservableCollection<UserModel>();
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
        public CommandBase ToggleUserStatusCommand { get; set; }
        public CommandBase DeleteUserCommand { get; set; }

        public SystemOperationViewModel()
        {
            RefreshCommand = new CommandBase(o => Refresh());
            SaveSerialCommand = new CommandBase(o => SaveSerialSettings());
            ToggleUserStatusCommand = new CommandBase(o => ToggleUserStatus(o));
            DeleteUserCommand = new CommandBase(o => DeleteUser(o));

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
                //config.Save(ConfigurationSaveMode.Modified) 将更改写回磁盘上的.config 文件。Modified 模式仅保存已修改的节，提高效率。
                config.Save(ConfigurationSaveMode.Modified);

                //强制刷新 appSettings 节的缓存
                //此刷新仅影响 ConfigurationManager 的静态缓存，不会影响已打开的 config 对象实例
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

        // ================= 用户管理：启用/禁用、删除 =================

        /// <summary>判断当前登录用户是否为管理员（is_admin == 1）</summary>
        private bool CheckAdmin()
        {
            var user = GlobalMonitor.UserList?.FirstOrDefault(u => u.UserName == CurrentUsername);
            if (user != null && user.IsAdmin == 1)
                return true;

            MessageBox.Show("当前用户不是管理员，无权修改！", "权限不足",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        /// <summary>启用/禁用用户：管理员校验通过后实时写库并刷新界面</summary>
        private void ToggleUserStatus(object o)
        {
            if (!(o is UserModel user)) return;

            if(user.IsAdmin == 1)
            {
                    MessageBox.Show("不能禁用管理员用户！", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

         

            if (!CheckAdmin()) return;

            MessageBoxResult result1 = MessageBox.Show($"确认{(user.Status ? "禁用" : "启用")}用户「{user.UserName}」吗？", "操作确认",
             MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result1 == MessageBoxResult.No) return;

            bool newStatus = !user.Status;
            var bll = new MonitorSystemBLL();
            var result = bll.UpdateUserStatus(user.Id, newStatus);
            if (result.State)
            {
                user.Status = newStatus; // Status 已实现通知，徽标实时刷新
                StatusMessage = $"已{(newStatus ? "启用" : "禁用")}用户 {user.UserName}";
            }
            else
            {
                MessageBox.Show("操作失败：" + result.Message, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>删除用户：管理员校验通过后实时写库并刷新界面</summary>
        private void DeleteUser(object o)
        {
            if (!(o is UserModel user)) return;
            if (!CheckAdmin()) return;

            if (user.UserName == CurrentUsername)
            {
                MessageBox.Show("不能删除当前登录用户！", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(user.IsAdmin == 1)
            {
                MessageBox.Show("不能删除管理员用户！", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var r = MessageBox.Show($"确认删除用户「{user.UserName}」吗？", "删除确认",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;

            var bll = new MonitorSystemBLL();
            var result = bll.DeleteUser(user.Id);
            if (result.State)
            {
                GlobalMonitor.UserList?.Remove(user); // ObservableCollection，界面实时移除该行
                UserCount = UserList.Count;
                StatusMessage = $"已删除用户 {user.UserName}";
            }
            else
            {
                MessageBox.Show("删除失败：" + result.Message, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
