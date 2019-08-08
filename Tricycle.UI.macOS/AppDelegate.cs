using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using AppKit;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
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
using Tricycle.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Tricycle.UI.macOS
{
    [Register("AppDelegate")]
    public sealed class AppDelegate : FormsApplicationDelegate
    {
        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 540;

        IAppManager _appManager;
        MainWindowDelegate _mainWindowDelegate;
        bool _isBusy = false;

        public AppDelegate()
        {
            var center = GetCenterCoordinate();
            var rect = new CoreGraphics.CGRect(center.X, center.Y, WINDOW_WIDTH, WINDOW_HEIGHT);
            var style = NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            _appManager = new AppManager();
            _mainWindowDelegate = new MainWindowDelegate(_appManager);

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
                Title = "Tricycle",
                Delegate = _mainWindowDelegate
            };
        }

        public override NSWindow MainWindow { get; }

        public override void DidFinishLaunching(NSNotification notification)
        {
            string resourcePath = NSBundle.MainBundle.ResourcePath;
            string configPath = Path.Combine(resourcePath, "Config");
            string ffmpegPath = Path.Combine(resourcePath, "Tools", "FFmpeg");
            var processCreator = new Func<IProcess>(() => new ProcessWrapper());
            var processRunner = new ProcessRunner(processCreator);
            var ffmpegConfig = ReadConfigFile<FFmpegConfig>(Path.Combine(configPath, "ffmpeg.json"));
            var ffmpegArgumentGenerator = new FFmpegArgumentGenerator(ProcessUtility.Self, ffmpegConfig);

            AppState.IocContainer = new Container(_ =>
            {
                _.For<IFileBrowser>().Use<FileBrowser>();
                _.For<IMediaInspector>().Use(new MediaInspector(Path.Combine(ffmpegPath, "ffprobe"),
                                                                processRunner,
                                                                ProcessUtility.Self));
                _.For<ICropDetector>().Use(new CropDetector(Path.Combine(ffmpegPath, "ffmpeg"),
                                                            processRunner,
                                                            ProcessUtility.Self));
                _.For<IFileSystem>().Use<FileSystem>();
                _.For<ITranscodeCalculator>().Use<TranscodeCalculator>();
                _.For<IMediaTranscoder>().Use(new MediaTranscoder(Path.Combine(ffmpegPath, "ffmpeg"),
                                                                  processCreator,
                                                                  ffmpegArgumentGenerator));
                _.For<IDevice>().Use(DeviceWrapper.Self);
                _.For<IAppManager>().Use(_appManager);
            });
            AppState.TricycleConfig = ReadConfigFile<TricycleConfig>(Path.Combine(configPath, "tricycle.json"));
            AppState.DefaultDestinationDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Movies");

            Forms.Init();
            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender) =>
            _mainWindowDelegate.WindowShouldClose(this) ? NSApplicationTerminateReply.Now : NSApplicationTerminateReply.Cancel;

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
            Debug.WriteLine($"{item.Identifier}: {item.Title}");

            switch(item.Identifier)
            {
                case "IAo-SY-fd9":
                case "tXI-mr-wws":
                    return !_isBusy;
                default:
                    return true;
            }
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

        T ReadConfigFile<T>(string fileName) where T : class, new()
        {
            T result = null;
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter(new CamelCaseNamingStrategy()) },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            try
            {
                string json = File.ReadAllText(fileName);

                result = JsonConvert.DeserializeObject<T>(json, serializerSettings);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
            }

            return result ?? new T();
        }

        Coordinate<nfloat> GetCenterCoordinate()
        {
            var frame = NSScreen.MainScreen.VisibleFrame;
            var x = frame.X + (frame.Width - WINDOW_WIDTH) / 2f;
            var y = frame.Y + (frame.Height - WINDOW_HEIGHT) / 2f;

            return new Coordinate<nfloat>(x, y);
        }
    }
}
