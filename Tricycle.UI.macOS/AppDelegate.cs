using System;
using System.IO;
using System.IO.Abstractions;
using AppKit;
using Foundation;
using StructureMap;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.IO.macOS;
using Tricycle.Media;
using Tricycle.Media.FFmpeg;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.UI.Models;
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
        volatile bool _isBusy = false;
        ConfigPage _configPage;

        public AppDelegate()
        {
            var center = GetCenterCoordinate();
            var rect = new CoreGraphics.CGRect(center.X, center.Y, WINDOW_WIDTH, WINDOW_HEIGHT);
            var style = NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            _appManager = new AppManager();

            _appManager.Busy += () =>
            {
                _isBusy = true;
            };
            _appManager.Ready += () =>
            {
                _isBusy = false;
            };
            _appManager.FileOpened += fileName =>
            {
                var url = new NSUrl($"file://{Uri.EscapeUriString(fileName)}");

                NSDocumentController.SharedDocumentController.NoteNewRecentDocumentURL(url);
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

            var ffmpegArgumentGenerator = new FFmpegArgumentGenerator(ProcessUtility.Self, ffmpegConfigManager);

            AppState.IocContainer = new Container(_ =>
            {
                _.For<IConfigManager<FFmpegConfig>>().Use(ffmpegConfigManager);
                _.For<IConfigManager<TricycleConfig>>().Use(tricycleConfigManager);
                _.For<IFileBrowser>().Use<FileBrowser>();
                _.For<IProcessUtility>().Use(ProcessUtility.Self);
                _.For<IMediaInspector>().Use(new MediaInspector(Path.Combine(ffmpegPath, "ffprobe"),
                                                                processRunner,
                                                                ProcessUtility.Self));
                _.For<ICropDetector>().Use(new CropDetector(Path.Combine(ffmpegPath, "ffmpeg"),
                                                            processRunner,
                                                            ProcessUtility.Self,
                                                            ffmpegConfigManager));
                _.For<IFileSystem>().Use(fileSystem);
                _.For<ITranscodeCalculator>().Use<TranscodeCalculator>();
                _.For<IMediaTranscoder>().Use(new MediaTranscoder(Path.Combine(ffmpegPath, "ffmpeg"),
                                                                  processCreator,
                                                                  ffmpegArgumentGenerator));
                _.For<IDevice>().Use(DeviceWrapper.Self);
                _.For<IAppManager>().Use(_appManager);
            });
            AppState.DefaultDestinationDirectory = Path.Combine(userPath, "Movies");

            Forms.Init();
            LoadApplication(new App());

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
                    return !_isBusy;
                case "Preferences…":
                    return !_isBusy;
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

        Coordinate<nfloat> GetCenterCoordinate()
        {
            var frame = NSScreen.MainScreen.VisibleFrame;
            var x = frame.X + (frame.Width - WINDOW_WIDTH) / 2f;
            var y = frame.Y + (frame.Height - WINDOW_HEIGHT) / 2f;

            return new Coordinate<nfloat>(x, y);
        }

        bool ShouldClose()
        {
            var cancellation = new CancellationArgs();

            _appManager.RaiseQuitting(cancellation);

            return !cancellation.Cancel;
        }
    }
}
