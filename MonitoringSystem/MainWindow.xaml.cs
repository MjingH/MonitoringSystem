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
            // 窗口拖动统一由标题栏的 Grid_MouseLeftButtonDown 处理，
            // 此处不再调用 DragMove，避免拦截子页面（如 SystemMonitor 画布）的鼠标事件，
            // 导致 Canvas 的 MouseLeftButtonUp 丢失、拖拽状态卡住。
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 只处理鼠标左键
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (e.ClickCount == 2)
            {
                // 双击标题栏最大化
                Max_Click(sender, e);
            }
            else
            {
                // DragMove 只能在主鼠标按钮处于按下状态时调用，
                // 快速连点/事件冒泡时序下左键可能已释放，需先校验再调用
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                try
                {
                    // 拖拽移动窗口
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // 左键状态异常时忽略，避免拖拽抛异常
                }
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
            App.Current.Shutdown();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 切换弹出状态
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
            e.Handled = true; // 避免事件继续冒泡
        }

        /// <summary>
        /// 点击窗口其他位置时关闭下拉菜单（点击菜单内部不会触发，因为 Popup 是独立窗口）
        /// </summary>
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!UserMenuPopup.IsOpen) return;

            var source = e.OriginalSource as DependencyObject;
            if (source != null && IsDescendantOf(source, UserInfoPanel)) return;

            UserMenuPopup.IsOpen = false;
        }

        /// <summary>
        /// 点击下拉菜单中的按钮后关闭菜单
        /// </summary>
        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
        }

        private bool IsDescendantOf(DependencyObject child, DependencyObject parent)
        {
            DependencyObject current = child;
            while (current != null)
            {
                if (ReferenceEquals(current, parent)) return true;
                current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
            }
            return false;
        }
    }
}
