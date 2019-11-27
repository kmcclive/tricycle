using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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
using Tricycle.UI.Models;
using Tricycle.Utilities;
using Xamarin.Forms;

namespace Tricycle.UI.ViewModels
{
    public delegate void AlertEventHandler(string title, string message);
    public delegate Task<bool> ConfirmEventHandler(string title, string message);

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
        readonly ITranscodeCalculator _transcodeCalculator;
        readonly IFileSystem _fileSystem;
        readonly IDevice _device;
        readonly IAppManager _appManager;
        readonly TricycleConfig _tricycleConfig;
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
        bool _isProgressVisible;
        double _progress;
        string _progressText;
        string _rateText;
        string _toggleStartImage = PLAY_IMAGE;

        MediaInfo _sourceInfo;
        CropParameters _cropParameters;
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
                             ITranscodeCalculator transcodeCalculator,
                             IFileSystem fileSystem,
                             IDevice device,
                             IAppManager appManager,
                             IConfigManager<TricycleConfig> tricycleConfigManager,
                             string defaultDestinationDirectory)
        {
            _fileBrowser = fileBrowser;
            _mediaInspector = mediaInspector;
            _mediaTranscoder = mediaTranscoder;
            _cropDetector = cropDetector;
            _transcodeCalculator = transcodeCalculator;
            _fileSystem = fileSystem;
            _device = device;
            _appManager = appManager;
            _tricycleConfig = tricycleConfigManager.Config;
            _defaultDestinationDirectory = defaultDestinationDirectory;

            _mediaTranscoder.Completed += OnTranscodeCompleted;
            _mediaTranscoder.Failed += OnTranscodeFailed;
            _mediaTranscoder.StatusChanged += OnTranscodeStatusChanged;

            _appManager.FileOpened += async fileName => await OpenSource(fileName);
            _appManager.Quitting += OnAppQuitting;

            SourceSelectCommand = new Command(async () => await SelectSource(),
                                              () => _isSourceSelectionEnabled);
            DestinationSelectCommand = new Command(async () => await SelectDestination(),
                                                   () => _isDestinationSelectionEnabled);
            StartCommand = new Command(async () => await ToggleRunning(),
                                       () => _isStartEnabled);

            ContainerFormatOptions = GetContainerFormatOptions();
            SelectedContainerFormat = ContainerFormatOptions?.FirstOrDefault();

            CropOptions = GetCropOptions();
            SelectedCropOption = CropOptions?.FirstOrDefault();
        }

        #endregion

        #region Properties

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

                PopulateAspectRatioOptions(_primaryVideoStream, _cropParameters, _isAutocropChecked);
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

        public bool IsProgressVisible
        {
            get { return _isProgressVisible; }
            set { SetProperty(ref _isProgressVisible, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        public string ProgressText
        {
            get { return _progressText; }
            set { SetProperty(ref _progressText, value); }
        }

        public string RateText
        {
            get { return _rateText; }
            set { SetProperty(ref _rateText, value); }
        }

        public string ToggleStartImage
        {
            get { return _toggleStartImage; }
            set { SetProperty(ref _toggleStartImage, value); }
        }

        public ICommand SourceSelectCommand { get; }
        public ICommand DestinationSelectCommand { get; }
        public ICommand StartCommand { get; }

        #endregion

        #region Events

        public event AlertEventHandler Alert;
        public event ConfirmEventHandler Confirm;

        #endregion

        #region Methods

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

        async Task ToggleRunning()
        {
            if (_isRunning)
            {
                await ConfirmStopTranscode();
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
            Command startCommand = ((Command)StartCommand);
            _isStartEnabled = false;

            _appManager.RaiseBusy();
            EnableControls(false);
            startCommand.ChangeCanExecute();

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

                ProcessConfig(_tricycleConfig); //this is done here to improve testability
                IsContainerFormatEnabled = true;
                DestinationName = GetDefaultDestinationName(_sourceInfo, _defaultExtension);
                isValid = true;
            }
            else
            {
                _sourceInfo = null;
                _cropParameters = null;
                IsContainerFormatEnabled = false;
                DestinationName = null;
            }

            DisplaySourceInfo(_sourceInfo, _primaryVideoStream);
            PopulateVideoOptions(_primaryVideoStream, _cropParameters);
            PopulateSubtitleOptions(_sourceInfo);
            IsVideoConfigEnabled = _sourceInfo != null;
            PopulateAudioOptions(_sourceInfo);
            UpdateManualCropCoordinates(_primaryVideoStream?.Dimensions ?? new Dimensions(), _cropParameters);

            if (isValid)
            {
                EnableControls(true);

                _isStartEnabled = _videoFormatOptions?.Any() == true;
            }
            else
            {
                Alert?.Invoke("Invalid Source", "The selected file could not be opened.");

                _isSourceSelectionEnabled = true;
                _isStartEnabled = false;

                ((Command)SourceSelectCommand).ChangeCanExecute();
            }

            startCommand.ChangeCanExecute();
            _appManager.RaiseReady();
        }

        void DisplaySourceInfo(MediaInfo sourceInfo, VideoStreamInfo videoStream)
        {
            if (sourceInfo != null)
            {
                TimeSpan duration = _sourceInfo.Duration;

                SourceDuration = string.Format("{0:00}:{1:00}:{2:00}",
                    duration.Hours, duration.Minutes, duration.Seconds);
                SourceSize = GetSizeName(videoStream.Dimensions);
                IsSourceHdr = videoStream.DynamicRange == DynamicRange.High;
                IsSourceInfoVisible = true;
            }
            else
            {
                IsSourceInfoVisible = false;
            }
        }

        string GetSizeName(Dimensions dimensions)
        {
            if ((dimensions.Width >= 3840) || (dimensions.Height >= 2160))
            {
                return "4K";
            }

            if ((dimensions.Width >= 1920) || (dimensions.Height >= 1080))
            {
                return "1080p";
            }

            if ((dimensions.Width >= 1280) || (dimensions.Height >= 720))
            {
                return "720p";
            }

            if ((dimensions.Width >= 853) || (dimensions.Height >= 480))
            {
                return "480p";
            }

            return $"{dimensions.Height}p";
        }

        void PopulateVideoOptions(VideoStreamInfo videoStream, CropParameters cropParameters)
        {
            if (videoStream != null)
            {
                IsHdrEnabled = videoStream.DynamicRange == DynamicRange.High;
                IsHdrChecked = _isHdrEnabled;
                SizeOptions = GetSizeOptions(videoStream.Dimensions);
                SelectedSize = SizeOptions?.FirstOrDefault();
                IsAutocropEnabled = HasBars(videoStream.Dimensions, _cropParameters);
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

            PopulateAspectRatioOptions(videoStream, cropParameters, IsAutocropChecked);
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

        void PopulateAspectRatioOptions(VideoStreamInfo videoStream, CropParameters cropParameters, bool autoCrop)
        {
            Dimensions? sourceDimensions = videoStream?.Dimensions;

            if (sourceDimensions.HasValue)
            {
                AspectRatioOptions =
                    GetAspectRatioOptions(sourceDimensions.Value, autoCrop ? cropParameters : null);
                SelectedAspectRatio = AspectRatioOptions?.FirstOrDefault();
            }
            else
            {
                AspectRatioOptions = null;
                SelectedAspectRatio = null;
            }
        }

        IList<ListItem> GetAspectRatioOptions(Dimensions sourceDimensions, CropParameters cropParameters)
        {
            var dimensions = cropParameters?.Size ?? sourceDimensions;
            double aspectRatio = VideoUtility.GetAspectRatio(dimensions);

            IList<ListItem> result =
                _tricycleConfig.Video?.AspectRatioPresets?.Where(p => VideoUtility.GetAspectRatio(p.Value) <= aspectRatio)
                                                          .OrderByDescending(p => VideoUtility.GetAspectRatio(p.Value))
                                                          .Select(s => new ListItem(s.Key, s.Value))
                                                          .ToList() ?? new List<ListItem>();

            result.Insert(0, ORIGINAL_OPTION);

            return result;
        }

        bool HasBars(Dimensions dimensions, CropParameters cropParameters)
        {
            if (cropParameters != null)
            {
                return !dimensions.Equals(cropParameters.Size);
            }

            return false;
        }

        void UpdateManualCropCoordinates(Dimensions sourceDimensions, CropParameters cropParameters)
        {
            if (cropParameters != null)
            {
                CropTop = cropParameters.Start.Y.ToString();
                CropBottom = (sourceDimensions.Height - cropParameters.Size.Height - cropParameters.Start.Y).ToString();
                CropLeft = cropParameters.Start.X.ToString();
                CropRight = (sourceDimensions.Width - cropParameters.Size.Width - cropParameters.Start.X).ToString();
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
            AudioFormat? knownFormat = AudioUtility.GetAudioFormat(audioStream);

            if (knownFormat.HasValue)
            {
                format = AudioUtility.GetFormatName(knownFormat.Value);
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
            var job = CreateJob();
            bool success = false;

            try
            {
                _mediaTranscoder.Start(job);

                _isRunning = true;
                IsProgressVisible = true;
                ToggleStartImage = STOP_IMAGE;

                _appManager.RaiseBusy();
                EnableControls(false);

                success = true;
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (InvalidOperationException) { }

            if (!success)
            {
                Alert?.Invoke("Job Error", @"Oops! Your job couldn't be started for some reason. ¯\_(ツ)_/¯");
            }
        }

        async Task<bool> ConfirmStopTranscode()
        {
            bool proceed = true;

            if (Confirm != null)
            {
                proceed = await Confirm("Stop Transcode", "Whoa... Are you sure you want to stop and lose your progress?");
            }

            if (proceed)
            {
                StopTranscode();
            }

            return proceed;
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
                Alert?.Invoke("Job Error", @"Oops! Your job couldn't be stopped for some reason. ¯\_(ツ)_/¯");
            }
        }

        void ResetJobState()
        {
            _isRunning = false;
            ToggleStartImage = PLAY_IMAGE;

            ResetProgress();
            EnableControls(true);
            _appManager.RaiseReady();
        }

        void ResetProgress()
        {
            IsProgressVisible = false;
            Progress = 0;
            ProgressText = string.Empty;
            RateText = string.Empty;
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
            return new TranscodeJob()
            {
                SourceInfo = _sourceInfo,
                OutputFileName = DestinationName,
                Format = (ContainerFormat)SelectedContainerFormat.Value,
                Streams = GetOutputStreams(),
                Subtitles = GetSubtitles()
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
                int value;
                var top = int.TryParse(CropTop, out value) ? value : 0;
                var bottom = int.TryParse(CropBottom, out value) ? value : 0;
                var left = int.TryParse(CropLeft, out value) ? value : 0;
                var right = int.TryParse(CropRight, out value) ? value : 0;

                cropParameters = new CropParameters()
                {
                    Start = new Coordinate<int>(left, top),
                    Size = new Dimensions(_primaryVideoStream.Dimensions.Width - left - right,
                                          _primaryVideoStream.Dimensions.Height - top - bottom)
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
                                                                cropParameters,
                                                                aspectRatio,
                                                                divisor);
        }

        Dimensions? GetScaledDimensions(CropParameters cropParameters, int divisor)
        {
            if (SelectedSize == ORIGINAL_OPTION)
            {
                return null;
            }

            Dimensions sourceDimensions = cropParameters?.Size ?? _primaryVideoStream.Dimensions;
            var targetDimensions = (Dimensions)SelectedSize.Value;

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
                Mixdown = (AudioMixdown)viewModel.SelectedMixdown.Value
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
            return outputStream.Format == AudioUtility.GetAudioFormat(sourceStream) &&
                AudioUtility.GetChannelCount(outputStream.Mixdown ?? AudioMixdown.Mono) == sourceStream.ChannelCount;
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

            string eta = null;

            if (status.Eta > TimeSpan.Zero)
            {
                eta = $"{Math.Floor(status.Eta.TotalHours):00}:{status.Eta.Minutes:00}:{status.Eta.Seconds:00}";
            }

            string progressText = string.Empty;

            if ((status.Size > 0) && (status.EstimatedTotalSize > 0))
            {
                progressText = $"({ByteSize.FromBytes(status.Size)} / {ByteSize.FromBytes(status.EstimatedTotalSize)}) ";
            }

            progressText += $"{status.Percent * 100:0.##}%";

            _device.BeginInvokeOnMainThread(() =>
            {
                if (!_isRunning)
                {
                    return;
                }

                Progress = status.Percent;
                RateText = string.IsNullOrEmpty(eta) ? $"{status.Speed:0.###}x" : $"ETA {eta} ({status.Speed:0.###}x)";
                ProgressText = progressText;
            });
        }

        void OnTranscodeCompleted()
        {
            _device.BeginInvokeOnMainThread(async () =>
            {
                if (_tricycleConfig.CompletionAlert)
                {
                    Alert?.Invoke("Transcode Complete", "Good news! Your shiny, new video is ready.");
                }

                ResetJobState();
                await OpenSource(SourceName);
            });
        }

        void OnTranscodeFailed(string error)
        {
            _device.BeginInvokeOnMainThread(async () =>
            {
                Alert?.Invoke("Transcode Failed", error);
                DeleteDestination();
                ResetJobState();
                await OpenSource(SourceName);
            });
        }

        private void OnAppQuitting(CancellationArgs args)
        {
            args.Cancel = _isRunning && !ConfirmStopTranscode().GetAwaiter().GetResult();
        }

        #endregion

        #endregion
    }
}
