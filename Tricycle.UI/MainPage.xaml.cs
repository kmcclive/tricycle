using System.ComponentModel;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.UI.Models;
using Tricycle.UI.ViewModels;
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
                AppState.IocContainer.GetInstance<ICropDetector>(),
                AppState.TricycleConfig,
                AppState.DefaultDestinationDirectory);

            viewModel.Alert += OnAlert;

            BindingContext = viewModel;
        }

        private void OnAlert(string title, string message)
        {
            DisplayAlert(title, message, "OK");
        }
    }
}
