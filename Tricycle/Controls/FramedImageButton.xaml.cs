using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Tricycle.Controls
{
    public partial class FramedImageButton : ContentView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
          propertyName: "Source",
          returnType: typeof(string),
          declaringType: typeof(FramedImageButton),
          defaultValue: "",
          defaultBindingMode: BindingMode.TwoWay,
          propertyChanged: SourcePropertyChanged);

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
        }

        private static void SourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (FramedImageButton)bindable;
            control.image.Source = ImageSource.FromFile(newValue.ToString());
        }
    }
}
