using System;
using System.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Tricycle.UI.Windows
{
    public enum MessageWindowButtons
    {
        Ok,
        OkCancel
    }

    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        Severity _severity = Severity.Info;
        MessageWindowButtons _buttons = MessageWindowButtons.Ok;

        public MessageWindow()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => txtMessage.Text?.ToString();
            set => txtMessage.Text = value;
        }

        public Severity Severity
        {
            get => _severity;
            set
            {
                _severity = value;

                string filename;

                switch (value)
                {
                    case Severity.Info:
                    default:
                        filename =  @"Assets\Images\info.png";
                        break;
                    case Severity.Warning:
                        filename = @"Assets\Images\warning.png";
                        break;
                    case Severity.Error:
                        filename = @"Assets\Images\error.png";
                        break;
                }

                imgSeverity.Source = new BitmapImage(new Uri(filename, UriKind.Relative));
            }
        }

        public MessageWindowButtons Buttons
        {
            get => _buttons;
            set
            {
                _buttons = value;

                btnCancel.Visibility = value == MessageWindowButtons.OkCancel ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static bool? Show(Window owner, string title, string message, Severity severity, MessageWindowButtons buttons)
        {
            var window = new MessageWindow()
            {
                Owner = owner,
                Title = title,
                Message = message,
                Severity = severity,
                Buttons = buttons
            };

            if (buttons == MessageWindowButtons.OkCancel)
            {
                window.btnCancel.Focus();
            }
            else
            {
                window.btnOK.Focus();
            }

            SystemSound sound;

            switch (severity)
            {
                case Severity.Info:
                default:
                    sound = SystemSounds.Beep;
                    break;
                case Severity.Warning:
                    sound = SystemSounds.Exclamation;
                    break;
                case Severity.Error:
                    sound = SystemSounds.Hand;
                    break;
            }

            sound.Play();

            return window.ShowDialog();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IconHelper.RemoveIcon(this);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
