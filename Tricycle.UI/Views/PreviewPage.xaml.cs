using System;
using System.Collections.Generic;
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
                AppState.IocContainer.GetInstance<IFileSystem>());

            _viewModel.Closed += async () => await OnClosed();

            BindingContext = _viewModel;
        }

        public Task Load(TranscodeJob job)
        {
            return _viewModel.Load(job);
        }

        async Task OnClosed()
        {
            await Navigation.PopModalAsync();
            _appManager.RaiseModalClosed();
        }
    }
}
