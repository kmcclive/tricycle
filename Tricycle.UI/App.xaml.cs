using System.Linq;
using System.Threading.Tasks;
using Tricycle.UI.Pages;
using Xamarin.Forms;

namespace Tricycle.UI
{
    public partial class App : Application
    {
        IAppManager _appManager;
        INavigation _navigation;
        MainPage _mainPage;
        ConfigPage _configPage;
        PreviewPage _previewPage;

        public App(IAppManager appManager)
        {
            InitializeComponent();

            _appManager = appManager;
            _mainPage = new MainPage(appManager);
            _navigation = _mainPage.Navigation;

            MainPage = _mainPage;

            NavigationPage.SetHasNavigationBar(_mainPage, false);

            _appManager.ModalOpened += async (modal) => await OnModalOpened(modal);
            _appManager.ModalClosed += async () => await OnModalClosed();
        }

        protected ConfigPage ConfigPage
        {
            get
            {
                if (_configPage == null)
                {
                    _configPage = new ConfigPage(_appManager);
                }

                return _configPage;
            }
        }

        protected PreviewPage PreviewPage
        {
            get
            {
                if (_previewPage == null)
                {
                    _previewPage = new PreviewPage(_appManager);
                }

                return _previewPage;
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        async Task OnModalOpened(Modal modal)
        {
            Page page = null;

            switch (modal)
            {
                case Modal.Config:
                    page = ConfigPage;
                    break;
                case Modal.Preview:
                    PreviewPage.TranscodeJob = _mainPage.GetTranscodeJob();
                    page = PreviewPage;
                    break;
            }

            if ((page != null) && !_navigation.ModalStack.Any(p => p == page))
            {
                await _navigation.PushModalAsync(page, false);
            }
        }

        async Task OnModalClosed()
        {
            await _navigation.PopModalAsync(false);
        }
    }
}
