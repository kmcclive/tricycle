using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class Pill : ContentView
    {
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
          nameof(TextColor),
          typeof(Color),
          typeof(Pill));

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
          nameof(Text),
          typeof(string),
          typeof(Pill));

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public string Text
        {
            get { return GetValue(TextProperty).ToString(); }
            set { SetValue(TextProperty, value); }
        }

        public Pill()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(TextColor):
                    label.TextColor = TextColor;
                    break;
                case nameof(Text):
                    label.Text = Text;
                    break;
            }
        }
    }
}
