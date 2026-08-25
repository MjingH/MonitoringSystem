using MonitoringSystem.Base;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using System.Windows.Media.Imaging;

namespace WpfCaptchaDemo
{
    /// <summary>
    /// 验证码功能的 ViewModel，负责生成验证码图片、存储当前验证码文本、验证用户输入。
    /// </summary>
    public class CaptchaViewModel : NotifyPropertyBase
    {
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

        // 验证结果消息（可选，用于显示提示）
        private string _validationMessage;

        // 是否验证通过（可用于控制其他 UI 状态）
        private bool _isValid;

        public CaptchaViewModel()
        {
            // 初始化命令
            RefreshCaptchaCommand = new CommandBase((o)=> RefreshCaptcha());
            ValidateCommand = new CommandBase(_ => Validate());

            // 生成初始验证码
            RefreshCaptcha();
        }

        /// <summary>
        /// 刷新验证码命令（通常绑定到“换一张”按钮）
        /// </summary>
        public ICommand RefreshCaptchaCommand { get; }

        /// <summary>
        /// 验证输入命令（绑定到“验证”按钮）
        /// </summary>
        public ICommand ValidateCommand { get; }

      

     

        /// <summary>
        /// 验证结果消息（可用于显示“正确”或“错误”）
        /// </summary>
        public string ValidationMessage
        {
            get => _validationMessage;
            private set
            {
                _validationMessage = value;
                RaisePropertyChanged();
            }
        }

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
            char[] chars = new char[CaptchaLength];
            for (int i = 0; i < CaptchaLength; i++)
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
            int height = 40;

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
                using (Font font = new Font("Arial", 20, System.Drawing.FontStyle.Bold))
                {
                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        // 随机颜色（深色系）
                        System.Drawing.Color color = Color.FromArgb(rand.Next(50, 200), rand.Next(50, 200), rand.Next(50, 200));
                        using (SolidBrush brush = new SolidBrush(color))
                        {
                            // 字符位置（水平间距和垂直偏移）
                            float x = 20 + i * 25 + rand.Next(-5, 5);
                            float y = rand.Next(10, 20);

                            // 随机旋转角度（-20° ~ 20°）
                            float angle = rand.Next(-20, 20);
                            using (Matrix matrix = new Matrix())
                            {
                                matrix.RotateAt(angle, new PointF(x + 10, y + 15));
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
            ValidationMessage = string.Empty;
            IsValid = false;
        }

        /// <summary>
        /// 验证用户输入是否与当前验证码匹配（忽略大小写）
        /// </summary>
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
            {
                ValidationMessage = "请输入验证码！";
                IsValid = false;
                return;
            }

            // 比较时不区分大小写
            if (UserInput.Equals(_currentCaptchaText, StringComparison.OrdinalIgnoreCase))
            {
                ValidationMessage = "验证通过！";
                IsValid = true;
            }
            else
            {
                ValidationMessage = "验证码错误，请重试！";
                IsValid = false;
                // 可自动刷新验证码（安全考虑）
                // RefreshCaptcha(); // 如果需要，可取消注释
            }
        }

        /// <summary>
        /// 判断是否可以执行验证（输入不为空时启用验证按钮）
        /// </summary>
        private bool CanValidate()
        {
            return !string.IsNullOrWhiteSpace(UserInput);
        }

    }

}