using MonitoringSystem.Base;
using MonitoringSystem.Model;
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

        public UserModel  UserModel { get; set; }

        public LoginViewModel LoginViewModel { get; set; }


        private UIElement _mainContent;

		public UIElement MainContent
		{
			get { return _mainContent; }
			set {
				Set(ref _mainContent, value);
			}
        }
		

		public CommandBase TabChangedCommand {  get; set; }

        public MainViewModel()
        {
            //UserModel = new UserModel();

            LoginViewModel = new LoginViewModel();

            TabChangedCommand = new CommandBase(OnTabChaged);
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


    }
}
