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
        double _qualityMin;
        double _qualityMax;
        bool _isHdrEnabled;
        bool _isHdrChecked;
        IList<ListItem> _sizeOptions;
        ListItem _selectedSize;
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
            set { SetProperty(ref _selectedVideoFormat, value); }
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
                }
            }
        }
    }
}
