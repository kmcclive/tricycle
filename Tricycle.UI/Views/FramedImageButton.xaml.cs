using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class FramedImageButton : ContentView
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
          nameof(Command),
          typeof(ICommand),
          typeof(FramedImageButton));
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
          nameof(Source),
          typeof(ImageSource),
          typeof(FramedImageButton));

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

        public event EventHandler Clicked;

        public FramedImageButton()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanging(propertyName);

            if (propertyName == nameof(Command))
            {
                var command = Command;

                if (command != null)
                {
                    command.CanExecuteChanged -= OnCanExecuteChanged;
                }
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Command):
                    var command = Command;

                    if (command != null)
                    {
                        command.CanExecuteChanged += OnCanExecuteChanged;
                    }

                    UpdateCurtainVisibility();
                    break;
                case nameof(IsEnabled):
                    UpdateCurtainVisibility();
                    break;
                case nameof(Source):
                    image.Source = Source;
                    break;
            }
        }

        void OnCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCurtainVisibility();
        }

        void OnTapped(object sender, EventArgs args)
        {
            if (curtain.IsVisible)
            {
                return;
            }

            var oldColor = frame.BackgroundColor;

            frame.BackgroundColor = Color.FromHex("e9e9e9");

            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                frame.BackgroundColor = oldColor;

                //This prevents the UI from getting hung up
                Device.StartTimer(TimeSpan.FromTicks(1), () =>
                {
                    Clicked?.Invoke(this, args);
                    Command?.Execute(null);
                    return false;
                });

                return false;
            });
        }

        void UpdateCurtainVisibility()
        {
            curtain.IsVisible = !IsEnabled || Command?.CanExecute(null) == false;
        }
    }
}
