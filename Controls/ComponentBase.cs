using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfControlLibrary1
{
    public class ComponentBase : UserControl
    {

        public ComponentBase()
        {
            this.PreviewMouseLeftButtonDown += CoolingTower_PreviewMouseLeftButtonDown;
        }

        private void CoolingTower_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //   取消权重
            this.IsSelected = !this.IsSelected;


            this.Command?.Execute(this.CommandParameter);
            e.Handled = true;
        }



        // 选中绑定
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;

                // 取消自定义控件多选
                if (value)
                {
                    var parent = VisualTreeHelper.GetParent(this) as Grid;
                    if (parent != null) 
                    {
                        foreach (var child in parent.Children) 
                        { 
                            if(child is ComponentBase)
                                (child as ComponentBase).IsSelected = false;
                        }

                    }
                }
                VisualStateManager.GoToState(this, value ? "SelectState" : "UnselectState", false);

            }
        }


        // 运行绑定
        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRunning.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register("IsRunning", typeof(bool), typeof(ComponentBase), new PropertyMetadata(default(bool),
                new PropertyChangedCallback(OnRunningStateChanged)));


        private static void OnRunningStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool state = (bool)e.NewValue;
            
            VisualStateManager.GoToState(d as ComponentBase, state ? "RunState" : "StopState", false);
        }


        // 失败绑定
        public bool IsFault
        {
            get { return (bool)GetValue(IsFaultProperty); }
            set { SetValue(IsFaultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFault.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFaultProperty =
            DependencyProperty.Register("IsFault", typeof(bool), typeof(ComponentBase), new PropertyMetadata(default(bool), new PropertyChangedCallback(OnFaultStateChanged)));

        private static void OnFaultStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool state = (bool)e.NewValue;
            VisualStateManager.GoToState(d as ComponentBase, state ? "FaultState" : "NormalState", false);
        }



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(default(ICommand)));



        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ComponentBase), new PropertyMetadata(default(object)));


    }
}
