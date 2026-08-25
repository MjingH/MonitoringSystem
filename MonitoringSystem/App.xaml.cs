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
                    // 使用 BeginInvoke 异步执行 UI 操作，不阻塞后台线程
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 如果存在登录窗口，可在此显示
                        // if (new LoginSystem().ShowDialog() == true)
                        // {
                        new MainWindow().ShowDialog();
                        // }
                        Application.Current.Shutdown();
                    }));
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
