using System.ComponentModel;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.UI.ViewModels;
using Tricycle.Utilities;
using Xamarin.Forms;

namespace Tricycle.UI
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var viewModel = new MainViewModel(
                AppState.IocContainer.GetInstance<IFileBrowser>(),
                AppState.IocContainer.GetInstance<IMediaInspector>(),
                AppState.IocContainer.GetInstance<IMediaTranscoder>(),
                AppState.IocContainer.GetInstance<ICropDetector>(),
                AppState.IocContainer.GetInstance<ITranscodeCalculator>(),
                AppState.IocContainer.GetInstance<IFileSystem>(),
                AppState.IocContainer.GetInstance<IDevice>(),
                AppState.IocContainer.GetInstance<IAppManager>(),
                AppState.TricycleConfig,
                AppState.DefaultDestinationDirectory);

            viewModel.Alert += OnAlert;
            viewModel.Confirm += OnConfirm;

            BindingContext = viewModel;
        }

        private void OnAlert(string title, string message)
        {
            DisplayAlert(title, message, "OK");
        }

        private Task<bool> OnConfirm(string title, string message)
        {
            return DisplayAlert(title, message, "OK", "Cancel");
        }
    }
}
