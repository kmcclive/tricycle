using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class FramedPanel : ContentView
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
          nameof(Title),
          typeof(string),
          typeof(FramedPanel));

        public string Title
        {
            get { return GetValue(TitleProperty)?.ToString(); }
            set { SetValue(TitleProperty, value); }
        }

        public FramedPanel()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Title):
                    if (GetTemplateChild("lblTitle") is Label lblTitle)
                    {
                        lblTitle.Text = Title;
                    }
                    break;
            }
        }
    }
}
