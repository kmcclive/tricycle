using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using StructureMap;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.IO.macOS;
using Tricycle.Media;
using Tricycle.Media.FFmpeg;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.UI.Views;
using Tricycle.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Tricycle.UI.macOS
{
    [Register("AppDelegate")]
    public sealed class AppDelegate : FormsApplicationDelegate
    {
        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 560;

        IAppManager _appManager;
        NSDocumentController _documentController;
        MainPage _mainPage;
        ConfigPage _configPage;
        PreviewPage _previewPage;

        public AppDelegate()
        {
            var center = GetCenterCoordinate();
            var rect = new CoreGraphics.CGRect(center.X, center.Y, WINDOW_WIDTH, WINDOW_HEIGHT);
            var style = NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            _appManager = new AppManager();

            _appManager.FileOpened += fileName =>
            {
                var url = new NSUrl($"file://{Uri.EscapeUriString(fileName)}");

                NSDocumentController.SharedDocumentController.NoteNewRecentDocumentURL(url);
            };
            _appManager.QuitConfirmed += () =>
            {
                NSApplication.SharedApplication.Terminate(this);
            };

            MainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Tricycle"
            };
            MainWindow.WindowShouldClose += sender => ShouldClose();
        }

        public override NSWindow MainWindow { get; }

        public override void WillFinishLaunching(NSNotification notification)
        {
            _documentController = new AppDocumentController(_appManager);
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            const string FFMPEG_CONFIG_NAME = "ffmpeg.json";
            const string TRICYCLE_CONFIG_NAME = "tricycle.json";

            string resourcePath = NSBundle.MainBundle.ResourcePath;
            string defaultConfigPath = Path.Combine(resourcePath, "Config");
            string ffmpegPath = Path.Combine(resourcePath, "Tools", "FFmpeg");
            string ffmpegFileName = Path.Combine(ffmpegPath, "ffmpeg");
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string userConfigPath = Path.Combine(userPath, "Library", "Preferences", "Tricycle");
            var processCreator = new Func<IProcess>(() => new ProcessWrapper());
            var processRunner = new ProcessRunner(processCreator);
            var fileSystem = new FileSystem();
            var ffmpegConfigManager =
                new JsonConfigManager<FFmpegConfig>(fileSystem,
                                                    Path.Combine(defaultConfigPath, FFMPEG_CONFIG_NAME),
                                                    Path.Combine(userConfigPath, FFMPEG_CONFIG_NAME));
            var tricycleConfigManager =
                new JsonConfigManager<TricycleConfig>(fileSystem,
                                                      Path.Combine(defaultConfigPath, TRICYCLE_CONFIG_NAME),
                                                      Path.Combine(userConfigPath, TRICYCLE_CONFIG_NAME));

            ffmpegConfigManager.Load();
            tricycleConfigManager.Load();

            var ffmpegArgumentGenerator = new FFmpegArgumentGenerator(new ArgumentPropertyReflector());

            AppState.IocContainer = new Container(_ =>
            {
                _.For<IConfigManager<FFmpegConfig>>().Use(ffmpegConfigManager);
                _.For<IConfigManager<TricycleConfig>>().Use(tricycleConfigManager);
                _.For<IFileBrowser>().Use<FileBrowser>();
                _.For<IProcessUtility>().Use(ProcessUtility.Self);
                _.For<IMediaInspector>().Use(new MediaInspector(Path.Combine(ffmpegPath, "ffprobe"),
                                                                processRunner,
                                                                ProcessUtility.Self));
                _.For<ICropDetector>().Use(new CropDetector(ffmpegFileName,
                                                            processRunner,
                                                            ffmpegConfigManager,
                                                            ffmpegArgumentGenerator));
                _.For<IFileSystem>().Use(fileSystem);
                _.For<ITranscodeCalculator>().Use<TranscodeCalculator>();
                _.For<IArgumentPropertyReflector>().Use<ArgumentPropertyReflector>();
                _.For<IMediaTranscoder>().Use(new MediaTranscoder(ffmpegFileName,
                                                                  processCreator,
                                                                  ffmpegConfigManager,
                                                                  ffmpegArgumentGenerator));
                _.For<IDevice>().Use(DeviceWrapper.Self);
                _.For<IAppManager>().Use(_appManager);
                _.For<IPreviewImageGenerator>().Use(new PreviewImageGenerator(ffmpegFileName,
                                                                              processRunner,
                                                                              ffmpegArgumentGenerator,
                                                                              ffmpegConfigManager,
                                                                              fileSystem));
            });
            AppState.DefaultDestinationDirectory = Path.Combine(userPath, "Movies");

            Forms.Init();

            var app = new App();

            _mainPage = app.MainPage as MainPage;

            LoadApplication(app);

            base.DidFinishLaunching(notification);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender) =>
            ShouldClose() ? NSApplicationTerminateReply.Now : NSApplicationTerminateReply.Cancel;

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        public override bool OpenFile(NSApplication sender, string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            _appManager.RaiseFileOpened(filename);
            return true;
        }

        [Action("validateMenuItem:")]
        public bool ValidateMenuItem(NSMenuItem item)
        {
            switch(item.Title)
            {
                case "Open…":
                    return !_appManager.IsBusy;
                case "Preferences…":
                    return !_appManager.IsBusy;
                default:
                    return true;
            }
        }

        [Action("openPreferences:")]
        public void OpenPreferences(NSObject sender)
        {
            if (_configPage == null)
            {
                _configPage = new ConfigPage();
            }

            _appManager.RaiseModalOpened(_configPage);
        }

        [Export("openDocument:")]
        void OpenFile(NSObject sender)
        {
            var browser = new FileBrowser();
            var result = browser.BrowseToOpen().GetAwaiter().GetResult();

            if (result.Confirmed)
            {
                _appManager.RaiseFileOpened(result.FileName);
            }
        }

        [Action("viewPreview:")]
        public void ViewPreview(NSObject sender)
        {
            var job = _mainPage?.GetTranscodeJob();

            if (job == null)
            {
                return;
            }

            if (_previewPage == null)
            {
                _previewPage = new PreviewPage();
            }

            Task.Run(() => _previewPage.Load(job));

            _appManager.RaiseModalOpened(_previewPage);
        }

        Coordinate<nfloat> GetCenterCoordinate()
        {
            var frame = NSScreen.MainScreen.VisibleFrame;
            var x = frame.X + (frame.Width - WINDOW_WIDTH) / 2f;
            var y = frame.Y + (frame.Height - WINDOW_HEIGHT) / 2f;

            return new Coordinate<nfloat>(x, y);
        }

        bool ShouldClose()
        {
            if (!_appManager.IsQuitConfirmed)
            {
                _appManager.RaiseQuitting();
            }

            return _appManager.IsQuitConfirmed;
        }
    }
}
