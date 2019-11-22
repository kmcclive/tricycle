using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
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
using Tricycle.UI.Views;
using Tricycle.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;
using Xamarin.Forms.Platform.WPF.Controls;
using ControlTemplate = System.Windows.Controls.ControlTemplate;
using Menu = System.Windows.Controls.Menu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Tricycle.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        IAppManager _appManager;
        MenuItem _openFileItem;
        MenuItem _optionsItem;
        ConfigurationPage _configurationPage;

        public MainWindow()
        {
            _appManager = new AppManager();

            _appManager.Busy += () =>
            {
                _openFileItem.IsEnabled = false;
                _optionsItem.IsEnabled = false;
            };
            _appManager.Ready += () =>
            {
                _openFileItem.IsEnabled = true;
                _optionsItem.IsEnabled = true;
            };

            InitializeAppState();
            InitializeComponent();
            Forms.Init();         
            LoadApplication(new UI.App());
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);

            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(CreateMenu));
        }

        void CreateMenu()
        {
            var contentContainer = Template.FindName("PART_ContentControl", this) as FormsContentControl;

            if (contentContainer == null)
            {
                return;
            }

            var panel = new StackPanel();
            var brush = new SolidColorBrush(Colors.WhiteSmoke);
            var menu = new Menu()
            {
                IsMainMenu = true,
                Background = brush,
                Padding = new System.Windows.Thickness(10, 5, 10, 5),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };
            var content = contentContainer.Content as UIElement;

            contentContainer.Content = panel;

            panel.Children.Add(menu);
            panel.Children.Add(content);

            var fileItem = new MenuItem()
            {
                Background = brush,
                Header = "_File"
            };

            menu.Items.Add(fileItem);

            _openFileItem = new MenuItem()
            {
                Background = brush,
                Header = "_Open…",
            };

            _openFileItem.Click += OnOpenFileClick;
            fileItem.Items.Add(_openFileItem);

            var toolsItem = new MenuItem()
            {
                Background = brush,
                Header = "_Tools"
            };

            menu.Items.Add(toolsItem);

            _optionsItem = new MenuItem()
            {
                Background = brush,
                Header = "_Options…"
            };

            _optionsItem.Click += OnOptionsClick;
            toolsItem.Items.Add(_optionsItem);
        }

        void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            var browser = new FileBrowser();
            var result = browser.BrowseToOpen().GetAwaiter().GetResult();

            if (result.Confirmed)
            {
                _appManager.RaiseFileOpened(result.FileName);
            }
        }

        private void OnOptionsClick(object sender, RoutedEventArgs e)
        {
            if (_configurationPage == null)
            {
                _configurationPage = new ConfigurationPage();
            }

            _appManager.RaiseModalOpened(_configurationPage);
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
