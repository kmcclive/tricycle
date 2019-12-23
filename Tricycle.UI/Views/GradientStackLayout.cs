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

        public static readonly BindableProperty LeftBorderColorProperty = BindableProperty.Create(
          nameof(LeftBorderColor),
          typeof(Color),
          typeof(GradientStackLayout));

        public static readonly BindableProperty LeftBorderWidthProperty = BindableProperty.Create(
          nameof(LeftBorderWidthProperty),
          typeof(double),
          typeof(GradientStackLayout));

        public static readonly BindableProperty TopBorderColorProperty = BindableProperty.Create(
          nameof(TopBorderColor),
          typeof(Color),
          typeof(GradientStackLayout));

        public static readonly BindableProperty TopBorderWidthProperty = BindableProperty.Create(
          nameof(TopBorderWidthProperty),
          typeof(double),
          typeof(GradientStackLayout));

        public static readonly BindableProperty RightBorderColorProperty = BindableProperty.Create(
          nameof(RightBorderColor),
          typeof(Color),
          typeof(GradientStackLayout));

        public static readonly BindableProperty RightBorderWidthProperty = BindableProperty.Create(
          nameof(RightBorderWidthProperty),
          typeof(double),
          typeof(GradientStackLayout));

        public static readonly BindableProperty BottomBorderColorProperty = BindableProperty.Create(
          nameof(BottomBorderColor),
          typeof(Color),
          typeof(GradientStackLayout));

        public static readonly BindableProperty BottomBorderWidthProperty = BindableProperty.Create(
          nameof(BottomBorderWidthProperty),
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

        public Color LeftBorderColor
        {
            get { return (Color)GetValue(LeftBorderColorProperty); }
            set { SetValue(LeftBorderColorProperty, value); }
        }

        public double LeftBorderWidth
        {
            get { return (double)GetValue(LeftBorderWidthProperty); }
            set { SetValue(LeftBorderWidthProperty, value); }
        }

        public Color TopBorderColor
        {
            get { return (Color)GetValue(TopBorderColorProperty); }
            set { SetValue(TopBorderColorProperty, value); }
        }

        public double TopBorderWidth
        {
            get { return (double)GetValue(TopBorderWidthProperty); }
            set { SetValue(TopBorderWidthProperty, value); }
        }

        public Color RightBorderColor
        {
            get { return (Color)GetValue(RightBorderColorProperty); }
            set { SetValue(RightBorderColorProperty, value); }
        }

        public double RightBorderWidth
        {
            get { return (double)GetValue(RightBorderWidthProperty); }
            set { SetValue(RightBorderWidthProperty, value); }
        }

        public Color BottomBorderColor
        {
            get { return (Color)GetValue(BottomBorderColorProperty); }
            set { SetValue(BottomBorderColorProperty, value); }
        }

        public double BottomBorderWidth
        {
            get { return (double)GetValue(BottomBorderWidthProperty); }
            set { SetValue(BottomBorderWidthProperty, value); }
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
