using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tricycle.Controls
{
    public partial class FramedImageButton : ContentView
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
          nameof(Command),
          typeof(ICommand),
          typeof(FramedImageButton));
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
          nameof(Source),
          typeof(string),
          typeof(FramedImageButton));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public string Source
        {
            get { return GetValue(SourceProperty).ToString(); }
            set { SetValue(SourceProperty, value); }
        }

        public event EventHandler Clicked;

        public FramedImageButton()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "Command":
                case "IsEnabled":
                    curtain.IsVisible = !IsEnabled || Command?.CanExecute(null) == false;
                    break;
                case "Source":
                    image.Source = ImageSource.FromFile(Source);
                    break;
            }
        }

        void OnFrameTapped(object sender, EventArgs args)
        {
            var oldColor = frame.BackgroundColor;

            frame.BackgroundColor = Color.FromHex("e9e9e9");

            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                frame.BackgroundColor = oldColor;
                return false;
            });

            Clicked?.Invoke(this, args);
            Command?.Execute(null);
        }
    }
}
