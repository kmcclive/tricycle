using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Models.Jobs;
using Tricycle.UI.ViewModels;
using Tricycle.Utilities;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            var appManager = AppState.IocContainer.GetInstance<IAppManager>();
            _viewModel = new MainViewModel(
                AppState.IocContainer.GetInstance<IFileBrowser>(),
                AppState.IocContainer.GetInstance<IMediaInspector>(),
                AppState.IocContainer.GetInstance<IMediaTranscoder>(),
                AppState.IocContainer.GetInstance<ICropDetector>(),
                AppState.IocContainer.GetInstance<ITranscodeCalculator>(),
                AppState.IocContainer.GetInstance<IFileSystem>(),
                AppState.IocContainer.GetInstance<IDevice>(),
                appManager,
                AppState.IocContainer.GetInstance<IConfigManager<TricycleConfig>>(),
                AppState.DefaultDestinationDirectory);

            appManager.ModalOpened += OnModalOpened;
            _viewModel.Alert += OnAlert;
            _viewModel.Confirm += OnConfirm;

            BindingContext = _viewModel;
        }

        public TranscodeJob GetTranscodeJob()
        {
            return _viewModel.GetTranscodeJob();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.IsPageVisible = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _viewModel.IsPageVisible = false;
        }

        void OnAlert(string title, string message)
        {
            DisplayAlert(title, message, "OK");
        }

        Task<bool> OnConfirm(string title, string message)
        {
            return DisplayAlert(title, message, "OK", "Cancel");
        }

        async void OnModalOpened(Page page)
        {
            if (!Navigation.ModalStack.Any(p => p == page))
            {
                await Navigation.PushModalAsync(page);
            }
        }
    }
}
