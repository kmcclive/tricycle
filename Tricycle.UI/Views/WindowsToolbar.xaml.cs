using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tricycle.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WindowsToolbar : ContentView
    {
        public static readonly BindableProperty IsPreviewVisibleProperty = BindableProperty.Create(
          nameof(IsPreviewVisible),
          typeof(bool),
          typeof(WindowsToolbar),
          true);
        public static readonly BindableProperty PreviewCommandProperty = BindableProperty.Create(
          nameof(PreviewCommand),
          typeof(ICommand),
          typeof(WindowsToolbar));
        public static readonly BindableProperty IsStartVisibleProperty = BindableProperty.Create(
          nameof(IsStartVisible),
          typeof(bool),
          typeof(WindowsToolbar),
          true);
        public static readonly BindableProperty StartCommandProperty = BindableProperty.Create(
          nameof(StartCommand),
          typeof(ICommand),
          typeof(WindowsToolbar));
        public static readonly BindableProperty StartImageSourceProperty = BindableProperty.Create(
          nameof(StartImageSource),
          typeof(ImageSource),
          typeof(WindowsToolbar),
          ImageSource.FromFile("Images/start.png"));

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

        public bool IsStartVisible
        {
            get => (bool)GetValue(IsStartVisibleProperty);
            set => SetValue(IsStartVisibleProperty, value);
        }

        public ICommand StartCommand
        {
            get => (ICommand)GetValue(StartCommandProperty);
            set => SetValue(StartCommandProperty, value);
        }

        public ImageSource StartImageSource
        {
            get => (ImageSource)GetValue(StartImageSourceProperty);
            set => SetValue(StartImageSourceProperty, value);
        }

        public WindowsToolbar()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(IsPreviewVisible):
                    btnPreview.IsVisible = IsPreviewVisible;
                    break;
                case nameof(PreviewCommand):
                    btnPreview.Command = PreviewCommand;
                    break;
                case nameof(IsStartVisible):
                    btnStart.IsVisible = IsStartVisible;
                    break;
                case nameof(StartCommand):
                    btnStart.Command = StartCommand;
                    break;
                case nameof(StartImageSource):
                    btnStart.Source = StartImageSource;
                    break;
            }
        }
    }
}
