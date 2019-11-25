using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class ConfigurationPage : ContentPage
    {
        public ConfigurationPage()
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
