using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// CricularProgressBar1.xaml 的交互逻辑
    /// </summary>
    public partial class CricularProgressBar1 : UserControl
    {




        public Double Value
        {
            get { return (Double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Double), typeof(CricularProgressBar1),
                new PropertyMetadata(0.0,new PropertyChangedCallback(OnPropertyChanged)));



        public Brush BackColor
        {
            get { return (Brush)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register("BackColor", typeof(Brush), typeof(CricularProgressBar1), 
                new PropertyMetadata(Brushes.LightGray));


            
        public Brush ForeColor
        {
            get { return (Brush)GetValue(ForeColorProperty); }
            set { SetValue(ForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register("ForeColor", typeof(Brush), typeof(CricularProgressBar1), new PropertyMetadata(Brushes.Orange));





        public CricularProgressBar1()
        {
            InitializeComponent();
            this.SizeChanged += CricularProgressBar1_SizeChanged;
        }

        private void CricularProgressBar1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdataValue();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CricularProgressBar1).UpdataValue();
        }

        private void UpdataValue()
        {
            /*  this.layout.Width = Math.Min(this.RenderSize.Width, this.RenderSize.Height);
              double radius  = Math.Min(this.RenderSize.Width, this.RenderSize.Height)/2;
              if (radius <= 0) return;

                  double newX = 0.0,newY = 0.0;
                  newX = radius + (radius - 3) * Math.Cos((this.Value % 100.0 * 3.6 - 90) * Math.PI / 180);
                  newY = radius + (radius - 3) * Math.Sin((this.Value % 100.0 * 3.6 - 90) * Math.PI / 180);

                  string pathDataStr = "M{0} 3A{1} {1} 0 {4} 1 {2} {3}";
                  pathDataStr  =string.Format(pathDataStr,
                      radius,
                      radius-3,
                      newX,
                      newY,
                      Value % 100 > 50 ? 1:0);
                  var converter = TypeDescriptor.GetConverter(typeof(Geometry));
                  this.path.Data = (Geometry)converter.ConvertFromString(pathDataStr);*/

            // 设置内部容器为正方形
            this.layout.Width = Math.Min(this.RenderSize.Width, this.RenderSize.Height);
            double radius = Math.Min(this.RenderSize.Width, this.RenderSize.Height) / 2;
            if (radius <= 0) return;

            // 获取当前百分比（0~100）
            double percent = this.Value % 100.0;

     

            // 情况2：Value 接近 100（处理浮点误差），画完整圆
            if (percent >= 99.9 || this.Value == 100)
            {
                // 使用 EllipseGeometry，半径与路径中的内半径一致（radius - 3）
                this.path.Data = new EllipseGeometry(new Point(radius, radius), radius - 3, radius - 3);
                return;
            }

            // 情况1：Value 为 0 或空，不显示任何内容（或显示一个点，但通常为空）
            if (percent <= 0)
            {
                this.path.Data = null;
                return;
            }

            // 情况3：正常情况（0 < Value < 100），用路径字符串绘制圆弧
            double newX = 0.0, newY = 0.0;
            // 角度转换：Value%100 * 3.6 将百分比转为角度（0~360），-90 表示从12点钟方向开始
            double angle = (percent * 3.6 - 90) * Math.PI / 180;
            newX = radius + (radius - 3) * Math.Cos(angle);
            newY = radius + (radius - 3) * Math.Sin(angle);

            // 路径字符串：起点为 (radius, 3)（即12点钟方向，内半径 = radius - 3）
            // 格式：M 起点X 起点Y A 半径X 半径Y 旋转角 大弧标志 扫描方向 终点X 终点Y
            string pathDataStr = "M{0} 3A{1} {1} 0 {4} 1 {2} {3}";
            pathDataStr = string.Format(pathDataStr,
                radius,
                radius - 3,
                newX,
                newY,
                percent > 50 ? 1 : 0); // 大弧标志：超过50%时为1
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            this.path.Data = (Geometry)converter.ConvertFromString(pathDataStr);
        }
    }
}
