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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonitoringSystem
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
 
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
           if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                // 双击标题栏最大化
                Max_Click(sender, e);
            }
            else
            {
                // 拖拽移动窗口
                this.DragMove();
            }
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
             
            }
            else
            {
                // 最大化窗口
                this.WindowState = WindowState.Maximized;

               
            }
        }

        private void Close_Clice(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 切换弹出状态
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
            e.Handled = true; // 避免事件继续冒泡
        }
    }
}
