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

            btnClose.Clicked += onClose;
        }

        async void onClose(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
