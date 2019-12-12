using System.IO.Abstractions;
using System.Threading.Tasks;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.UI.ViewModels;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class PreviewPage : ContentPage
    {
        IAppManager _appManager;
        PreviewViewModel _viewModel;

        public PreviewPage()
        {
            InitializeComponent();

            _appManager = AppState.IocContainer.GetInstance<IAppManager>();
            _viewModel = new PreviewViewModel(
                AppState.IocContainer.GetInstance<IPreviewImageGenerator>(),
                AppState.IocContainer.GetInstance<IFileSystem>(),
                AppState.IocContainer.GetInstance<IAppManager>(),
                AppState.IocContainer.GetInstance<IDevice>());

            _viewModel.Closed += async () => await OnClosed();

            BindingContext = _viewModel;
        }

        public TranscodeJob TranscodeJob { get; set; }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.IsPageVisible = true;

            if (TranscodeJob != null)
            {
                Task.Run(() => _viewModel.Load(TranscodeJob));
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _viewModel.IsPageVisible = false;
        }

        async Task OnClosed()
        {
            await Navigation.PopModalAsync();
            _appManager.RaiseModalClosed();
        }
    }
}
