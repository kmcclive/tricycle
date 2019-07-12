using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Media.Models;
using Tricycle.UI.Models;
using Xamarin.Forms;

namespace Tricycle.UI.ViewModels
{
    public delegate void AlertEventHandler(string title, string message);

    public class MainViewModel : ViewModelBase
    {
        #region Constants

        const int DEFAULT_STEP_COUNT = 4;
        const string DEFAULT_EXTENSION = "mp4";
        static readonly ListItem ORIGINAL_OPTION = new ListItem("Same as source", Guid.NewGuid());
        static readonly ListItem NONE_OPTION = new ListItem("None", Guid.NewGuid());

        #endregion

        #region Fields

        readonly IFileBrowser _fileBrowser;
        readonly IMediaInspector _mediaInspector;
        readonly ICropDetector _cropDetector;
        readonly IFileSystem _fileSystem;
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
        bool _isAutocropEnabled;
        bool _isAutocropChecked;
        IList<ListItem> _aspectRatioOptions;
        ListItem _selectedAspectRatio;
        bool _isDenoiseChecked;
        IList<AudioOutputViewModel> _audioOutputs = new ObservableCollection<AudioOutputViewModel>();
        bool _isContainerFormatEnabled;
        IList<ListItem> _containerFormatOptions;
        ListItem _selectedContainerFormat;
        string _destinationName;
        bool _isProgressVisible;
        double _progress;
        string _progressText;
        string _rateText;

        MediaInfo _sourceInfo;
        CropParameters _cropParameters;
        string _defaultExtension = DEFAULT_EXTENSION;
        VideoStreamInfo _primaryVideoStream;
        IList<ListItem> _audioFormatOptions;
        IList<ListItem> _audioTrackOptions;
        IDictionary<AudioFormat, IList<ListItem>> _audioMixdownOptionsByFormat;

        #endregion

        #region Constructors

        public MainViewModel(IFileBrowser fileBrowser,
                             IMediaInspector mediaInspector,
                             ICropDetector cropDetector,
                             IFileSystem fileSystem,
                             TricycleConfig tricycleConfig,
                             string defaultDestinationDirectory)
        {
            _fileBrowser = fileBrowser;
            _mediaInspector = mediaInspector;
            _cropDetector = cropDetector;
            _fileSystem = fileSystem;
            _tricycleConfig = tricycleConfig;
            _defaultDestinationDirectory = defaultDestinationDirectory;

            SourceSelectCommand = new Command(async () => await SelectSource());
            DestinationSelectCommand = new Command(async () => await SelectDestination(), () => _sourceInfo != null);
            StartCommand = new Command(async () => await StartTranscode(),
                                       () => _sourceInfo != null && (_videoFormatOptions?.Any() == true));

            ContainerFormatOptions = GetContainerFormatOptions();
            SelectedContainerFormat = ContainerFormatOptions?.FirstOrDefault();
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
            set { SetProperty(ref _isVideoConfigEnabled, value); }
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
                IsHdrChecked = IsHdrEnabled;
                QualityStepCount =
                    _tricycleConfig.Video?.Codecs?.FirstOrDefault(f => f.Format.Equals(value.Value))?
                                                  .QualitySteps ?? DEFAULT_STEP_COUNT;
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
            get { return _isHdrEnabled; }
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

        public bool IsAutocropEnabled
        {
            get { return _isAutocropEnabled; }
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

        public bool IsDenoiseChecked
        {
            get { return _isDenoiseChecked; }
            set { SetProperty(ref _isDenoiseChecked, value); }
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

        public ICommand SourceSelectCommand { get; }
        public ICommand DestinationSelectCommand { get; }
        public ICommand StartCommand { get; }

        #endregion

        #region Events

        public event AlertEventHandler Alert;

        #endregion

        #region Methods

        #region Command Actions

        async Task SelectSource()
        {
            var result = await _fileBrowser.BrowseToOpen();

            if (result.Confirmed)
            {
                SourceName = result.FileName;
                _sourceInfo = await _mediaInspector.Inspect(result.FileName);

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
                IsVideoConfigEnabled = _sourceInfo != null;
                PopulateAudioOptions(_sourceInfo);
                ((Command)DestinationSelectCommand).ChangeCanExecute();
                ((Command)StartCommand).ChangeCanExecute();

                if (!isValid)
                {
                    Alert?.Invoke("Invalid Source", "The selected file could not be opened.");
                }
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

        async Task StartTranscode()
        {

        }

        #endregion

        #region Helpers

        void ProcessConfig(TricycleConfig config)
        {
            _defaultExtension = GetDefaultExtension((ContainerFormat)SelectedContainerFormat.Value);

            VideoFormatOptions = GetVideoFormatOptions(config.Video?.Codecs);
            SelectedVideoFormat = VideoFormatOptions?.FirstOrDefault();
            QualityStepCount =
                config.Video?.Codecs?.FirstOrDefault()?.QualitySteps ?? DEFAULT_STEP_COUNT;

            ProcessAudioCodecs(config.Audio?.Codecs);
        }

        IList<ListItem> GetVideoFormatOptions(IList<VideoCodec> codecs)
        {
            return codecs?.Select(f =>
            {
                switch (f.Format)
                {
                    case VideoFormat.Avc:
                        return new ListItem("AVC", f.Format);
                    case VideoFormat.Hevc:
                        return new ListItem("HEVC", f.Format);
                    default:
                        return new ListItem(string.Empty);
                }
            }).Distinct().ToArray();
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

        void ProcessAudioCodecs(IList<AudioCodec> codecs)
        {
            _audioFormatOptions = new List<ListItem>();
            _audioMixdownOptionsByFormat = new Dictionary<AudioFormat, IList<ListItem>>();

            if (codecs?.Any() != true)
            {
                return;
            }

            foreach (var codec in codecs)
            {
                AudioFormat format = codec.Format;
                string name = GetAudioFormatName(format);

                if (name == null)
                {
                    continue;
                }

                var formatOption = new ListItem(name, format);

                if (!_audioFormatOptions.Contains(formatOption))
                {
                    _audioFormatOptions.Add(new ListItem(name, format));
                    _audioMixdownOptionsByFormat[format] = GetAudioMixdownOptions(codec.Presets);
                }
            }
        }

        string GetAudioFormatName(AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.Aac:
                    return "AAC";
                case AudioFormat.Ac3:
                    return "Dolby Digital";
                case AudioFormat.HeAac:
                    return "HE-AAC";
                default:
                    return null;
            }
        }

        IList<ListItem> GetAudioMixdownOptions(IList<AudioPreset> presets)
        {
            return presets?.Select(p =>
            {
                switch (p.Mixdown)
                {
                    case AudioMixdown.Mono:
                        return new ListItem("Mono", p.Mixdown);
                    case AudioMixdown.Stereo:
                        return new ListItem("Stereo", p.Mixdown);
                    case AudioMixdown.Surround5dot1:
                        return new ListItem("Surround", p.Mixdown);
                    default:
                        return new ListItem(string.Empty);
                }
            }).OrderByDescending(p => p.Name).ToArray();
        }

        VideoStreamInfo GetPrimaryVideoStream(IList<StreamInfo> streams)
        {
            return streams?.OfType<VideoStreamInfo>()
                           .FirstOrDefault();
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
                IsHdrChecked = IsHdrEnabled;
                SizeOptions = GetSizeOptions(videoStream.Dimensions);
                SelectedSize = SizeOptions?.FirstOrDefault();
                IsAutocropEnabled = HasBars(videoStream.Dimensions, _cropParameters);
                IsAutocropChecked = IsAutocropEnabled;

                if (IsHdrChecked)
                {
                    SelectedVideoFormat =
                        _videoFormatOptions?.FirstOrDefault(f => object.Equals(f.Value, VideoFormat.Hevc));
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
            return object.Equals(selectedFormat?.Value, VideoFormat.Hevc) &&
                object.Equals(videoStream?.DynamicRange, DynamicRange.High);
        }

        IList<ListItem> GetSizeOptions(Dimensions sourceDimensions)
        {
            IList<ListItem> result =
                _tricycleConfig.Video?.SizePresets?.Where(s => s.Value.Height <= sourceDimensions.Height ||
                                                               s.Value.Width <= sourceDimensions.Width)
                                                   .OrderByDescending(s => s.Value.Width * s.Value.Height)
                                                   .Select(s => new ListItem(s.Key))
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
            double aspectRatio = GetAspectRatio(dimensions);

            IList<ListItem> result =
                _tricycleConfig.Video?.AspectRatioPresets?.Where(p => GetAspectRatio(p.Value) <= aspectRatio)
                                                          .OrderByDescending(p => GetAspectRatio(p.Value))
                                                          .Select(s => new ListItem(s.Key))
                                                          .ToList() ?? new List<ListItem>();

            result.Insert(0, ORIGINAL_OPTION);

            return result;
        }

        double GetAspectRatio(Dimensions dimensions)
        {
            return (double)dimensions.Width / (double)dimensions.Height;
        }

        bool HasBars(Dimensions dimensions, CropParameters cropParameters)
        {
            if (cropParameters != null)
            {
                return !dimensions.Equals(cropParameters.Size);
            }

            return false;
        }

        void PopulateAudioOptions(MediaInfo sourceInfo)
        {
            ClearAudioOutputs();

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
                                                   .Select(s => new ListItem(GetAudioTrackName(s, index++), s))
                                                   .ToList() ?? new List<ListItem>();

            result.Insert(0, NONE_OPTION);

            return result;
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

            if (Regex.IsMatch(audioStream.FormatName, @"ac(\-)?3", RegexOptions.IgnoreCase))
            {
                format = "Dolby Digital";
            }
            else if (Regex.IsMatch(audioStream.FormatName, @"aac", RegexOptions.IgnoreCase))
            {
                format = "AAC";
            }
            else if (Regex.IsMatch(audioStream.FormatName, @"truehd", RegexOptions.IgnoreCase))
            {
                format = "Dolby TrueHD";
            }

            if (!string.IsNullOrWhiteSpace(audioStream.ProfileName))
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

            return $"{index}: {format} {mixdown} ({audioStream.Language})";
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

        IList<ListItem> GetAudioMixdownOptions(AudioStreamInfo stream, AudioFormat format)
        {
            IList<ListItem> result = null;

            if (_audioMixdownOptionsByFormat.TryGetValue(format, out var allOptions))
            {
                result = allOptions.Where(o => GetChannelCount((AudioMixdown)o.Value) <= stream.ChannelCount)
                                   .ToArray();
            }

            return result;
        }

        int GetChannelCount(AudioMixdown mixdown)
        {
            switch (mixdown)
            {
                case AudioMixdown.Mono:
                    return 1;
                case AudioMixdown.Stereo:
                    return 2;
                case AudioMixdown.Surround5dot1:
                    return 6;
                default:
                    return 0;
            }
        }

        void UnsubscribeFromAudioOutputEvents(AudioOutputViewModel audioOutput)
        {
            audioOutput.FormatSelected -= OnAudioFormatSelected;
            audioOutput.TrackSelected -= OnAudioTrackSelected;
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
                model.FormatOptions = _audioFormatOptions;
                model.SelectedFormat = _audioFormatOptions.FirstOrDefault();
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

                mixdownOptions = GetAudioMixdownOptions(stream, format);
            }

            model.MixdownOptions = mixdownOptions;
            model.SelectedMixdown = mixdownOptions?.FirstOrDefault();
        }

        #endregion

        #endregion
    }
}
