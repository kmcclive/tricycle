using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using Iso639;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;
using Tricycle.Models.Templates;
using Tricycle.UI.Models;
using Tricycle.Utilities;
using Xamarin.Forms;

namespace Tricycle.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constants

        const int DEFAULT_STEP_COUNT = 4;
        const string DEFAULT_EXTENSION = "mp4";
        const string PLAY_IMAGE = "Images/play.png";
        const string STOP_IMAGE = "Images/stop.png";
        static readonly ListItem ORIGINAL_OPTION = new ListItem("Same as source", Guid.NewGuid());
        static readonly ListItem NONE_OPTION = new ListItem("None", Guid.NewGuid());

        #endregion

        #region Fields

        readonly IFileBrowser _fileBrowser;
        readonly IMediaInspector _mediaInspector;
        readonly IMediaTranscoder _mediaTranscoder;
        readonly ICropDetector _cropDetector;
        readonly IInterlaceDetector _interlaceDetector;
        readonly ITranscodeCalculator _transcodeCalculator;
        readonly IFileSystem _fileSystem;
        readonly IDevice _device;
        readonly IAppManager _appManager;
        readonly IConfigManager<TricycleConfig> _configManager;
        readonly IConfigManager<Dictionary<string, JobTemplate>> _templateManager;
        readonly string _defaultDestinationDirectory;

        bool _isSourceInfoVisible;
        string _sourceDuration;
        string _sourceSize;
        bool _isSourceHdr;
        string _sourceName;
        bool _isVideoConfigEnabled;
        IList<ListItem> _videoFormatOptions;
        ListItem _selectedVideoFormat;
        double _qualityMin = 0;
        double _qualityMax = 1;
        double _quality = 0.5;
        int _qualityStepCount = DEFAULT_STEP_COUNT;
        bool _isHdrEnabled;
        bool _isHdrChecked;
        IList<ListItem> _sizeOptions;
        ListItem _selectedSize;
        IList<ListItem> _cropOptions;
        ListItem _selectedCropOption;
        bool _isAutoCropControlVisible;
        bool _isManualCropControlVisible;
        bool _isAutocropEnabled;
        bool _isAutocropChecked;
        IList<ListItem> _aspectRatioOptions;
        ListItem _selectedAspectRatio;
        string _cropTop;
        string _cropBottom;
        string _cropLeft;
        string _cropRight;
        bool _isDenoiseChecked;
        IList<ListItem> _subtitleOptions;
        ListItem _selectedSubtitle;
        bool _isForcedSubtitlesEnabled;
        bool _isForcedSubtitlesChecked;
        IList<AudioOutputViewModel> _audioOutputs = new ObservableCollection<AudioOutputViewModel>();
        bool _isContainerFormatEnabled;
        IList<ListItem> _containerFormatOptions;
        ListItem _selectedContainerFormat;
        string _destinationName;
        double _progress;
        string _startImageSource = PLAY_IMAGE;
        string _status;
        bool _isSpinnerVisible;

        TricycleConfig _tricycleConfig;
        MediaInfo _sourceInfo;
        CropParameters _cropParameters;
        Dimensions _croppedDimensions;
        bool _isInterlaced;
        string _defaultExtension = DEFAULT_EXTENSION;
        VideoStreamInfo _primaryVideoStream;
        IList<ListItem> _audioFormatOptions;
        IList<ListItem> _audioTrackOptions;
        IDictionary<AudioFormat, IList<ListItem>> _audioMixdownOptionsByFormat;
        volatile bool _isRunning = false;
        bool _isSourceSelectionEnabled = true;
        bool _isDestinationSelectionEnabled = false;
        bool _isStartEnabled = false;

        #endregion

        #region Constructors

        public MainViewModel(IFileBrowser fileBrowser,
                             IMediaInspector mediaInspector,
                             IMediaTranscoder mediaTranscoder,
                             ICropDetector cropDetector,
                             IInterlaceDetector interlaceDetector,
                             ITranscodeCalculator transcodeCalculator,
                             IFileSystem fileSystem,
                             IDevice device,
                             IAppManager appManager,
                             IConfigManager<TricycleConfig> configManager,
                             IConfigManager<Dictionary<string, JobTemplate>> templateManager,
                             string defaultDestinationDirectory)
        {
            _fileBrowser = fileBrowser;
            _mediaInspector = mediaInspector;
            _mediaTranscoder = mediaTranscoder;
            _cropDetector = cropDetector;
            _interlaceDetector = interlaceDetector;
            _transcodeCalculator = transcodeCalculator;
            _fileSystem = fileSystem;
            _device = device;
            _appManager = appManager;
            _configManager = configManager;
            _tricycleConfig = configManager.Config;
            _templateManager = templateManager;
            _defaultDestinationDirectory = defaultDestinationDirectory;

            _mediaTranscoder.Completed += OnTranscodeCompleted;
            _mediaTranscoder.Failed += OnTranscodeFailed;
            _mediaTranscoder.StatusChanged += OnTranscodeStatusChanged;

            _appManager.FileOpened += async fileName => await OpenSource(fileName);
            _appManager.Quitting += OnAppQuitting;
            _appManager.TemplateSaved += SaveTemplate;
            _appManager.TemplateApplied += ApplyTemplate;

            _configManager.ConfigChanged += async config => await OnConfigChanged(config);

            SourceSelectCommand = new Command(async () => await SelectSource(),
                                              () => _isSourceSelectionEnabled);
            DestinationSelectCommand = new Command(async () => await SelectDestination(),
                                                   () => _isDestinationSelectionEnabled);
            StartCommand = new Command(ToggleRunning,
                                       () => _isStartEnabled);
            PreviewCommand = new Command(() => _appManager.RaiseModalOpened(Modal.Preview),
                                         () => _isStartEnabled && !_isRunning);

            ContainerFormatOptions = GetContainerFormatOptions();
            SelectedContainerFormat = ContainerFormatOptions?.FirstOrDefault();

            CropOptions = GetCropOptions();
            SelectedCropOption = CropOptions?.FirstOrDefault();
        }

        #endregion

        #region Properties

        public bool IsPageVisible { get; set; }

        public bool IsSourceInfoVisible
        {
            get { return _isSourceInfoVisible; }
            set { SetProperty(ref _isSourceInfoVisible, value); }
        }

        public string SourceDuration
        {
            get { return _sourceDuration; }
            set { SetProperty(ref _sourceDuration, value); }
        }

        public string SourceSize
        {
            get { return _sourceSize; }
            set { SetProperty(ref _sourceSize, value); }
        }

        public bool IsSourceHdr
        {
            get { return _isSourceHdr; }
            set { SetProperty(ref _isSourceHdr, value); }
        }

        public string SourceName
        {
            get { return _sourceName; }
            set { SetProperty(ref _sourceName, value); }
        }

        public bool IsVideoConfigEnabled
        {
            get { return _isVideoConfigEnabled; }
            set
            {
                SetProperty(ref _isVideoConfigEnabled, value);
                RaisePropertyChanged(nameof(IsHdrEnabled));
                RaisePropertyChanged(nameof(IsAutocropEnabled));
                RaisePropertyChanged(nameof(IsForcedSubtitlesEnabled));
            }
        }

        public IList<ListItem> VideoFormatOptions
        {
            get { return _videoFormatOptions; }
            set { SetProperty(ref _videoFormatOptions, value); }
        }

        public ListItem SelectedVideoFormat
        {
            get { return _selectedVideoFormat; }
            set
            {
                SetProperty(ref _selectedVideoFormat, value);

                IsHdrEnabled = IsHdrSupported(_selectedVideoFormat, _primaryVideoStream);
                IsHdrChecked = _isHdrEnabled;
                QualityStepCount = (_selectedVideoFormat != null) && (_tricycleConfig.Video?.Codecs != null) &&
                    _tricycleConfig.Video.Codecs.TryGetValue((VideoFormat)_selectedVideoFormat.Value, out var codec)
                    ? codec.QualitySteps
                    : DEFAULT_STEP_COUNT;
            }
        }

        public double QualityMin
        {
            get { return _qualityMin; }
            set { SetProperty(ref _qualityMin, value); }
        }

        public double QualityMax
        {
            get { return _qualityMax; }
            set { SetProperty(ref _qualityMax, value); }
        }

        public double Quality
        {
            get { return _quality; }
            set { SetProperty(ref _quality, value); }
        }

        public int QualityStepCount
        {
            get { return _qualityStepCount; }
            set { SetProperty(ref _qualityStepCount, value); }
        }

        public bool IsHdrEnabled
        {
            get { return _isHdrEnabled && _isVideoConfigEnabled; }
            set { SetProperty(ref _isHdrEnabled, value); }
        }

        public bool IsHdrChecked
        {
            get { return _isHdrChecked; }
            set { SetProperty(ref _isHdrChecked, value); }
        }

        public IList<ListItem> SizeOptions
        {
            get { return _sizeOptions; }
            set { SetProperty(ref _sizeOptions, value); }
        }

        public ListItem SelectedSize
        {
            get { return _selectedSize; }
            set { SetProperty(ref _selectedSize, value); }
        }

        public IList<ListItem> CropOptions
        {
            get { return _cropOptions; }
            set { SetProperty(ref _cropOptions, value); }
        }

        public ListItem SelectedCropOption
        {
            get { return _selectedCropOption; }
            set
            {
                SetProperty(ref _selectedCropOption, value);

                IsManualCropControlVisible = object.Equals(_selectedCropOption?.Value, CropOption.Manual);
                IsAutoCropControlVisible = !IsManualCropControlVisible;
            }
        }

        public bool IsAutoCropControlVisible
        {
            get { return _isAutoCropControlVisible; }
            set { SetProperty(ref _isAutoCropControlVisible, value); }
        }

        public bool IsManualCropControlVisible
        {
            get { return _isManualCropControlVisible; }
            set { SetProperty(ref _isManualCropControlVisible, value); }
        }

        public bool IsAutocropEnabled
        {
            get { return _isAutocropEnabled && _isVideoConfigEnabled; }
            set { SetProperty(ref _isAutocropEnabled, value); }
        }

        public bool IsAutocropChecked
        {
            get { return _isAutocropChecked; }
            set
            {
                SetProperty(ref _isAutocropChecked, value);

                PopulateAspectRatioOptions(_primaryVideoStream, _croppedDimensions, _isAutocropChecked);
            }
        }

        public IList<ListItem> AspectRatioOptions
        {
            get { return _aspectRatioOptions; }
            set { SetProperty(ref _aspectRatioOptions, value); }
        }

        public ListItem SelectedAspectRatio
        {
            get { return _selectedAspectRatio; }
            set { SetProperty(ref _selectedAspectRatio, value); }
        }

        public string CropTop
        {
            get { return _cropTop; }
            set { SetProperty(ref _cropTop, value); }
        }

        public string CropBottom
        {
            get { return _cropBottom; }
            set { SetProperty(ref _cropBottom, value); }
        }

        public string CropLeft
        {
            get { return _cropLeft; }
            set { SetProperty(ref _cropLeft, value); }
        }

        public string CropRight
        {
            get { return _cropRight; }
            set { SetProperty(ref _cropRight, value); }
        }

        public bool IsDenoiseChecked
        {
            get { return _isDenoiseChecked; }
            set { SetProperty(ref _isDenoiseChecked, value); }
        }

        public IList<ListItem> SubtitleOptions
        {
            get { return _subtitleOptions; }
            set { SetProperty(ref _subtitleOptions, value); }
        }

        public ListItem SelectedSubtitle
        {
            get { return _selectedSubtitle; }
            set
            {
                SetProperty(ref _selectedSubtitle, value);

                IsForcedSubtitlesEnabled = value != NONE_OPTION;
            }
        }

        public bool IsForcedSubtitlesEnabled
        {
            get { return _isForcedSubtitlesEnabled && _isVideoConfigEnabled; }
            set { SetProperty(ref _isForcedSubtitlesEnabled, value); }
        }

        public bool IsForcedSubtitlesChecked
        {
            get { return _isForcedSubtitlesChecked; }
            set { SetProperty(ref _isForcedSubtitlesChecked, value); }
        }

        public IList<AudioOutputViewModel> AudioOutputs
        {
            get { return _audioOutputs; }
            set { SetProperty(ref _audioOutputs, value); }
        }

        public bool IsContainerFormatEnabled
        {
            get { return _isContainerFormatEnabled; }
            set { SetProperty(ref _isContainerFormatEnabled, value); }
        }

        public IList<ListItem> ContainerFormatOptions
        {
            get { return _containerFormatOptions; }
            set { SetProperty(ref _containerFormatOptions, value); }
        }

        public ListItem SelectedContainerFormat
        {
            get { return _selectedContainerFormat; }
            set
            {
                SetProperty(ref _selectedContainerFormat, value);

                _defaultExtension = GetDefaultExtension((ContainerFormat)value.Value);

                if (!string.IsNullOrWhiteSpace(DestinationName))
                {
                    DestinationName = Path.ChangeExtension(DestinationName, _defaultExtension);
                }
            }
        }

        public string DestinationName
        {
            get { return _destinationName; }
            set { SetProperty(ref _destinationName, value); }
        }

        public bool IsSpinnerVisible
        {
            get { return _isSpinnerVisible; }
            set { SetProperty(ref _isSpinnerVisible, value); }
        }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        public string StartImageSource
        {
            get { return _startImageSource; }
            set { SetProperty(ref _startImageSource, value); }
        }

        public ICommand SourceSelectCommand { get; }

        public ICommand DestinationSelectCommand { get; }

        public ICommand PreviewCommand { get; }

        public ICommand StartCommand { get; }

        #endregion

        #region Methods

        #region Public

        public TranscodeJob GetTranscodeJob()
        {
            return _isStartEnabled ? CreateJob() : null;
        }

        #endregion

        #region Command Actions

        async Task SelectSource()
        {
            var result = await _fileBrowser.BrowseToOpen();

            if (result.Confirmed)
            {
                _appManager.RaiseFileOpened(result.FileName);
            }
        }

        async Task SelectDestination()
        {
            string directory = _defaultDestinationDirectory;
            string fileName = null;

            if (!string.IsNullOrWhiteSpace(DestinationName))
            {
                directory = Path.GetDirectoryName(DestinationName);
                fileName = Path.GetFileName(DestinationName);
            }

            var result = await _fileBrowser.BrowseToSave(directory, fileName);

            if (result.Confirmed)
            {
                DestinationName = result.FileName;
            }
        }

        void ToggleRunning()
        {
            if (_isRunning)
            {
                ConfirmStopTranscode();
            }
            else
            {
                StartTranscode();
            }
        }

        #endregion

        #region Helpers

        void ProcessConfig(TricycleConfig config)
        {
            _defaultExtension = GetDefaultExtension((ContainerFormat)SelectedContainerFormat.Value);

            ProcessVideoCodecs(config.Video?.Codecs);
            ProcessAudioCodecs(config.Audio?.Codecs);
        }

        void ProcessVideoCodecs(IDictionary<VideoFormat, VideoCodec> codecs)
        {
            var formatOptions = new List<ListItem>();
            int? qualitySteps = null;

            foreach (var codec in codecs ?? Enumerable.Empty<KeyValuePair<VideoFormat, VideoCodec>>())
            {
                VideoFormat format = codec.Key;
                string name = GetVideoFormatName(format);

                if (name == null)
                {
                    continue;
                }

                var formatOption = new ListItem(name, format);

                if (!formatOptions.Contains(formatOption))
                {
                    formatOptions.Add(formatOption);

                    if (!qualitySteps.HasValue)
                    {
                        qualitySteps = codec.Value.QualitySteps;
                    }
                }
            }

            VideoFormatOptions = formatOptions;
            SelectedVideoFormat = formatOptions.FirstOrDefault();
            QualityStepCount = qualitySteps ?? DEFAULT_STEP_COUNT;
        }

        string GetVideoFormatName(VideoFormat format)
        {
            switch (format)
            {
                case VideoFormat.Avc:
                    return "AVC";
                case VideoFormat.Hevc:
                    return "HEVC";
                default:
                    return null;
            }
        }

        IList<ListItem> GetContainerFormatOptions()
        {
            return Enum.GetValues(typeof(ContainerFormat)).Cast<ContainerFormat>().Select(f =>
            {
                switch (f)
                {
                    case ContainerFormat.Mkv:
                        return new ListItem("MKV", f);
                    case ContainerFormat.Mp4:
                        return new ListItem("MP4", f);
                    default:
                        return new ListItem(string.Empty);
                }
            }).ToArray();
        }

        IList<ListItem> GetCropOptions()
        {
            return Enum.GetValues(typeof(CropOption)).Cast<CropOption>().Select(o => new ListItem(o)).ToArray();
        }

        void ProcessAudioCodecs(IDictionary<AudioFormat, AudioCodec> codecs)
        {
            _audioFormatOptions = new List<ListItem>();
            _audioMixdownOptionsByFormat = new Dictionary<AudioFormat, IList<ListItem>>();

            if (codecs?.Any() != true)
            {
                return;
            }

            foreach (var codec in codecs)
            {
                AudioFormat format = codec.Key;
                string name = AudioUtility.GetFormatName(format);

                if (name == null)
                {
                    continue;
                }

                var formatOption = new ListItem(name, format);
                IList<ListItem> mixdownOptions = GetAudioMixdownOptions(codec.Value.Presets);

                if (!_audioFormatOptions.Contains(formatOption) && (mixdownOptions?.Any() == true))
                {
                    _audioFormatOptions.Add(formatOption);
                    _audioMixdownOptionsByFormat[format] = mixdownOptions;
                }
            }
        }

        IList<ListItem> GetAudioMixdownOptions(IList<AudioPreset> presets)
        {
            return presets?.Select(p =>
            {
                string name = AudioUtility.GetMixdownName(p.Mixdown);

                return string.IsNullOrEmpty(name) ? new ListItem(string.Empty) : new ListItem(name, p.Mixdown);

            }).OrderByDescending(p => p.Name).ToArray();
        }

        VideoStreamInfo GetPrimaryVideoStream(IList<StreamInfo> streams)
        {
            return streams?.OfType<VideoStreamInfo>()
                           .FirstOrDefault();
        }

        async Task OpenSource(string fileName)
        {
            Command startCommand = (Command)StartCommand;
            Command previewCommand = (Command)PreviewCommand;

            _isStartEnabled = false;
            IsSpinnerVisible = true;
            Status = "Scanning source...";

            _appManager.RaiseBusy();
            EnableControls(false);
            startCommand.ChangeCanExecute();
            previewCommand.ChangeCanExecute();

            SourceName = fileName;
            _sourceInfo = await _mediaInspector.Inspect(fileName);

            if (_sourceInfo != null)
            {
                _primaryVideoStream = GetPrimaryVideoStream(_sourceInfo.Streams);
            }
            else
            {
                _primaryVideoStream = null;
            }

            bool isValid = false;

            if (_primaryVideoStream != null)
            {
                _cropParameters = await _cropDetector.Detect(_sourceInfo);
                _croppedDimensions = GetCroppedDimensions(_primaryVideoStream.Dimensions,
                                                          _primaryVideoStream.StorageDimensions,
                                                          _cropParameters);
                _isInterlaced = await _interlaceDetector.Detect(_sourceInfo);

                ProcessConfig(_tricycleConfig);
                IsContainerFormatEnabled = true;
                DestinationName = GetDefaultDestinationName(_sourceInfo, _defaultExtension);
                isValid = true;
            }
            else
            {
                _sourceInfo = null;
                _cropParameters = null;
                _croppedDimensions = new Dimensions();
                _isInterlaced = false;
                IsContainerFormatEnabled = false;
                DestinationName = null;
            }

            DisplaySourceInfo(_sourceInfo, _primaryVideoStream, _isInterlaced);
            PopulateVideoOptions(_primaryVideoStream, _croppedDimensions);
            PopulateSubtitleOptions(_sourceInfo);
            IsVideoConfigEnabled = _sourceInfo != null;
            PopulateAudioOptions(_sourceInfo);
            UpdateManualCropCoordinates(_primaryVideoStream?.Dimensions,
                                        _primaryVideoStream?.StorageDimensions,
                                        _cropParameters);

            IsSpinnerVisible = false;
            Status = string.Empty;

            if (isValid)
            {
                EnableControls(true);

                _isStartEnabled = _videoFormatOptions?.Any() == true;
            }
            else
            {
                _appManager.Alert("Invalid Source", "The selected file could not be opened.", Severity.Warning);

                _isSourceSelectionEnabled = true;
                _isStartEnabled = false;

                ((Command)SourceSelectCommand).ChangeCanExecute();
            }

            startCommand.ChangeCanExecute();
            previewCommand.ChangeCanExecute();
            _appManager.RaiseReady();
            _appManager.RaiseSourceSelected(isValid);
        }

        void DisplaySourceInfo(MediaInfo sourceInfo, VideoStreamInfo videoStream, bool isInterlaced)
        {
            if (sourceInfo != null)
            {
                TimeSpan duration = _sourceInfo.Duration;

                SourceDuration = string.Format("{0:00}:{1:00}:{2:00}",
                    duration.Hours, duration.Minutes, duration.Seconds);
                SourceSize = GetSizeName(videoStream.Dimensions, isInterlaced);
                IsSourceHdr = videoStream.DynamicRange == DynamicRange.High;
                IsSourceInfoVisible = true;
            }
            else
            {
                IsSourceInfoVisible = false;
            }
        }

        string GetSizeName(Dimensions dimensions, bool isInterlaced)
        {
            if ((dimensions.Width >= 3840) || (dimensions.Height >= 2160))
            {
                return "4K";
            }

            string suffix = isInterlaced ? "i" : "p";

            if ((dimensions.Width >= 1920) || (dimensions.Height >= 1080))
            {
                return $"1080{suffix}";
            }

            if ((dimensions.Width >= 1280) || (dimensions.Height >= 720))
            {
                return $"720{suffix}";
            }

            if ((dimensions.Width >= 853) || (dimensions.Height >= 480))
            {
                return $"480{suffix}";
            }

            return $"{dimensions.Height}{suffix}";
        }

        void PopulateVideoOptions(VideoStreamInfo videoStream, Dimensions croppedDimensions)
        {
            if (videoStream != null)
            {
                IsHdrEnabled = videoStream.DynamicRange == DynamicRange.High;
                IsHdrChecked = _isHdrEnabled;
                SizeOptions = GetSizeOptions(videoStream.Dimensions);
                SelectedSize = SizeOptions?.FirstOrDefault();
                IsAutocropEnabled = HasBars(videoStream.Dimensions, croppedDimensions);
                IsAutocropChecked = _isAutocropEnabled;

                if (IsHdrChecked)
                {
                    SelectedVideoFormat =
                        _videoFormatOptions?.FirstOrDefault(f => VideoUtility.SupportsHdr((VideoFormat)f.Value));
                }
            }
            else
            {
                IsHdrEnabled = false;
                IsHdrChecked = false;
                SizeOptions = null;
                SelectedSize = null;
                IsAutocropEnabled = false;
                IsAutocropChecked = false;
            }

            PopulateAspectRatioOptions(videoStream, croppedDimensions, IsAutocropChecked);
        }

        bool IsHdrSupported(ListItem selectedFormat, VideoStreamInfo videoStream)
        {
            return (selectedFormat != null) &&
                VideoUtility.SupportsHdr((VideoFormat)selectedFormat.Value) &&
                object.Equals(videoStream?.DynamicRange, DynamicRange.High);
        }

        IList<ListItem> GetSizeOptions(Dimensions sourceDimensions)
        {
            IList<ListItem> result =
                _tricycleConfig.Video?.SizePresets?.Where(s => s.Value.Height <= sourceDimensions.Height ||
                                                               s.Value.Width <= sourceDimensions.Width)
                                                   .OrderByDescending(s => s.Value.Width * s.Value.Height)
                                                   .Select(s => new ListItem(s.Key, s.Value))
                                                   .ToList() ?? new List<ListItem>();

            result.Insert(0, ORIGINAL_OPTION);

            return result;
        }

        void PopulateAspectRatioOptions(VideoStreamInfo videoStream, Dimensions croppedDimensions, bool autoCrop)
        {
            Dimensions? sourceDimensions = videoStream?.Dimensions;

            if (sourceDimensions.HasValue)
            {
                AspectRatioOptions = GetAspectRatioOptions(autoCrop ? croppedDimensions : sourceDimensions.Value);
                SelectedAspectRatio = AspectRatioOptions?.FirstOrDefault();
            }
            else
            {
                AspectRatioOptions = null;
                SelectedAspectRatio = null;
            }
        }

        IList<ListItem> GetAspectRatioOptions(Dimensions dimensions)
        {
            double aspectRatio = VideoUtility.GetAspectRatio(dimensions);

            IList<ListItem> result =
                _tricycleConfig.Video?.AspectRatioPresets?.Where(p => VideoUtility.GetAspectRatio(p.Value) <= aspectRatio)
                                                          .OrderByDescending(p => VideoUtility.GetAspectRatio(p.Value))
                                                          .Select(s => new ListItem(s.Key, s.Value))
                                                          .ToList() ?? new List<ListItem>();

            result.Insert(0, ORIGINAL_OPTION);

            return result;
        }

        bool HasBars(Dimensions dimensions, Dimensions croppedDimensions)
        {
            return !dimensions.Equals(croppedDimensions);
        }

        Dimensions GetCroppedDimensions(Dimensions dimensions, Dimensions storageDimensions, CropParameters cropParameters)
        {
            if (cropParameters == null)
            {
                return dimensions;
            }

            var sampleAspectRatio = VideoUtility.GetSampleAspectRatio(dimensions, storageDimensions);
            var width = (int)Math.Round(cropParameters.Size.Width * sampleAspectRatio);

            return new Dimensions(width, cropParameters.Size.Height);
        }

        void UpdateManualCropCoordinates(Dimensions? sourceDimensions,
                                         Dimensions? storageDimensions,
                                         CropParameters cropParameters)
        {
            if (sourceDimensions.HasValue && storageDimensions.HasValue && (cropParameters != null))
            {
                var sampleAspectRatio = VideoUtility.GetSampleAspectRatio(sourceDimensions.Value,
                                                                          storageDimensions.Value);

                CropTop = cropParameters.Start.Y.ToString();
                CropBottom = (sourceDimensions.Value.Height
                              - cropParameters.Size.Height
                              - cropParameters.Start.Y).ToString();
                CropLeft =  (cropParameters.Start.X * sampleAspectRatio).ToString("0");
                CropRight = ((sourceDimensions.Value.Width - cropParameters.Size.Width - cropParameters.Start.X)
                             * sampleAspectRatio).ToString("0");
            }
            else
            {
                CropTop = "0";
                CropBottom = "0";
                CropLeft = "0";
                CropRight = "0";
            }
        }

        void PopulateSubtitleOptions(MediaInfo sourceInfo)
        {
            SubtitleOptions = GetSubtitleOptions(sourceInfo);
            SelectedSubtitle = SubtitleOptions?.FirstOrDefault();
            IsForcedSubtitlesChecked = SubtitleOptions?.Count() > 1 && _tricycleConfig.ForcedSubtitlesOnly;
        }

        IList<ListItem> GetSubtitleOptions(MediaInfo sourceInfo)
        {
            int i = 1;

            IList<ListItem> result = sourceInfo?.Streams?.Where(s => s.StreamType == StreamType.Subtitle)
                                                         .Select(s => new ListItem(GetSubtitleName(s, i++), s))
                                                         .ToList() ?? new List<ListItem>();

            result.Insert(0, NONE_OPTION);

            return result;
        }

        string GetSubtitleName(StreamInfo stream, int index)
        {
            if (string.IsNullOrWhiteSpace(stream.Language))
            {
                return index.ToString();
            }

            return $"{index}: {Language.FromPart2(stream.Language)?.Name ?? stream.Language}";
        }

        void PopulateAudioOptions(MediaInfo sourceInfo)
        {
            ClearAudioOutputs();

            if (_audioFormatOptions?.Any() != true)
            {
                return;
            }

            _audioTrackOptions = GetAudioTrackOptions(sourceInfo?.Streams);

            if (_audioTrackOptions.Count < 2) //only none
            {
                return;
            }

            var audioOutput = CreateAudioOutput();

            AudioOutputs.Add(audioOutput);

            if (_audioTrackOptions.Count > 1)
            {
                //select the first audio track (that is not none)
                audioOutput.SelectedTrack = _audioTrackOptions[1];
            }
        }

        void ClearAudioOutputs()
        {
            for (int i = AudioOutputs.Count - 1; i >= 0; i--)
            {
                var audioOutput = AudioOutputs[i];

                UnsubscribeFromAudioOutputEvents(audioOutput);

                AudioOutputs.RemoveAt(i);
            }
        }

        IList<ListItem> GetAudioTrackOptions(IList<StreamInfo> sourceStreams)
        {
            int index = 1;
            IList<ListItem> result = sourceStreams?.OfType<AudioStreamInfo>()
                                                   .Where(s => IsAudioTrackSupported(s))
                                                   .Select(s => new ListItem(GetAudioTrackName(s, index++), s))
                                                   .ToList() ?? new List<ListItem>();

            result.Insert(0, NONE_OPTION);

            return result;
        }

        bool IsAudioTrackSupported(AudioStreamInfo audioStream)
        {
            return !string.IsNullOrWhiteSpace(audioStream.FormatName) &&
                (audioStream.ChannelCount > 0) &&
                GetAudioFormatOptions(audioStream).Any();
        }

        string GetAudioTrackName(AudioStreamInfo audioStream, int index)
        {
            string mixdown = "Mono";

            if (audioStream.ChannelCount > 7)
            {
                mixdown = "7.1";
            }
            else if (audioStream.ChannelCount > 5)
            {
                mixdown = "5.1";
            }
            else if (audioStream.ChannelCount > 1)
            {
                mixdown = "Stereo";
            }

            string format = audioStream.FormatName;

            if (audioStream.Format.HasValue)
            {
                format = AudioUtility.GetFormatName(audioStream.Format.Value);
            }
            else if (Regex.IsMatch(audioStream.FormatName, @"truehd", RegexOptions.IgnoreCase))
            {
                format = "Dolby TrueHD";
            }
            else if (!string.IsNullOrWhiteSpace(audioStream.ProfileName))
            {
                if (Regex.IsMatch(audioStream.ProfileName, format, RegexOptions.IgnoreCase))
                {
                    format = audioStream.ProfileName;
                }
                else
                {
                    format += $" {audioStream.ProfileName}";
                }
            }

            string result = $"{index}: {format} {mixdown}";

            if (!string.IsNullOrWhiteSpace(audioStream.Language))
            {
                result += $" ({Language.FromPart2(audioStream.Language)?.Name ?? audioStream.Language})";
            }

            return result;
        }

        string GetDefaultExtension(ContainerFormat format)
        {
            string result = DEFAULT_EXTENSION;

            if ((_tricycleConfig.DefaultFileExtensions != null) &&
                _tricycleConfig.DefaultFileExtensions.TryGetValue(format, out var extension))
            {
                result = extension;
            }

            return result;
        }

        string GetDefaultDestinationName(MediaInfo sourceInfo, string extension)
        {
            string fileName = "movie";

            if (!string.IsNullOrWhiteSpace(sourceInfo.FileName))
            {
                fileName = Path.GetFileNameWithoutExtension(sourceInfo.FileName);
            }

            string result;
            int count = 1;

            do
            {
                string number = count > 1 ? $" {count}" : string.Empty;
                result = Path.Combine(_defaultDestinationDirectory,
                                     $"{fileName}{number}.{extension}");
                count++;
            } while (_fileSystem.File.Exists(result));

            return result;
        }

        AudioOutputViewModel CreateAudioOutput()
        {
            var result = new AudioOutputViewModel()
            {
                TrackOptions = _audioTrackOptions,
                SelectedTrack = _audioTrackOptions.FirstOrDefault(),
            };

            result.TrackSelected += OnAudioTrackSelected;
            result.FormatSelected += OnAudioFormatSelected;

            return result;
        }

        IEnumerable<ListItem> GetAudioFormatOptions(AudioStreamInfo stream)
        {
            return _audioFormatOptions?.Where(f =>
                GetAudioMixdownOptions(stream, (AudioFormat)f.Value).Any())
                ?? Enumerable.Empty<ListItem>();
        }

        IEnumerable<ListItem> GetAudioMixdownOptions(AudioStreamInfo stream, AudioFormat format)
        {
            IEnumerable<ListItem> result = null;

            if (_audioMixdownOptionsByFormat.TryGetValue(format, out var allOptions))
            {
                result = allOptions.Where(o => AudioUtility.GetChannelCount((AudioMixdown)o.Value) <= stream.ChannelCount);
            }

            return result ?? Enumerable.Empty<ListItem>();
        }

        void UnsubscribeFromAudioOutputEvents(AudioOutputViewModel audioOutput)
        {
            audioOutput.FormatSelected -= OnAudioFormatSelected;
            audioOutput.TrackSelected -= OnAudioTrackSelected;
        }

        void StartTranscode()
        {
            IsSpinnerVisible = true;
            Status = "Transcoding...";

            var job = CreateJob();
            bool success = false;

            try
            {
                _mediaTranscoder.Start(job);

                _isRunning = true;
                StartImageSource = STOP_IMAGE;

                _appManager.RaiseBusy();
                EnableControls(false);
                ((Command)PreviewCommand).ChangeCanExecute();

                success = true;
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (InvalidOperationException) { }

            if (!success)
            {
                _appManager.Alert("Job Error",
                                  @"Oops! Your job couldn't be started for some reason. ¯\_(ツ)_/¯",
                                  Severity.Warning);
                IsSpinnerVisible = false;
                Status = string.Empty;
            }
        }

        bool ConfirmStopTranscode()
        {
            if (_appManager.Confirm("Stop Transcode", "Whoa... Are you sure you want to stop and lose your progress?"))
            {
                StopTranscode();
                return true;
            }

            return false;
        }

        void StopTranscode()
        {
            bool success = false;

            try
            {
                _mediaTranscoder.Stop();
                ResetJobState();
                DeleteDestination();

                success = true;
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (InvalidOperationException) { }

            if (!success)
            {
                _appManager.Alert("Job Error",
                                  @"Oops! Your job couldn't be stopped for some reason. ¯\_(ツ)_/¯",
                                  Severity.Warning);
            }
        }

        void ResetJobState()
        {
            _isRunning = false;
            StartImageSource = PLAY_IMAGE;

            ResetProgress();
            EnableControls(true);
            ((Command)PreviewCommand).ChangeCanExecute();
            _appManager.RaiseReady();
        }

        void ResetProgress()
        {
            IsSpinnerVisible = false;
            Progress = 0;
            Status = string.Empty;
        }

        void DeleteDestination()
        {
            if (!_tricycleConfig.DeleteIncompleteFiles)
            {
                return;
            }

            try
            {
                if (_fileSystem.File.Exists(DestinationName))
                {
                    _fileSystem.File.Delete(DestinationName);
                }
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        TranscodeJob CreateJob()
        {
            var format = (ContainerFormat)SelectedContainerFormat.Value;

            return new TranscodeJob()
            {
                SourceInfo = _sourceInfo,
                OutputFileName = DestinationName,
                Format = format,
                Streams = GetOutputStreams(),
                Subtitles = GetSubtitles(),
                Metadata = new Dictionary<string, string>()
                {
                    {
                        format == ContainerFormat.Mkv ? "encoder" : "encoding_tool",
                        $"{AppState.AppName} {AppState.AppVersion}"
                    }
                }
            };
        }

        IList<OutputStream> GetOutputStreams()
        {
            var result = GetAudioOutputStreams();
            
            result.Insert(0, GetVideoOutput());

            return result;
        }

        VideoOutputStream GetVideoOutput()
        {
            var format = (VideoFormat)SelectedVideoFormat.Value;
            int divisor = 8;

            if (_tricycleConfig.Video?.SizeDivisor > 0)
            {
                divisor = _tricycleConfig.Video.SizeDivisor;
            }

            CropParameters cropParameters = GetJobCropParameters(divisor);

            return new VideoOutputStream()
            {
                SourceStreamIndex = _primaryVideoStream.Index,
                Format = format,
                Quality = CalculateQuality(format, (decimal)Quality),
                CropParameters = cropParameters,
                ScaledDimensions = GetScaledDimensions(cropParameters, divisor),
                DynamicRange = IsHdrChecked ? DynamicRange.High : DynamicRange.Standard,
                CopyHdrMetadata = IsHdrChecked,
                Deinterlace = GetDeinterlaceFlag(_isInterlaced),
                Denoise = IsDenoiseChecked,
                Tonemap = IsSourceHdr && !IsHdrChecked
            };
        }

        decimal CalculateQuality(VideoFormat format, decimal quality)
        {
            decimal result = 20;

            if ((_tricycleConfig.Video?.Codecs != null) && _tricycleConfig.Video.Codecs.TryGetValue(format, out var codec))
            {
                decimal min = codec.QualityRange.Min ?? 22;
                decimal max = codec.QualityRange.Max ?? 18;

                result = (max - min) * quality + min;
            }

            return result;
        }

        CropParameters GetJobCropParameters(int divisor)
        {
            double? aspectRatio = null;
            CropParameters cropParameters = null;

            if (object.Equals(SelectedCropOption?.Value, CropOption.Manual))
            {
                var sampleAspectRatio = VideoUtility.GetSampleAspectRatio(_primaryVideoStream.Dimensions,
                                                                          _primaryVideoStream.StorageDimensions);
                int value;
                var top = int.TryParse(CropTop, out value) ? value : 0;
                var bottom = int.TryParse(CropBottom, out value) ? value : 0;
                var left = int.TryParse(CropLeft, out value) ? (int)Math.Round(value / sampleAspectRatio) : 0;
                var right = int.TryParse(CropRight, out value) ? (int)Math.Round(value / sampleAspectRatio) : 0;

                cropParameters = new CropParameters()
                {
                    Start = new Coordinate<int>(left, top),
                    Size = new Dimensions(_primaryVideoStream.StorageDimensions.Width - left - right,
                                          _primaryVideoStream.StorageDimensions.Height - top - bottom)
                };
            }
            else
            {
                if (SelectedAspectRatio != ORIGINAL_OPTION)
                {
                    aspectRatio = VideoUtility.GetAspectRatio((Dimensions)SelectedAspectRatio.Value);
                }

                if (IsAutocropChecked)
                {
                    cropParameters = _cropParameters;
                }
            }

            if (!aspectRatio.HasValue && (cropParameters == null))
            {
                return null;
            }

            return _transcodeCalculator.CalculateCropParameters(_primaryVideoStream.Dimensions,
                                                                _primaryVideoStream.StorageDimensions,
                                                                cropParameters,
                                                                aspectRatio,
                                                                divisor);
        }

        Dimensions? GetScaledDimensions(CropParameters cropParameters, int divisor)
        {
            if ((SelectedSize == ORIGINAL_OPTION) &&
                _primaryVideoStream.Dimensions.Equals(_primaryVideoStream.StorageDimensions))
            {
                return null;
            }

            var sourceDimensions = _primaryVideoStream.Dimensions;

            if (cropParameters != null)
            {
                var sampleAspectRatio =
                    VideoUtility.GetSampleAspectRatio(_primaryVideoStream.Dimensions, _primaryVideoStream.StorageDimensions);
                var width = (int)Math.Round(cropParameters.Size.Width * sampleAspectRatio);

                sourceDimensions = new Dimensions(width, cropParameters.Size.Height);
            }

            var targetDimensions = SelectedSize == ORIGINAL_OPTION ? sourceDimensions : (Dimensions)SelectedSize.Value;

            return _transcodeCalculator.CalculateScaledDimensions(sourceDimensions, targetDimensions, divisor);
        }

        SubtitlesConfig GetSubtitles()
        {
            if (SelectedSubtitle == NONE_OPTION)
            {
                return null;
            }

            return new SubtitlesConfig()
            {
                SourceStreamIndex = ((StreamInfo)SelectedSubtitle?.Value).Index,
                ForcedOnly = IsForcedSubtitlesChecked
            };
        }

        bool GetDeinterlaceFlag(bool isInterlaced)
        {
            switch (_tricycleConfig.Video?.Deinterlace)
            {
                case SmartSwitchOption.Off:
                    return false;
                case SmartSwitchOption.On:
                    return true;
                case SmartSwitchOption.Auto:
                default:
                    return isInterlaced;
            }
        }

        IList<OutputStream> GetAudioOutputStreams()
        {
            IList<OutputStream> result = new List<OutputStream>();

            if (AudioOutputs?.Any() != true)
            {
                return result;
            }

            foreach (var viewModel in AudioOutputs)
            {
                AudioStreamInfo sourceStream = GetStream(viewModel);

                if (sourceStream == null)
                {
                    continue;
                }

                var output = Map(viewModel);

                if ((_tricycleConfig.Audio?.PassthruMatchingTracks == true) &&
                    Match(sourceStream, output))
                {
                    result.Add(new OutputStream()
                    {
                        SourceStreamIndex = output.SourceStreamIndex
                    });
                }
                else
                {
                    result.Add(output);
                }
            }

            return result;
        }

        AudioStreamInfo GetStream(AudioOutputViewModel viewModel)
        {
            return viewModel.SelectedTrack.Value as AudioStreamInfo;
        }

        AudioOutputStream Map(AudioOutputViewModel viewModel)
        {
            var result = new AudioOutputStream()
            {
                SourceStreamIndex = GetStream(viewModel).Index,
                Format = (AudioFormat)viewModel.SelectedFormat.Value,
                Mixdown = (AudioMixdown)viewModel.SelectedMixdown.Value,
                Metadata = new Dictionary<string, string>()
                {
                    { "title", viewModel.SelectedMixdown.ToString() }
                }
            };

            AudioCodec codec = null;

            _tricycleConfig.Audio?.Codecs?.TryGetValue(result.Format, out codec);

            AudioPreset preset = codec?.Presets.FirstOrDefault(p => p.Mixdown == result.Mixdown);

            if (preset != null)
            {
                result.Quality = preset.Quality;
            }

            return result;
        }

        bool Match(AudioStreamInfo sourceStream, AudioOutputStream outputStream)
        {
            return outputStream.Format == sourceStream.Format &&
                AudioUtility.GetChannelCount(outputStream.Mixdown ?? AudioMixdown.Mono) == sourceStream.ChannelCount;
        }

        void SaveTemplate(string name)
        {
            var template = new JobTemplate()
            {
                Format = (ContainerFormat)SelectedContainerFormat.Value,
                Video = GetVideoTemplate(),
                AudioTracks = GetAudioTemplates()
            };

            if (SelectedSubtitle != NONE_OPTION)
            {
                var subtitleStream = (StreamInfo)SelectedSubtitle.Value;

                template.Subtitles = new SubtitleTemplate()
                {
                    ForcedOnly = IsForcedSubtitlesChecked,
                    Language = _sourceInfo.Streams.FirstOrDefault(s => s.Index == subtitleStream.Index)?.Language
                };
            }

            var templates = new Dictionary<string, JobTemplate>(_templateManager.Config); // clone the config

            templates[name] = template;

            _templateManager.Config = templates;
            _templateManager.Save();
        }

        VideoTemplate GetVideoTemplate()
        {
            var format = (VideoFormat)SelectedVideoFormat.Value;

            return new VideoTemplate()
            {
                AspectRatioPreset = SelectedAspectRatio != ORIGINAL_OPTION ? SelectedAspectRatio.ToString() : null,
                CropBars = IsAutocropChecked,
                Denoise = IsDenoiseChecked,
                Format = format,
                Hdr = IsHdrChecked,
                ManualCrop = object.Equals(SelectedCropOption?.Value, CropOption.Manual)
                             ? new ManualCropTemplate()
                             {
                                 Top = int.TryParse(CropTop, out var top) ? top : 0,
                                 Bottom = int.TryParse(CropBottom, out var bottom) ? bottom : 0,
                                 Left = int.TryParse(CropLeft, out var left) ? left : 0,
                                 Right = int.TryParse(CropRight, out var right) ? right : 0
                             }
                             : null,
                Quality = CalculateQuality(format, (decimal)Quality),
                SizePreset = SelectedSize != ORIGINAL_OPTION ? SelectedSize?.ToString() : null
            };
        }

        IList<AudioTemplate> GetAudioTemplates()
        {
            IList<AudioTemplate> result = new List<AudioTemplate>();

            if (AudioOutputs?.Any() != true)
            {
                return result;
            }

            var audioStreamsByLanguage = _sourceInfo.Streams.Where(s => s.StreamType == StreamType.Audio)
                                                            .GroupBy(s => s.Language)
                                                            .ToDictionary(g => g.Key);

            foreach (var viewModel in AudioOutputs)
            {
                AudioStreamInfo sourceStream = GetStream(viewModel);

                if (sourceStream == null)
                {
                    continue;
                }

                int i = 0;
                var template = new AudioTemplate()
                {
                    Format = (AudioFormat)viewModel.SelectedFormat.Value,
                    Language = sourceStream.Language,
                    Mixdown = (AudioMixdown)viewModel.SelectedMixdown.Value,
                    RelativeIndex = audioStreamsByLanguage.GetValueOrDefault(sourceStream.Language)?
                                                          .OrderBy(s => s.Index)
                                                          .Select(s => new
                                                          {
                                                              RelativeIndex = i++,
                                                              StreamIndex = s.Index
                                                          })
                                                         .FirstOrDefault(s => s.StreamIndex == sourceStream.Index)?
                                                         .RelativeIndex ?? 0
                };

                result.Add(template);
            }

            return result;
        }

        void ApplyTemplate(string name)
        {
            var template = _templateManager.Config.GetValueOrDefault(name);

            if (template == null)
            {
                return;
            }

            SelectedContainerFormat = ContainerFormatOptions?.FirstOrDefault(f => object.Equals(f.Value, template.Format));

            if (template.Video != null)
            {
                ApplyTemplate(template.Video);
            }

            if (template.Subtitles != null)
            {
                var subtitles = template.Subtitles;

                SelectedSubtitle = string.IsNullOrEmpty(subtitles.Language)
                                   ? NONE_OPTION
                                   : SubtitleOptions?.Where(s => s != NONE_OPTION)
                                                     .FirstOrDefault(s =>
                                                        (s.Value as StreamInfo)?.Language == subtitles.Language)
                                   ?? NONE_OPTION;
                IsForcedSubtitlesChecked = subtitles.ForcedOnly;
            }

            ApplyTemplates(template.AudioTracks);
        }

        void ApplyTemplate(VideoTemplate video)
        {
            SelectedVideoFormat = VideoFormatOptions?.FirstOrDefault(f => object.Equals(f.Value, video.Format));
            Quality = (double)GetQualityPercent(video.Format, video.Quality);
            IsHdrChecked = video.Hdr && IsHdrSupported(SelectedVideoFormat, _primaryVideoStream);
            SelectedSize = GetClosestSize(video.SizePreset);

            if (video.ManualCrop != null)
            {
                var crop = video.ManualCrop;

                SelectedCropOption = new ListItem(CropOption.Manual);
                CropTop = crop.Top.ToString();
                CropBottom = crop.Bottom.ToString();
                CropLeft = crop.Left.ToString();
                CropRight = crop.Right.ToString();
            }
            else
            {
                SelectedCropOption = new ListItem(CropOption.Auto);
            }

            IsAutocropChecked = video.CropBars && HasBars(_primaryVideoStream.Dimensions, _croppedDimensions);
            SelectedAspectRatio = GetClosestAspectRatio(video.AspectRatioPreset);
            IsDenoiseChecked = video.Denoise;
        }

        decimal GetQualityPercent(VideoFormat format, decimal quality)
        {
            decimal result = 0.5M;

            if ((_tricycleConfig.Video?.Codecs != null) && _tricycleConfig.Video.Codecs.TryGetValue(format, out var codec))
            {
                decimal min = codec.QualityRange.Min ?? 22;
                decimal max = codec.QualityRange.Max ?? 18;

                result = (quality - min) /  (max - min);
            }

            return result;
        }

        void ApplyTemplates(IList<AudioTemplate> templates)
        {
            PopulateAudioOptions(_sourceInfo);

            if (AudioOutputs?.Any() != true)
            {
                return;
            }

            var output = AudioOutputs.First();

            output.SelectedTrack = NONE_OPTION;

            if (templates == null)
            {
                return;
            }

            var tracksByLanguage = output.TrackOptions.Where(t => t != NONE_OPTION)
                                                      .Where(t => ((StreamInfo)t.Value).StreamType == StreamType.Audio)
                                                      .GroupBy(t => ((StreamInfo)t.Value).Language)
                                                      .ToDictionary(g => g.Key);

            foreach (var template in templates)
            {
                if (string.IsNullOrEmpty(template?.Language))
                {
                    continue;
                }

                var tracks = tracksByLanguage.GetValueOrDefault(template.Language);

                if (tracks?.Any() != true)
                {
                    continue;
                }

                int i = 0;
                var track = tracks.OrderBy(t => ((StreamInfo)t.Value).Index)
                                  .Select(t => new
                                  {
                                    Track = t,
                                    RelativeIndex = i++
                                  })
                                  .Where(t => t.RelativeIndex <= template.RelativeIndex)
                                  .Select(t => t.Track)
                                  .LastOrDefault();

                if (track == null)
                {
                    continue;
                }

                var channelCount = AudioUtility.GetChannelCount(template.Mixdown);

                output = AudioOutputs[AudioOutputs.Count - 1];
                output.SelectedTrack = track;
                output.SelectedFormat = output.FormatOptions?.FirstOrDefault(f => object.Equals(f.Value, template.Format))
                                        ?? output.FormatOptions?.FirstOrDefault();
                output.SelectedMixdown = output.MixdownOptions?.Where(m =>
                                            AudioUtility.GetChannelCount((AudioMixdown)m.Value) <= channelCount)
                                                               .FirstOrDefault();
            }

            RemoveDuplicateAudioOutputs();
        }

        ListItem GetClosestSize(string presetName)
        {
            return GetClosestPreset(presetName, _tricycleConfig?.Video?.SizePresets, SizeOptions);
        }

        ListItem GetClosestAspectRatio(string presetName)
        {
            return GetClosestPreset(presetName, _tricycleConfig?.Video?.AspectRatioPresets, AspectRatioOptions);
        }

        ListItem GetClosestPreset(string presetName, IDictionary<string, Dimensions> presets, IList<ListItem> options)
        {
            if (string.IsNullOrEmpty(presetName))
            {
                return ORIGINAL_OPTION;
            }

            ListItem result = options?.FirstOrDefault(s => s.Name == presetName);

            if (result != null)
            {
                return result;
            }

            if (presets?.ContainsKey(presetName) != true)
            {
                return ORIGINAL_OPTION;
            }

            result = options.FirstOrDefault(s => s != ORIGINAL_OPTION);

            return result ?? ORIGINAL_OPTION;
        }

        void RemoveDuplicateAudioOutputs()
        {
            for (int i = 0; i < AudioOutputs.Count; i++)
            {
                var output1 = AudioOutputs[i];

                for (int j = AudioOutputs.Count - 1; j > i; j--)
                {
                    var output2 = AudioOutputs[j];

                    if (object.Equals(output1.SelectedTrack, output2.SelectedTrack) &&
                        object.Equals(output1.SelectedFormat, output2.SelectedFormat) &&
                        object.Equals(output1.SelectedMixdown, output2.SelectedMixdown))
                    {
                        output2.SelectedTrack = NONE_OPTION;
                    }
                }
            }
        }

        void EnableControls(bool isEnabled)
        {
            IsVideoConfigEnabled = isEnabled;
            IsContainerFormatEnabled = isEnabled;

            foreach (var audio in AudioOutputs ?? Enumerable.Empty<AudioOutputViewModel>())
            {
                audio.IsEnabled = isEnabled;
            }

            _isSourceSelectionEnabled = isEnabled;
            _isDestinationSelectionEnabled = isEnabled;

            ((Command)SourceSelectCommand).ChangeCanExecute();
            ((Command)DestinationSelectCommand).ChangeCanExecute();
        }

        string GetPercentFormat(double percent)
        {
            if (percent >= 1)
            {
                return "#.#";
            }

            return "0.##";
        }

        string GetSizeFormat(ByteSize size)
        {
            if (size.LargestWholeNumberValue >= 100)
            {
                return "#";
            }

            return "#.#";
        }

        string GetSpeedFormat(double speed)
        {
            if (speed >= 10)
            {
                return "#";
            }

            if (speed >= 1)
            {
                return "#.#";
            }

            return "0.##";
        }

        #endregion

        #region Event Handlers

        void OnAudioTrackSelected(object sender, ItemChangedEventArgs args)
        {
            if (sender is AudioOutputViewModel == false)
            {
                return;
            }

            var model = (AudioOutputViewModel)sender;

            if (((args.NewItem == null) || object.Equals(args.NewItem, NONE_OPTION)) &&
                (AudioOutputs.Count > 1))
            {
                UnsubscribeFromAudioOutputEvents(model);

                AudioOutputs.Remove(model);

                return;
            }

            if (((args.OldItem == null) || object.Equals(args.OldItem, NONE_OPTION)) &&
                !object.Equals(args.NewItem, NONE_OPTION))
            {
                _audioOutputs.Add(CreateAudioOutput());
            }  

            if ((args.NewItem != null) &&
                !args.NewItem.Equals(NONE_OPTION))
            {
                model.FormatOptions = GetAudioFormatOptions((AudioStreamInfo)args.NewItem.Value).ToArray();
                model.SelectedFormat = model.FormatOptions.FirstOrDefault();
            }
            else
            {
                model.FormatOptions = null;
            }
        }

        void OnAudioFormatSelected(object sender, ItemChangedEventArgs args)
        {
            if (sender is AudioOutputViewModel == false)
            {
                return;
            }

            var model = (AudioOutputViewModel)sender;
            IList<ListItem> mixdownOptions = null;

            if ((model.SelectedTrack != null) &&
                !model.SelectedTrack.Equals(NONE_OPTION) &&
                (args.NewItem != null))
            {
                var stream = (AudioStreamInfo)model.SelectedTrack.Value;
                var format = (AudioFormat)args.NewItem.Value;

                mixdownOptions = GetAudioMixdownOptions(stream, format).ToArray();
            }

            model.MixdownOptions = mixdownOptions;
            model.SelectedMixdown = mixdownOptions?.FirstOrDefault();
        }

        void OnTranscodeStatusChanged(TranscodeStatus status)
        {
            if (!_isRunning || (status == null))
            {
                return;
            }

            var builder = new StringBuilder("Transcoding... ");
            string speedFormat = GetSpeedFormat(status.Speed);

            builder.AppendFormat("{0,4}x", status.Speed.ToString(speedFormat));

            if (status.Eta > TimeSpan.Zero)
            {
                builder.AppendFormat(" | {0:00}:{1:00}:{2:00}",
                                     Math.Floor(status.Eta.TotalHours),
                                     status.Eta.Minutes,
                                     status.Eta.Seconds);
            }

            if ((status.Size > 0) && (status.EstimatedTotalSize > 0))
            {
                var byteSize = ByteSize.FromBytes(status.EstimatedTotalSize);
                string sizeFormat = GetSizeFormat(byteSize);

                builder.AppendFormat(" | {0,7}", $"~{byteSize.ToString(sizeFormat)}");
            }

            var percent = status.Percent * 100;
            string percentFormat = GetPercentFormat(percent);

            builder.AppendFormat(" | {0,4}%", percent.ToString(percentFormat));

            _device.BeginInvokeOnMainThread(() =>
            {
                if (!_isRunning)
                {
                    return;
                }

                Progress = status.Percent;
                Status = builder.ToString();
            });
        }

        void OnTranscodeCompleted()
        {
            _device.BeginInvokeOnMainThread(async () =>
            {
                if (_tricycleConfig.CompletionAlert)
                {
                    _appManager.Alert("Transcode Complete", "Good news! Your shiny, new video is ready.", Severity.Info);
                }

                ResetJobState();
                await OpenSource(SourceName);
            });
        }

        void OnTranscodeFailed(string error)
        {
            _device.BeginInvokeOnMainThread(async () =>
            {
                _appManager.Alert("Transcode Failed", error, Severity.Warning);
                DeleteDestination();
                ResetJobState();
                await OpenSource(SourceName);
            });
        }

        void OnAppQuitting()
        {
            if (IsPageVisible && (!_isRunning || ConfirmStopTranscode()))
            {
                // This raises the event outside of the current closing call stack
                _device.StartTimer(TimeSpan.FromTicks(1), () =>
                {
                    _appManager.RaiseQuitConfirmed();
                    return false;
                });
            }
        }

        async Task OnConfigChanged(TricycleConfig config)
        {
            _tricycleConfig = config;

            if ((_sourceInfo != null) && !_isRunning)
            {
                await OpenSource(_sourceInfo.FileName);
            }
        }

        #endregion

        #endregion
    }
}
