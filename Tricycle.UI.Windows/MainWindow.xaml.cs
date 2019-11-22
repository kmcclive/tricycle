using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using StructureMap;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.IO.Windows;
using Tricycle.Media;
using Tricycle.Media.FFmpeg;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace Tricycle.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        IAppManager _appManager;

        public MainWindow()
        {
            _appManager = new AppManager();

            InitializeAppState();
            InitializeComponent();
            Forms.Init();         
            LoadApplication(new UI.App());
        }

        void InitializeAppState()
        {
            const string FFMPEG_CONFIG_NAME = "ffmpeg.json";
            const string TRICYCLE_CONFIG_NAME = "tricycle.json";

            string appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string assetsPath = Path.Combine(appPath, "Assets");
            string defaultConfigPath = Path.Combine(assetsPath, "Config");
            string ffmpegPath = Path.Combine(assetsPath, "Tools", "FFmpeg");
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userConfigPath = Path.Combine(appDataPath, "Tricycle");
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
            AppState.DefaultDestinationDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Videos");
        }
    }
}
