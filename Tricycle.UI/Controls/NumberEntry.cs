using Xamarin.Forms;

namespace Tricycle.UI.Controls
{
    public class NumberEntry : Entry
    {
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
                string validatedText = string.Empty;

                for (int i = 0; i < e.NewTextValue.Length; i++)
                {
                    char c = e.NewTextValue[i];

                    if (char.IsDigit(c))
                    {
                        validatedText += c;
                    }
                }

                if (validatedText != e.NewTextValue)
                {
                    entry.Text = validatedText;
                }
            }
        }
    }
}
