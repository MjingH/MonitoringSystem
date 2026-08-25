using MonitoringSystem.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MonitoringSystem.View;

namespace MonitoringSystem
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            GlobalMonitor.Start(
                () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //if (new LoginSystem().ShowDialog() == true)
                       // {
                            new MainWindow().ShowDialog();
                        //}
                        Application.Current.Shutdown();
                        
                    });
                },
                (msg) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(msg,"系统启动失败");
                    });
                });
        }
        protected override void OnExit(ExitEventArgs e)
        {
            GlobalMonitor.Dispose();
            base.OnExit(e);
        }
    }
}
