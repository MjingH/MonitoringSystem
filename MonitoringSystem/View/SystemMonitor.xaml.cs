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

namespace MonitoringSystem.View
{
    /// <summary>
    /// SystemMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class SystemMonitor : UserControl
    {
        public SystemMonitor()
        {
            InitializeComponent();
        }

        private void CricularProgressBar1_Loaded(object sender, RoutedEventArgs e)
        {

        }

        // 缩放仿真平台
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double newWidth = this.mainView.ActualWidth + e.Delta;
            double newHeight = this.mainView.ActualHeight + e.Delta;

            if(newWidth < 500) newHeight = 500;
            if(newHeight < 100) newHeight = 100;
            this.mainView.Width = newWidth;
            this.mainView.Height = newHeight;

            // 中间区域缩放
            this.mainView.SetValue(Canvas.LeftProperty, (this.RenderSize.Width - this.mainView.Width) / 2);
            
        }

        bool _isMoving = false;
        Point _downPoint = new Point(0,0);
        double left = 0,top = 0;

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _isMoving = true;
            _downPoint = e.GetPosition(sender as Canvas);

            left = double.Parse(this.mainView.GetValue(Canvas.LeftProperty).ToString());
            top = double.Parse(this.mainView.GetValue(Canvas.TopProperty).ToString());
            (sender as Canvas).CaptureMouse(); 
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _isMoving = false;

            (sender as Canvas).ReleaseMouseCapture();
        }

        private void CoolingPump_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Pinpeline_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CoolingPump_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void Pinpeline_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 必须同时满足“按下状态”与“左键仍处于按下”，
            // 防止 MouseLeftButtonUp 丢失后 _isMoving 卡在 true，导致松开鼠标画布仍跟着移动
            if (_isMoving && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(sender as Canvas);

                this.mainView.SetValue(Canvas.LeftProperty,left +  currentPoint.X-_downPoint.X);
                this.mainView.SetValue(Canvas.TopProperty,top +  currentPoint.Y-_downPoint.Y);

                e.Handled = true;

            }
        }
        
    }
}
