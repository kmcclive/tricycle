using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class MacTitleBar : ContentView
    {
        public static readonly BindableProperty StatusProperty = BindableProperty.Create(
          nameof(Status),
          typeof(string),
          typeof(MacTitleBar));
        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
          nameof(Progress),
          typeof(double),
          typeof(MacTitleBar));
        public static readonly BindableProperty IsBackVisibleProperty = BindableProperty.Create(
          nameof(IsBackVisible),
          typeof(bool),
          typeof(MacTitleBar),
          true);
        public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(
          nameof(BackCommand),
          typeof(ICommand),
          typeof(MacTitleBar));
        public static readonly BindableProperty IsPreviewVisibleProperty = BindableProperty.Create(
          nameof(IsPreviewVisible),
          typeof(bool),
          typeof(MacTitleBar),
          true);
        public static readonly BindableProperty PreviewCommandProperty = BindableProperty.Create(
          nameof(PreviewCommand),
          typeof(ICommand),
          typeof(MacTitleBar));

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

        public bool IsBackVisible
        {
            get => (bool)GetValue(IsBackVisibleProperty); 
            set => SetValue(IsBackVisibleProperty, value);
        }

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        public bool IsPreviewVisible
        {
            get => (bool)GetValue(IsPreviewVisibleProperty);
            set => SetValue(IsPreviewVisibleProperty, value);
        }

        public ICommand PreviewCommand
        {
            get => (ICommand)GetValue(PreviewCommandProperty);
            set => SetValue(PreviewCommandProperty, value);
        }

        public MacTitleBar()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Status):
                    lblStatus.Text = Status;
                    lblStatus.IsVisible = !string.IsNullOrWhiteSpace(Status);
                    imgIcon.IsVisible = !lblStatus.IsVisible;
                    break;
                case nameof(Progress):
                    barProgress.Progress = Progress;
                    break;
                case nameof(IsBackVisible):
                    btnBack.IsVisible = IsBackVisible;
                    break;
                case nameof(BackCommand):
                    btnBack.Command = BackCommand;
                    break;
                case nameof(IsPreviewVisible):
                    btnPreview.IsVisible = IsPreviewVisible;
                    break;
                case nameof(PreviewCommand):
                    btnPreview.Command = PreviewCommand;
                    break;
            }
        }
    }
}
