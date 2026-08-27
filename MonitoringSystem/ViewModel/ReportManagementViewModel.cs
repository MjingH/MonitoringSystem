using MonitoringSystem.Base;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 报表管理页面的 ViewModel。
    ///
    /// 为什么新增这个类：
    ///   在“改造1”之前，报表管理页是通过反射（Type.GetType + Activator.CreateInstance）
    ///   直接创建 View（UserControl）来显示的，因此当时并不需要一个专门的 ViewModel。
    ///
    ///   改造后，导航方式改为“切换 ViewModel”，再由 MainWindow.xaml 中的 DataTemplate
    ///   根据 ViewModel 的具体类型自动选择对应的 View（ReportManagement.xaml）来渲染。
    ///   所以必须为“报表管理页”提供一个 ViewModel 类型，作为 DataTemplate 的匹配键（DataType）。
    ///
    ///   换句话说：ViewModel 只负责“是什么页面”，View 由 DataTemplate 决定“长什么样”。
    /// </summary>
    public class ReportManagementViewModel : NotifyPropertyBase
    {
        // 目前报表管理页面（ReportManagement.xaml）还是占位页面，没有交互数据，
        // 因此这里暂不添加任何属性或命令；后续需要报表数据时，再在此类中扩展
        // 可绑定的属性（如 ObservableCollection<LogModel> 等）和命令即可。
    }
}
