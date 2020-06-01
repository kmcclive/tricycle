using System.Globalization;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public class NumberEntry : Entry
    {
        public static readonly BindableProperty AllowDecimalsProperty = BindableProperty.Create(
          nameof(AllowDecimals),
          typeof(bool),
          typeof(NumberEntry));

        public bool AllowDecimals
        {
            get { return (bool)GetValue(AllowDecimalsProperty); }
            set { SetValue(AllowDecimalsProperty, value); }
        }

        public NumberEntry()
        {
            HorizontalTextAlignment = TextAlignment.End;

            TextChanged += OnTextChanged;
        }

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;

            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                string pattern = @"\d*";

                if (AllowDecimals)
                {
                    pattern += $"({Regex.Escape(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)}\\d*)?";
                }

                if (!Regex.IsMatch(e.NewTextValue, $"^{pattern}$"))
                {
                    entry.Text = e.OldTextValue;
                }
            }
        }
    }
}
