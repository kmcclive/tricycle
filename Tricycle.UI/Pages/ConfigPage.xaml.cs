using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Models.Templates;
using Tricycle.UI.ViewModels;
using Xamarin.Forms;

namespace Tricycle.UI.Pages
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

        ConfigViewModel _viewModel;

        public ConfigPage(IAppManager appManager)
        {
            InitializeComponent();

            _viewModel = new ConfigViewModel(
                AppState.IocContainer.GetInstance<IConfigManager<TricycleConfig>>(),
                AppState.IocContainer.GetInstance<IConfigManager<FFmpegConfig>>(),
                AppState.IocContainer.GetInstance<IConfigManager<Dictionary<string, JobTemplate>>>(),
                appManager,
                AppState.IocContainer.GetInstance<IDevice>());
            var sections = Enum.GetValues(typeof(Section)).Cast<Section>().ToArray();
            var selectedSection = sections[0];

            BindingContext = _viewModel;
            vwSections.ItemsSource = sections;
            vwSections.SelectedItem = selectedSection;

            SelectSection(selectedSection);

            vwSections.ItemSelected += OnSectionSelected;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SelectSection(Section.General);
            _viewModel.Initialize();
            _viewModel.IsPageVisible = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _viewModel.Close();
            _viewModel.IsPageVisible = false;
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
