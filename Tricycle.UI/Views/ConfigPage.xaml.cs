using System;
using System.Linq;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.UI.ViewModels;
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

            var viewModel = new ConfigViewModel(
                AppState.IocContainer.GetInstance<IConfigManager<TricycleConfig>>(),
                AppState.IocContainer.GetInstance<IConfigManager<FFmpegConfig>>());
            var sections = Enum.GetValues(typeof(Section)).Cast<Section>().ToArray();
            var selectedSection = sections[0];

            BindingContext = viewModel;
            vwSections.ItemsSource = sections;
            vwSections.SelectedItem = selectedSection;

            SelectSection(selectedSection);
            viewModel.Initialize();

            viewModel.Closed += async () => await Navigation.PopModalAsync();
            vwSections.ItemSelected += OnSectionSelected;
            btnClose.Clicked += async (sender, e) => await Navigation.PopModalAsync();
        }

        void OnSectionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SelectSection((Section)e.SelectedItem);
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
