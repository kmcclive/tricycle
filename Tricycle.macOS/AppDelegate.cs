using System;
using System.IO;
using AppKit;
using Foundation;
using StructureMap;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.IO.macOS;
using Tricycle.Media;
using Tricycle.Media.FFmpeg;
using Tricycle.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Tricycle.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
            var rect = new CoreGraphics.CGRect(200, 200, 800, 460);

            MainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            MainWindow.Title = "Tricycle";
        }

        public override NSWindow MainWindow { get; }

        public override void DidFinishLaunching(NSNotification notification)
        {
            string resourcePath = NSBundle.MainBundle.ResourcePath;
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
            AppState.TricycleConfig = new TricycleConfig();

            Forms.Init();
            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
