using System;

using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class ConfigPage : ContentPage
    {
        public ConfigPage()
        {
            InitializeComponent();

            var sections = new string[] { "General", "Advanced" };

            vwSections.ItemsSource = sections;
            vwSections.SelectedItem = sections[0];

            btnClose.Clicked += onClose;
        }

        async void onClose(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
