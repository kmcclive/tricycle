using System;
using System.Diagnostics;
using System.IO;
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
using Tricycle.Media.Models;
using Tricycle.UI.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Tricycle.UI.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 490;

        public AppDelegate()
        {
            var center = GetCenterCoordinate();
            var rect = new CoreGraphics.CGRect(center.X, center.Y, WINDOW_WIDTH, WINDOW_HEIGHT);
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            MainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            MainWindow.Title = "Tricycle";
        }

        public override NSWindow MainWindow { get; }

        public override void DidFinishLaunching(NSNotification notification)
        {
            string resourcePath = NSBundle.MainBundle.ResourcePath;
            string configPath = Path.Combine(resourcePath, "Config");
            string ffmpegPath = Path.Combine(resourcePath, "Tools", "FFmpeg");
            var processCreator = new Func<IProcess>(() => new ProcessWrapper());
            var processRunner = new ProcessRunner(processCreator);

            AppState.IocContainer = new Container(_ =>
            {
                _.For<IFileBrowser>().Use<FileBrowser>();
                _.For<IMediaInspector>().Use(new MediaInspector(Path.Combine(ffmpegPath, "ffprobe"),
                                                                processRunner,
                                                                ProcessUtility.Self));
                _.For<ICropDetector>().Use(new CropDetector(Path.Combine(ffmpegPath, "ffmpeg"),
                                                            processRunner,
                                                            ProcessUtility.Self));
            });
            AppState.TricycleConfig = ReadConfigFile(Path.Combine(configPath, "tricycle.json"));
            AppState.DefaultDestinationDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Movies");

            Forms.Init();
            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        TricycleConfig ReadConfigFile(string fileName)
        {
            TricycleConfig result = null;
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter(new CamelCaseNamingStrategy()) },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            try
            {
                string json = File.ReadAllText(fileName);

                result = JsonConvert.DeserializeObject<TricycleConfig>(json, serializerSettings);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
            }

            return result ?? new TricycleConfig();
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
