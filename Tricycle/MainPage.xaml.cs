using System.ComponentModel;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.ViewModels;
using Xamarin.Forms;

namespace Tricycle
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainViewModel(
                AppState.IocContainer.GetInstance<IFileBrowser>(),
                AppState.IocContainer.GetInstance<IMediaInspector>(),
                AppState.IocContainer.GetInstance<ICropDetector>(),
                AppState.TricycleConfig,
                AppState.DefaultDestinationDirectory);
        }
    }
}
