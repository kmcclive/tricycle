using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Tricycle.IO;
using Tricycle.Media;
using Tricycle.Models;
using Xamarin.Forms;

namespace Tricycle.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        readonly IFileBrowser _fileBrowser;
        readonly IMediaInspector _mediaInspector;
        readonly TricycleConfig _tricycleConfig;

        bool _isSourceInfoVisible;
        string _sourceFormat;
        string _sourceDuration;
        string _sourceSize;
        string _sourceDynamicRange;
        Color _sourceDynamicRangeColor;
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

        public MainViewModel(IFileBrowser fileBrowser, IMediaInspector mediaInspector, TricycleConfig tricycleConfig)
        {
            _fileBrowser = fileBrowser;
            _mediaInspector = mediaInspector;
            _tricycleConfig = tricycleConfig;

            SourceSelectCommand = new Command(() =>
            {
                var result = _fileBrowser.BrowseToOpen();

                if (result.Confirmed)
                {
                    SourceName = result.FileName;
                }
            });
        }

        public bool IsSourceInfoVisible
        {
            get { return _isSourceInfoVisible; }
            set { SetProperty(ref _isSourceInfoVisible, value); }
        }

        public string SourceFormat
        {
            get { return _sourceFormat; }
            set { SetProperty(ref _sourceFormat, value); }
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

        public string SourceDynamicRange
        {
            get { return _sourceDynamicRange; }
            set { SetProperty(ref _sourceDynamicRange, value); }
        }

        public Color SourceDynamicRangeColor
        {
            get { return _sourceDynamicRangeColor; }
            set { SetProperty(ref _sourceDynamicRangeColor, value); }
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
    }
}
