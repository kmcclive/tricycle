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
        public static readonly BindableProperty IsBackVisibleProperty = BindableProperty.Create(
          nameof(IsBackVisible),
          typeof(bool),
          typeof(MacTitleBar));
        public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(
          nameof(BackCommand),
          typeof(ICommand),
          typeof(MacTitleBar));
        public static readonly BindableProperty PreviewCommandProperty = BindableProperty.Create(
          nameof(PreviewCommand),
          typeof(ICommand),
          typeof(MacTitleBar));

        public string Status
        {
            get { return GetValue(StatusProperty)?.ToString(); }
            set { SetValue(StatusProperty, value); }
        }

        public bool IsBackVisible
        {
            get { return (bool)GetValue(IsBackVisibleProperty); }
            set { SetValue(IsBackVisibleProperty, value); }
        }

        public ICommand BackCommand
        {
            get { return (ICommand)GetValue(BackCommandProperty); }
            set { SetValue(BackCommandProperty, value); }
        }

        public ICommand PreviewCommand
        {
            get { return (ICommand)GetValue(PreviewCommandProperty); }
            set { SetValue(PreviewCommandProperty, value); }
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
                case nameof(BackCommand):
                    btnBack.Command = BackCommand;
                    break;
                case nameof(IsBackVisible):
                    btnBack.IsVisible = IsBackVisible;
                    break;
            }
        }
    }
}
