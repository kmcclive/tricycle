using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        const int DEFAULT_STEP_COUNT = 4;
        static readonly ListItem ORIGINAL_OPTION = new ListItem("Same as source", Guid.NewGuid());

        readonly IFileBrowser _fileBrowser;
        readonly IMediaInspector _mediaInspector;
        readonly ICropDetector _cropDetector;
        readonly TricycleConfig _tricycleConfig;

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

        MediaInfo _sourceInfo;
        CropParameters _cropParameters;

        public MainViewModel(IFileBrowser fileBrowser,
                             IMediaInspector mediaInspector,
                             ICropDetector cropDetector,
                             TricycleConfig tricycleConfig)
        {
            _fileBrowser = fileBrowser;
            _mediaInspector = mediaInspector;
            _cropDetector = cropDetector;
            _tricycleConfig = tricycleConfig;

            SourceSelectCommand = new Command(async () => await SelectSource());
            VideoFormatOptions = tricycleConfig.Video?.Codecs?.Select(f =>
            {
                switch (f.Format)
                {
                    case VideoFormat.Avc:
                        return new ListItem("AVC", f.Format);
                    case VideoFormat.Hevc:
                        return new ListItem("HEVC", f.Format);
                    default:
                        return new ListItem("");
                }
            }).ToArray();
            QualityStepCount =
                tricycleConfig.Video?.Codecs?.FirstOrDefault()?.QualitySteps ?? DEFAULT_STEP_COUNT;
        }

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

        public ICommand SourceSelectCommand { get; }


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
                }
            }
        }

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
    }
}
