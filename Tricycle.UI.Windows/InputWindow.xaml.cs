using System;
using System.Media;
using System.Windows;

namespace Tricycle.UI.Windows
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        bool _isValueRequired;

        public InputWindow()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => lblMessage.Content?.ToString();
            set => lblMessage.Content = value;
        }

        public string Value
        {
            get => txtValue.Text;
            set => txtValue.Text = value;
        }

        public bool IsValueRequired
        {
            get => _isValueRequired;
            set
            {
                _isValueRequired = value;

                btnOK.IsEnabled = !_isValueRequired || !string.IsNullOrWhiteSpace(txtValue.Text);
            }
        }

        public static string Show(Window owner, string title, string message, string defaultValue, bool isValueRequired)
        {
            var window = new InputWindow()
            {
                Owner = owner,
                Title = title,
                Message = message,
                Value = defaultValue,
                IsValueRequired = isValueRequired
            };

            if (!isValueRequired || !string.IsNullOrWhiteSpace(defaultValue))
            {
                window.btnOK.Focus();
            }
            else
            {
                window.btnCancel.Focus();
            }

            SystemSounds.Beep.Play();

            return window.ShowDialog() == true ? window.Value : null;
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

        private void txtValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isValueRequired)
            {
                btnOK.IsEnabled = !string.IsNullOrWhiteSpace(txtValue.Text);
            }
        }
    }
}
