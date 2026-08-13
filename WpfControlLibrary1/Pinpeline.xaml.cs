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

namespace WpfControlLibrary1
{
    /// <summary>
    /// Pinpeline.xaml 的交互逻辑
    /// </summary>
    public partial class Pinpeline : UserControl
    {
        public Pinpeline()
        {
            InitializeComponent();
        }


        // 液体流向，1（左向右），0（右向左）
        public int Direction
        {
            get { return (int)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Direction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(int), typeof(Pinpeline), new PropertyMetadata(default(int),new PropertyChangedCallback(OnDirectionChanged)));

        private static void OnDirectionChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            int value = int.Parse(e.NewValue.ToString());
            bool b =  VisualStateManager.GoToState(d as Pinpeline, value == 1 ? "WEFlowState" : "EWFlowState", false);
        }


        // 液体颜色
        public Brush LiquidColor
        {
            get { return (Brush)GetValue(LiquidColorProperty); }
            set { SetValue(LiquidColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LiquidColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LiquidColorProperty =
            DependencyProperty.Register("LiquidColor", typeof(Brush), typeof(Pinpeline), new PropertyMetadata(Brushes.Orange));


        // 管道圆角

        public int CapRadius
        {
            get { return (int)GetValue(CapRadiusProperty); }
            set { SetValue(CapRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CapRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CapRadiusProperty =
            DependencyProperty.Register("CapRadius", typeof(int), typeof(Pinpeline), new PropertyMetadata(0));



    }
}
