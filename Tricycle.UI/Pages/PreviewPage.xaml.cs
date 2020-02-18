using System.IO.Abstractions;
using System.Threading.Tasks;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.UI.ViewModels;
using Xamarin.Forms;

namespace Tricycle.UI.Pages
{
    public partial class PreviewPage : ContentPage
    {
        PreviewViewModel _viewModel;

        public PreviewPage(IAppManager appManager)
        {
            InitializeComponent();

            _viewModel = new PreviewViewModel(
                AppState.IocContainer.GetInstance<IPreviewImageGenerator>(),
                AppState.IocContainer.GetInstance<IFileSystem>(),
                appManager,
                AppState.IocContainer.GetInstance<IDevice>());

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

            _viewModel.Close();
            _viewModel.IsPageVisible = false;
        }
    }
}
