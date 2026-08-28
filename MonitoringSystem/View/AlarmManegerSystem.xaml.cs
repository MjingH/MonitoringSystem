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
    /// AlarmManegerSystem.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmManegerSystem : UserControl
    {
        private AlarmManegerViewModel _viewModel;

        public AlarmManegerSystem()
        {
            InitializeComponent();
            _viewModel = new AlarmManegerViewModel();
            this.DataContext = _viewModel;
        }

        /// <summary>
        /// 控件从可视化树卸载时释放 ViewModel（取消静态事件订阅，避免内存泄漏）
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.Dispose();
            _viewModel = null;
        }
    }
}
