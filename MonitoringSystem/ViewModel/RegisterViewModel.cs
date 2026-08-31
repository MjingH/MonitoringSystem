using MonitoringSystem.Base;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MonitoringSystem.ViewModel
{
    /// <summary>
    /// 注册窗口 ViewModel：校验输入、调用业务层注册新用户
    /// </summary>
    public class RegisterViewModel : NotifyPropertyBase
    {
        public UserModel UserModel { get; set; } = new UserModel();

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { Set(ref _confirmPassword, value); }
        }

        private string _messageError;
        public string MessageError { get => _messageError; set { Set(ref _messageError, value); } }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { Set(ref _isLoading, value); } }

        private bool _isMale = true;
        public bool IsMale
        {
            get => _isMale;
            set
            {
                if (_isMale == value) return;
                _isMale = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsFemale));
            }
        }

        public bool IsFemale
        {
            get => !_isMale;
            set
            {
                if (_isMale == !value) return;
                _isMale = !value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsMale));
            }
        }

        public CommandBase1 RegisterCommand { get; set; }
        public CommandBase1 CloseCommand { get; set; }

        public RegisterViewModel()
        {
            RegisterCommand = new CommandBase1();
            RegisterCommand.DoCanExecute = o => true;
            RegisterCommand.DoExecute = DoRegister;

            CloseCommand = new CommandBase1();
            CloseCommand.DoCanExecute = o => true;
            CloseCommand.DoExecute = o => (o as Window)?.Close();
        }

        private void DoRegister(object o)
        {
            MessageError = string.Empty;

            if (string.IsNullOrWhiteSpace(UserModel.UserName))
            {
                MessageError = "用户名不能为空";
                return;
            }
            if (string.IsNullOrEmpty(UserModel.Password))
            {
                MessageError = "密码不能为空";
                return;
            }
            if (UserModel.Password.Length < 4)
            {
                MessageError = "密码长度不能少于 4 位";
                return;
            }
            if (UserModel.Password != ConfirmPassword)
            {
                MessageError = "两次输入的密码不一致";
                return;
            }

            IsLoading = true;
            var window = o as Window;

            Task.Run(() =>
            {
                try
                {
                    var bll = new MonitorSystemBLL();
                    var result = bll.RegisterUser(UserModel.UserName, UserModel.Password, IsMale);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsLoading = false;
                        if (result.State)
                        {
                            MessageBox.Show("注册成功，请返回登录。", "注册",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            window?.Close();
                        }
                        else
                        {
                            MessageError = result.Message;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsLoading = false;
                        MessageError = ex.Message;
                    });
                }
            });
        }
    }
}
