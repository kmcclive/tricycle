using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Bridge;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Globalization;
using Tricycle.IO;
using Tricycle.IO.UWP;
using Tricycle.Media;
using Tricycle.Media.FFmpeg;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Models.Templates;
using Tricycle.Utilities;
using Windows.Storage;
using Xamarin.Forms.Platform.UWP;
using IFileSystem = System.IO.Abstractions.IFileSystem;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tricycle.UI.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : WindowsPage
    {
        IAppManager _appManager = new AppManager();
        IConfigManager<Dictionary<string, JobTemplate>> _templateManager;

        public MainPage()
        {
            InitializeAppState();
            InitializeComponent();
            LoadApplication(new UI.App(_appManager));
        }

        void InitializeAppState()
        {
            const string FFMPEG_CONFIG_NAME = "ffmpeg.json";
            const string TRICYCLE_CONFIG_NAME = "tricycle.json";
            const string TEMPLATE_CONFIG_NAME = "templates.json";

            var asm = Assembly.GetExecutingAssembly();

            AppState.AppName = asm.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            AppState.AppVersion = asm.GetName().Version;

            string appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string assetsPath = Path.Combine(appPath, "Assets");
            string defaultConfigPath = Path.Combine(assetsPath, "Config");
            string ffmpegPath = Path.Combine(assetsPath, "Tools", "FFmpeg");
            string ffmpegFileName = Path.Combine(ffmpegPath, "ffmpeg.exe");
            string appDataPath = ApplicationData.Current.RoamingFolder.Path;
            string userConfigPath = Path.Combine(appDataPath, "Tricycle");
            var connSerializer = new JsonSerializer(new Newtonsoft.Json.JsonSerializerSettings());
            var processCreator = new Func<IProcess>(() => new ProcessClient(() => WindowsAppState.Connection, connSerializer));
            var processRunner = new ProcessRunner(processCreator);
            var fileSystem = new FileSystem(new System.IO.Abstractions.FileSystem());
            var jsonSerializer = new JsonSerializer();
            var ffmpegConfigManager = new FFmpegConfigManager(fileSystem,
                                                              jsonSerializer,
                                                              Path.Combine(defaultConfigPath, FFMPEG_CONFIG_NAME),
                                                              Path.Combine(userConfigPath, FFMPEG_CONFIG_NAME));
            var tricycleConfigManager = new TricycleConfigManager(fileSystem,
                                                                  jsonSerializer,
                                                                  Path.Combine(defaultConfigPath, TRICYCLE_CONFIG_NAME),
                                                                  Path.Combine(userConfigPath, TRICYCLE_CONFIG_NAME));
            _templateManager =
                new FileConfigManager<Dictionary<string, JobTemplate>>(
                    fileSystem,
                    jsonSerializer,
                    Path.Combine(defaultConfigPath, TEMPLATE_CONFIG_NAME),
                    Path.Combine(userConfigPath, TEMPLATE_CONFIG_NAME));

            ffmpegConfigManager.Load();
            tricycleConfigManager.Load();
            _templateManager.Load();

            if (tricycleConfigManager.Config.Trace)
            {
                // EnableLogging();
            }

            // _templateManager.ConfigChanged += config => PopulateTemplateMenu();

            var ffmpegArgumentGenerator = new FFmpegArgumentGenerator(new ArgumentPropertyReflector());

            AppState.IocContainer = new Container(_ =>
            {
                _.For<IConfigManager<FFmpegConfig>>().Use(ffmpegConfigManager);
                _.For<IConfigManager<TricycleConfig>>().Use(tricycleConfigManager);
                _.For<IConfigManager<Dictionary<string, JobTemplate>>>().Use(_templateManager);
                _.For<IFileBrowser>().Use<FileBrowser>();
                _.For<IFolderBrowser>().Use<FolderBrowser>();
                _.For<IProcessUtility>().Use(ProcessUtility.Self);
                _.For<IMediaInspector>().Use(new MediaInspector(Path.Combine(ffmpegPath, "ffprobe.exe"),
                                                                processRunner,
                                                                ProcessUtility.Self,
                                                                jsonSerializer));
                _.For<ICropDetector>().Use(new CropDetector(ffmpegFileName,
                                                            processRunner,
                                                            ffmpegConfigManager,
                                                            ffmpegArgumentGenerator));
                _.For<IInterlaceDetector>().Use(new InterlaceDetector(ffmpegFileName,
                                                                      processRunner,
                                                                      ffmpegArgumentGenerator));
                _.For<IFileSystem>().Use(fileSystem);
                _.For<ITranscodeCalculator>().Use<TranscodeCalculator>();
                _.For<IMediaTranscoder>().Use(new MediaTranscoder(ffmpegFileName,
                                                                  processCreator,
                                                                  ffmpegConfigManager,
                                                                  ffmpegArgumentGenerator));
                _.For<IDevice>().Use(DeviceWrapper.Self);
                _.For<IAppManager>().Use(_appManager);
                _.For<IPreviewImageGenerator>().Use(new PreviewImageGenerator(ffmpegFileName,
                                                                              processRunner,
                                                                              ffmpegConfigManager,
                                                                              ffmpegArgumentGenerator,
                                                                              fileSystem));
                _.For<ILanguageService>().Use<LanguageService>();
            });
            AppState.DefaultDestinationDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Videos");
        }
    }
}
