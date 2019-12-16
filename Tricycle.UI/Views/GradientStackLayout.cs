using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public class GradientStackLayout : StackLayout
    {
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
          nameof(BorderColor),
          typeof(Color),
          typeof(GradientStackLayout));

        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
          nameof(BorderWidthProperty),
          typeof(double),
          typeof(GradientStackLayout));

        public static readonly BindableProperty GradientColorsProperty = BindableProperty.Create(
          nameof(GradientColors),
          typeof(ICollection<Color>),
          typeof(GradientStackLayout));

        public static readonly BindableProperty GradientOrientationProperty = BindableProperty.Create(
          nameof(GradientOrientation),
          typeof(StackOrientation),
          typeof(GradientStackLayout));

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public double BorderWidth
        {
            get { return (double)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        public ICollection<Color> GradientColors
        {
            get { return (ICollection<Color>)GetValue(GradientColorsProperty); }
            set { SetValue(GradientColorsProperty, value); }
        }

        public StackOrientation GradientOrientation
        {
            get { return (StackOrientation)GetValue(GradientOrientationProperty); }
            set { SetValue(GradientOrientationProperty, value); }
        }

        public GradientStackLayout()
        {
            SetValue(GradientColorsProperty, new List<Color>());
        }
    }
}
