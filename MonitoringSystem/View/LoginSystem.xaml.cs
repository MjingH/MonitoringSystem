using MonitoringSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfCaptchaDemo;

namespace MonitoringSystem.View
{
    /// <summary>
    /// LoginSystem.xaml 的交互逻辑
    /// </summary>
    public partial class LoginSystem : Window
    {
        public LoginSystem()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();

            this.Closing += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Closing event, Cancel={e.Cancel}");
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

    }
}
