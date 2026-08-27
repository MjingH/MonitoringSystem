using MonitoringSystem.Base;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
using MonitoringSystem.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MonitoringSystem.ViewModel
{
    public class LoginViewModel : NotifyPropertyBase
    {


        public DataResult<List<UserModel>> user { get; set; }

  

        public CommandBase1 CloseWindowCommand { get; set; }

        private string _messageError;

        public string MessageError
        {
            get { return _messageError; }
            set { _messageError = value; RaisePropertyChanged(); }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value;RaisePropertyChanged(); }
        }



        public CommandBase1 LoginCommand { get; set; }

        public UserModel UserModel { get; set; } = new UserModel();

        public MonitorSystemBLL monitorSystemBLL { get; set; }

        private string _currtentUsername;

      



        #region 验证码实现逻辑

        // 用于生成随机验证码的字符集（数字 + 大写字母，排除易混淆字符）
        private const string CharSet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        // 验证码长度
        private const int CaptchaLength = 6;

        // 当前验证码文本（用于比对）
        private string _currentCaptchaText;


        private string _userInput;
        /// <summary>
        /// 用户输入的验证码文本（双向绑定到 TextBox）
        /// </summary>
        public string UserInput
        {
            get => _userInput;
            set
            {
                if (_userInput != value)
                {
                    _userInput = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 验证码图片源（绑定到 Image 控件）
        private BitmapImage _captchaImageSource;
        public BitmapImage CaptchaImageSource
        {
            get => _captchaImageSource;
            private set
            {
                _captchaImageSource = value;
                RaisePropertyChanged();
            }
        }


        // 是否验证通过（可用于控制其他 UI 状态）
        private bool _isValid;

  

        /// <summary>
        /// 刷新验证码命令（通常绑定到“换一张”按钮）
        /// </summary>
        public ICommand RefreshCaptchaCommand { get; }

        /// <summary>
        /// 验证输入命令（绑定到“验证”按钮）
        /// </summary>
        public ICommand ValidateCommand { get; }



        /// <summary>
        /// 当前验证码是否通过（可用于控制其他控件的启用状态）
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            private set
            {
                _isValid = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 生成随机验证码字符串（仅包含 CharSet 中的字符）
        /// </summary>
        private string GenerateRandomText()
        {
            var rand = new Random();
            char[] chars = new char[4];
            for (int i = 0; i < 4 ; i++)
            {
                chars[i] = CharSet[rand.Next(CharSet.Length)];
            }
            return new string(chars);
        }

        /// <summary>
        /// 将验证码文本绘制为 BitmapImage，并添加干扰点、线条等增强安全性
        /// </summary>
        private BitmapImage GenerateCaptchaImage(string captchaText)
        {
            // 定义图片尺寸
            int width = 90;
            int height = 30;

            // 使用 System.Drawing.Bitmap 绘制（需要引用 System.Drawing.Common）
            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // 设置背景色为白色
                g.Clear(System.Drawing.Color.White);

                // 随机数生成器
                Random rand = new Random();

                // 绘制干扰线（3条随机颜色的斜线）
                for (int i = 0; i < 3; i++)
                {
                    int x1 = rand.Next(width);
                    int y1 = rand.Next(height);
                    int x2 = rand.Next(width);
                    int y2 = rand.Next(height);
                    using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(rand.Next(100, 200), rand.Next(256), rand.Next(256), rand.Next(256)), 1.5f))
                    {
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }

                // 绘制每个字符（不同大小、颜色、旋转角度）
                using (Font font = new Font("Arial", 14, System.Drawing.FontStyle.Bold))
                {
                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        // 随机颜色（深色系）
                        System.Drawing.Color color = Color.FromArgb(rand.Next(50, 200), rand.Next(50, 200), rand.Next(50, 200));
                        using (SolidBrush brush = new SolidBrush(color))
                        {
                            // 1. 横向定位：起始X设为10，每个字符步进12（字体12号，每个字符约占10px，间距2px）
                            float x = 10 + i * 18 + rand.Next(-2, 2);

                            // 2. 纵向定位：垂直居中，y从12到18（字体高约12~14，图片高40，居中区域在12~18）
                            float y = rand.Next(3, 9);    // 将范围整体减小 5

                            // 随机旋转角度（-20° ~ 20°）
                            float angle = rand.Next(-15, 15);
                            using (Matrix matrix = new Matrix())
                            {
                                // 3. 旋转中心点需要随字符位置调整（字符中心大致在 (x+6, y+10) 附近，可根据实际微调）
                                matrix.RotateAt(angle, new PointF(x + 6, y + 4));
                                g.Transform = matrix;
                                g.DrawString(captchaText[i].ToString(), font, brush, x, y);
                                g.ResetTransform();
                            }
                        }
                    }
                }

                // 添加随机干扰点（100个噪点）
                for (int i = 0; i < 100; i++)
                {
                    int x = rand.Next(width);
                    int y = rand.Next(height);
                    bitmap.SetPixel(x, y, Color.FromArgb(rand.Next(100, 200), rand.Next(256), rand.Next(256), rand.Next(256)));
                }

                // 将 Bitmap 转换为 BitmapImage（WPF 可用的图片源）
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad; // 确保流可被及时释放
                    image.EndInit();
                    image.Freeze(); // 冻结以便跨线程使用（可选）
                    return image;
                }
            }
        }

        /// <summary>
        /// 刷新验证码：生成新文本并更新图片
        /// </summary>
        private void RefreshCaptcha()
        {
            // 生成随机验证码文本
            _currentCaptchaText = GenerateRandomText();

            // 生成对应的图片
            CaptchaImageSource = GenerateCaptchaImage(_currentCaptchaText);

            // 清空用户输入和之前的验证状态
            UserInput = string.Empty;
            this.MessageError = string.Empty;
            IsValid = false;
        }


        #endregion



        public LoginViewModel() {



            // 初始化命令
            RefreshCaptchaCommand = new CommandBase((o) => RefreshCaptcha());
           // ValidateCommand = new CommandBase(_ => Validate());

            // 生成初始验证码
            RefreshCaptcha();

            this.CloseWindowCommand = new CommandBase1();
            this.CloseWindowCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });
            this.CloseWindowCommand.DoExecute = new Action<object>((o) =>
            {
                (o as Window).Close();
            });
            
            this.LoginCommand = new CommandBase1();

            LoginCommand.DoCanExecute = new Func<object,bool>(o => { return true; });
            LoginCommand.DoExecute = new Action<object>(DoLogin);
        }

        private void DoLogin(object o)
        {
            
            this.IsLoading = true;
            this.MessageError = "";
            if (string.IsNullOrEmpty(UserModel.UserName))
            {
                this.IsLoading = false;
                this.MessageError = "用户名不能为空";
                return;
            }

            if (string.IsNullOrEmpty(UserModel.Password)) 
            {
                this.IsLoading = false;
                this.MessageError = "密码不能为空";
                return;
            }


            if (string.IsNullOrWhiteSpace(UserInput))
            {
                this.MessageError = "请输入验证码";
                IsValid = false;
                this.IsLoading = false;
                return;
            }

            // 比较时不区分大小写
            if (UserInput.Equals(_currentCaptchaText, StringComparison.OrdinalIgnoreCase))
            {
               
                IsValid = true;
            }
            else
            {
                this.MessageError = "验证码错误，请重试！";
                this.IsLoading = false;
                IsValid = false;
                return;
                // 可自动刷新验证码（安全考虑）
                // RefreshCaptcha(); // 如果需要，可取消注释
            }

        

            Task.Run(new Action(() => 
             {

                 try
                 {
                     
                     monitorSystemBLL = new MonitorSystemBLL();
                      user = monitorSystemBLL.LoginUser(UserModel.UserName, UserModel.Password);
                     
                     Console.WriteLine(user.Data[0].UserName);
                     if (!user.State)
                     {
                         MessageError = user.Message;
                         this.IsLoading = false;
                         return;
                     }
                     
                     else if (user.State)
                     {
                         Application.Current.Dispatcher.Invoke(new Action(() =>
                         {
                             GlobalMonitor.CurrentUsername = UserModel.UserName; // 保存到全局
                             (o as LoginSystem).DialogResult = true;
                         }));
                     }
                 }
                 catch (Exception ex)
                 {

                     this.MessageError = ex.Message;
                 }
                 
             }));

        }
    }
}
