using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MonitoringSystem.Base
{
    internal class PasswordhHelper
    {



        public static readonly DependencyProperty PasswordProperty =
             DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordhHelper), new
                 FrameworkPropertyMetadata("", new PropertyChangedCallback(OnPropertyChanged)));



        public static string GetPassword(DependencyObject d)
        {
            return d.GetValue(PasswordProperty).ToString();
        }

        public static void SetPassword(DependencyObject d,string value)
        {
            d.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty AttachProperty =
             DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordhHelper), new
                 FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(OnAttached)));



        public static bool GetAttached(DependencyObject d)
        {
            return (bool)d.GetValue(AttachProperty);
        }

        public static void SetAttached(DependencyObject d,bool value)
        {
            d.SetValue(AttachProperty, value);
        }


        static bool _isUpdating = false;


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox == null) return;

            // 取消事件订阅，避免设置密码时触发循环
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

            // 仅在非内部更新时，将附加属性值同步到 PasswordBox
            if (!_isUpdating)
            {
                passwordBox.Password = e.NewValue?.ToString();
            }

            // 重新订阅事件，确保后续用户输入能继续处理
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        
        }

        private static void OnAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;

            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;

        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            _isUpdating = true;
            SetPassword(passwordBox, passwordBox.Password);
            _isUpdating = false;
        }
    }
}
