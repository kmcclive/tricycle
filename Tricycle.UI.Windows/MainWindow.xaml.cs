using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
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
using Xamarin.Forms.Platform.WPF.Controls;

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

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            var topBar = Template.FindName("PART_TopAppBar", this) as FormsAppBar;

            if (topBar != null)
            {
                topBar.MaxHeight = 0;
            }
        }

        void InitializeAppState()
        {
            string appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string assetsPath = Path.Combine(appPath, "Assets");
            string configPath = Path.Combine(assetsPath, "Config");
            string ffmpegPath = Path.Combine(assetsPath, "Tools", "FFmpeg");
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
                                                            ProcessUtility.Self,
                                                            ffmpegConfig));
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
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Videos");
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
    }
}
