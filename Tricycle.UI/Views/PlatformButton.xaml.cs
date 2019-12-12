using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tricycle.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlatformButton : ContentView
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
          nameof(Command),
          typeof(ICommand),
          typeof(PlatformButton));
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
          nameof(Source),
          typeof(ImageSource),
          typeof(PlatformButton));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public event EventHandler Clicked
        {
            add
            {
                switch(Device.RuntimePlatform)
                {
                    case Device.macOS:
                        macButton.Clicked += value;
                        break;
                    case Device.WPF:
                        wpfButton.Clicked += value;
                        break;
                }
            }

            remove
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.macOS:
                        macButton.Clicked -= value;
                        break;
                    case Device.WPF:
                        wpfButton.Clicked -= value;
                        break;
                }
            }
        }

        public PlatformButton()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Command):
                    switch(Device.RuntimePlatform)
                    {
                        case Device.macOS:
                            macButton.Command = Command;
                            break;
                        case Device.WPF:
                            wpfButton.Command = Command;
                            break;
                    }
                    break;
                case nameof(Source):
                    switch (Device.RuntimePlatform)
                    {
                        case Device.macOS:
                            macButton.Source = Source;
                            break;
                        case Device.WPF:
                            wpfButton.Source = Source;
                            break;
                    }
                    break;
            }
        }
    }
}