using MonitoringSystem.Base;
using MonitoringSystem.Model;
using MonitoringSystem.View;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MonitoringSystem.ViewModel
{
    public class MainViewModel : NotifyPropertyBase
    {

        public LoginViewModel LoginViewModel { get; set; }

     


        private UIElement _mainContent;

		public UIElement MainContent
		{
			get { return _mainContent; }
			set {
				Set(ref _mainContent, value);
			}
        }


        private string _currentUsername;
        public string CurrentUsername
        {
            get => _currentUsername;
            set { _currentUsername = value; RaisePropertyChanged(); }
        }


        public CommandBase TabChangedCommand {  get; set; }

        public CommandBase LogoutCommand { get; set; }

        public CommandBase ProfileCommand { get; set; }

        public CommandBase SettingsCommand { get; set; }

        public MainViewModel()
        {
            CurrentUsername = GlobalMonitor.CurrentUsername ?? "未登录";

            //UserModel = new UserModel();

            //LoginViewModel = new LoginViewModel();

            TabChangedCommand = new CommandBase(OnTabChaged);
            LogoutCommand = new CommandBase(DoLogout);
            ProfileCommand = new CommandBase(DoProfile);
            SettingsCommand = new CommandBase(DoSettings);
            OnTabChaged("MonitoringSystem.View.SystemMonitor");
            // 初始化默认显示内容（与第一个 RadioButton 的 CommandParameter 一致）
            //if (TabChangedCommand.CanExecute("MonitoringSystem.View.SystemMonitor"))
            // TabChangedCommand.Execute("MonitoringSystem.View.SystemMonitor");   
        }

        private void OnTabChaged(object o)
        {
            /* System.Diagnostics.Debug.WriteLine("命令执行了！参数：" + o);
                // 完整方法
                if (o == null) return;
                string[] strValues = o.ToString().Split('|');
                Assembly assembly = Assembly.LoadFrom(strValues[0]);
                Type type = assembly.GetType(strValues[1]);
                var instance = Activator.CreateInstance(type);
                System.Diagnostics.Debug.WriteLine("创建的对象类型：" + instance.GetType().FullName);
                this.MainContent = (UIElement)instance;*/
            //this.MainContent = (UIElement)Activator.CreateInstance(type);

            // 简化方法，必须在同一个程序集
            if (o == null) return;
            try
            {
                Type type = Type.GetType(o.ToString());
                if (type == null) return;
                this.MainContent = (UIElement)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建异常: {ex}");
            }
        }

        /// <summary>
        /// 退出登录：弹出登录窗口，重新登录成功后更新当前用户
        /// </summary>
        private void DoLogout(object o)
        {
            (o as Window).Close();
            var loginWindow = new LoginSystem();
            if (loginWindow.ShowDialog() == true)
            {
                CurrentUsername = GlobalMonitor.CurrentUsername;
                // 重新登录后切换回系统监控首页
                OnTabChaged("MonitoringSystem.View.SystemMonitor");
            }
        }

        /// <summary>
        /// 个人中心：展示当前登录用户信息
        /// </summary>
        private void DoProfile(object o)
        {
            var user = GlobalMonitor.UserList?.FirstOrDefault(u => u.UserName == CurrentUsername);
            string info;
            if (user != null)
            {
                info = $"用户名：{user.UserName}\n" +
                       $"姓名：{user.Name}\n" +
                       $"性别：{(user.Sex ? "男" : "女")}\n" +
                       $"创建时间：{user.CreateTime}";
            }
            else
            {
                info = $"当前登录用户：{CurrentUsername}";
            }

            MessageBox.Show(info, "个人中心", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 设置：占位功能
        /// </summary>
        private void DoSettings(object o)
        {
            MessageBox.Show("设置功能开发中，敬请期待。", "设置", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
