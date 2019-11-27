using System;
using System.Linq;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class ConfigPage : ContentPage
    {
        enum Section
        {
            General,
            Video,
            Audio,
            Advanced
        }

        public ConfigPage()
        {
            InitializeComponent();

            var sections = Enum.GetValues(typeof(Section)).Cast<Section>().ToArray();
            var selectedSection = sections[0];

            vwSections.ItemsSource = sections;
            vwSections.SelectedItem = selectedSection;

            SelectSection(selectedSection);

            vwSections.ItemSelected += OnSectionSelected;
            btnClose.Clicked += OnClose;
        }

        void OnSectionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SelectSection((Section)e.SelectedItem);
        }

        async void OnClose(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        void SelectSection(Section section)
        {
            pnlSection.Title = section.ToString();

            foreach (var child in stackSections.Children)
            {
                child.IsVisible = false;
            }

            ContentView view = null;

            switch (section)
            {
                case Section.General:
                    view = sctGeneral;
                    break;
                case Section.Video:
                    view = sctVideo;
                    break;
                case Section.Audio:
                    view = sctAudio;
                    break;
                case Section.Advanced:
                    view = sctAdvanced;
                    break;
            }

            if (view != null)
            {
                // Using Device to invoke this seems to workaround a bug with macOS
                Device.InvokeOnMainThreadAsync(() => view.IsVisible = true);
            }
        }
    }
}
