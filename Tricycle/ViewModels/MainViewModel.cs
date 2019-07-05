using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Media.Models;
using Tricycle.Models;
using Xamarin.Forms;

namespace Tricycle.ViewModels
{
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
        readonly TricycleConfig _tricycleConfig;
        readonly string _defaultDestinationDirectory;

        bool _isSourceInfoVisible;
        string _sourceDuration;
        string _sourceSize;
        bool _isSourceHdr;
        string _sourceName;
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
        IList<ListItem> _audioTrackOptions;
        bool _isContainerFormatEnabled;
        IList<ListItem> _containerFormatOptions;
        ListItem _selectedContainerFormat;
        string _destinationName;

        MediaInfo _sourceInfo;
        CropParameters _cropParameters;
        string _defaultExtension = DEFAULT_EXTENSION;

        #endregion

        #region Constructors

        public MainViewModel(IFileBrowser fileBrowser,
                             IMediaInspector mediaInspector,
                             ICropDetector cropDetector,
                             TricycleConfig tricycleConfig,
                             string defaultDestinationDirectory)
        {
            _fileBrowser = fileBrowser;
            _mediaInspector = mediaInspector;
            _cropDetector = cropDetector;
            _tricycleConfig = tricycleConfig;
            _defaultDestinationDirectory = defaultDestinationDirectory;
            _defaultExtension = GetDefaultExtension(ContainerFormat.Mp4);

            SourceSelectCommand = new Command(async () => await SelectSource());
            DestinationSelectCommand = new Command(async () => await SelectDestination(), () => _sourceInfo != null);
            StartCommand = new Command(async () => await StartTranscode(), () => _sourceInfo != null);
            
            VideoFormatOptions = tricycleConfig.Video?.Codecs?.Select(f =>
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
            }).ToArray();
            QualityStepCount =
                tricycleConfig.Video?.Codecs?.FirstOrDefault()?.QualitySteps ?? DEFAULT_STEP_COUNT;         
            ContainerFormatOptions = Enum.GetValues(typeof(ContainerFormat)).Cast<ContainerFormat>().Select(f =>
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

                PopulateAspectRatioOptions();
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

        public IList<ListItem> AudioTrackOptions
        {
            get { return _audioTrackOptions; }
            set { SetProperty(ref _audioTrackOptions, value); }
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

        public ICommand SourceSelectCommand { get; }
        public ICommand DestinationSelectCommand { get; }
        public ICommand StartCommand { get; }

        #endregion

        #region Methods

        #region Command Actions

        async Task SelectSource()
        {
            var result = await _fileBrowser.BrowseToOpen();

            if (result.Confirmed)
            {
                _sourceInfo = await _mediaInspector.Inspect(result.FileName);

                if (_sourceInfo != null)
                {
                    TimeSpan duration = _sourceInfo.Duration;
                    VideoStreamInfo videoStream = GetPrimaryVideoStream(_sourceInfo.Streams);

                    SourceName = result.FileName;
                    SourceDuration = string.Format("{0:00}:{1:00}:{2:00}",
                        duration.Hours, duration.Minutes, duration.Seconds);
                    SourceSize = GetSizeName(videoStream.Dimensions);
                    IsSourceHdr = videoStream.DynamicRange == DynamicRange.High;
                    IsSourceInfoVisible = true;

                    _cropParameters = await _cropDetector.Detect(_sourceInfo);

                    IsHdrChecked = IsSourceHdr;
                    SizeOptions = GetSizeOptions(videoStream.Dimensions);
                    IsAutocropEnabled = HasBars(videoStream.Dimensions, _cropParameters);
                    IsAutocropChecked = IsAutocropEnabled;
                    PopulateAspectRatioOptions();
                    AudioTrackOptions = GetAudioTrackOptions(_sourceInfo.Streams);

                    IsContainerFormatEnabled = true;
                    DestinationName = GetDefaultDestinationName(_sourceInfo, _defaultExtension);
                    ((Command)DestinationSelectCommand).ChangeCanExecute();
                    ((Command)StartCommand).ChangeCanExecute();
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

        VideoStreamInfo GetPrimaryVideoStream(IList<StreamInfo> streams)
        {
            return streams.OfType<VideoStreamInfo>()
                          .FirstOrDefault();
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

            return "480p";
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

        void PopulateAspectRatioOptions()
        {
            Dimensions? sourceDimensions = null;

            if (_sourceInfo?.Streams != null)
            {
                var videoStream = GetPrimaryVideoStream(_sourceInfo.Streams);

                sourceDimensions = videoStream?.Dimensions;
            }

            if (sourceDimensions.HasValue && _cropParameters != null)
            {
                AspectRatioOptions =
                    GetAspectRatioOptions(sourceDimensions.Value, _isAutocropChecked ? _cropParameters : null);
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
            } while (File.Exists(result));

            return result;
        }

        #endregion

        #endregion
    }
}
