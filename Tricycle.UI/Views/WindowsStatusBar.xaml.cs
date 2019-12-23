using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tricycle.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WindowsStatusBar : ContentView
    {
        const string DEFAULT_STATUS = "Ready";

        public static readonly BindableProperty IsSpinnerVisibleProperty = BindableProperty.Create(
          nameof(IsSpinnerVisible),
          typeof(bool),
          typeof(WindowsStatusBar));
        public static readonly BindableProperty StatusProperty = BindableProperty.Create(
          nameof(Status),
          typeof(string),
          typeof(WindowsStatusBar),
          DEFAULT_STATUS);
        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
          nameof(Progress),
          typeof(double),
          typeof(WindowsStatusBar));

        public string Status
        {
            get => GetValue(StatusProperty)?.ToString();
            set => SetValue(StatusProperty, value);
        }

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public bool IsSpinnerVisible
        {
            get => (bool)GetValue(IsSpinnerVisibleProperty);
            set => SetValue(IsSpinnerVisibleProperty, value);
        }

        public WindowsStatusBar()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(IsSpinnerVisible):
                    actSpinner.IsVisible = IsSpinnerVisible && !barProgress.IsVisible;
                    actSpinner.IsRunning = actSpinner.IsVisible;
                    break;
                case nameof(Status):
                    lblStatus.Text = string.IsNullOrEmpty(Status) ? DEFAULT_STATUS : Status;
                    break;
                case nameof(Progress):
                    barProgress.Progress = Progress;
                    barProgress.IsVisible = Progress > 0;
                    actSpinner.IsVisible = IsSpinnerVisible && !barProgress.IsVisible;
                    break;
            }
        }
    }
}
