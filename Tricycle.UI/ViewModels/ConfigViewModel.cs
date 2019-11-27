using System;
using System.Collections.Generic;
using Tricycle.UI.Models;

namespace Tricycle.UI.ViewModels
{
    public class ConfigViewModel : ViewModelBase
    {
        #region Fields

        bool _alertOnCompletion;
        bool _deleteIncompleteFiles;
        bool _preferForcedSubtitles;
        string _mp4FileExtension;
        string _mkvFileExtension;
        int _sizeDivisor;
        QualityScaleViewModel _avcQualityScale;
        QualityScaleViewModel _hevcQualityScale;
        IList<VideoPresetViewModel> _sizePresets;
        IList<VideoPresetViewModel> _aspectRatioPresets;
        bool _passthruMatchingTracks;
        IList<AudioQualityPresetViewModel> _audioQualityPresets;
        IList<ListItem> _x264PresetOptions;
        ListItem _selectedX264Preset;
        IList<ListItem> _x265PresetOptions;
        ListItem _selectedX265Preset;
        string _aacCodec;
        string _ac3Codec;
        string _cropDetectOptions;
        string _denoiseOptions;
        string _tonemapOptions;

        #endregion

        #region Properties

        public bool AlertOnCompletion
        {
            get => _alertOnCompletion;
            set => SetProperty(ref _alertOnCompletion, value);
        }

        public bool DeleteIncompletFiles
        {
            get => _deleteIncompleteFiles;
            set => SetProperty(ref _deleteIncompleteFiles, value);
        }

        public bool PreferForcedSubtitles
        {
            get => _preferForcedSubtitles;
            set => SetProperty(ref _preferForcedSubtitles, value);
        }

        public string Mp4FileExtension
        {
            get => _mp4FileExtension;
            set => SetProperty(ref _mp4FileExtension, value);
        }

        public string MkvFileExtension
        {
            get => _mkvFileExtension;
            set => SetProperty(ref _mkvFileExtension, value);
        }

        public int SizeDivisor
        {
            get => _sizeDivisor;
            set => SetProperty(ref _sizeDivisor, value);
        }

        public QualityScaleViewModel AvcQualityScale
        {
            get => _avcQualityScale;
            set => SetProperty(ref _avcQualityScale, value);
        }

        public QualityScaleViewModel HevcQualityScale
        {
            get => _hevcQualityScale;
            set => SetProperty(ref _hevcQualityScale, value);
        }

        public IList<VideoPresetViewModel> SizePresets
        {
            get => _sizePresets;
            set => SetProperty(ref _sizePresets, value);
        }

        public IList<VideoPresetViewModel> AspectRatioPresets
        {
            get => _aspectRatioPresets;
            set => SetProperty(ref _aspectRatioPresets, value);
        }

        public bool PassthruMatchingTracks
        {
            get => _passthruMatchingTracks;
            set => SetProperty(ref _passthruMatchingTracks, value);
        }

        public IList<AudioQualityPresetViewModel> AudioQualityPresets
        {
            get => _audioQualityPresets;
            set => SetProperty(ref _audioQualityPresets, value);
        }

        public IList<ListItem> X264PresetOptions
        {
            get => _x264PresetOptions;
            set => SetProperty(ref _x264PresetOptions, value);
        }

        public ListItem SelectedX264Preset
        {
            get => _selectedX264Preset;
            set => SetProperty(ref _selectedX264Preset, value);
        }

        public IList<ListItem> X265PresetOptions
        {
            get => _x265PresetOptions;
            set => SetProperty(ref _x265PresetOptions, value);
        }

        public ListItem SelectedX265Preset
        {
            get => _selectedX265Preset;
            set => SetProperty(ref _selectedX265Preset, value);
        }

        public string AacCodec
        {
            get => _aacCodec;
            set => SetProperty(ref _aacCodec, value);
        }

        public string Ac3Codec
        {
            get => _ac3Codec;
            set => SetProperty(ref _ac3Codec, value);
        }

        public string CropDetectOptions
        {
            get => _cropDetectOptions;
            set => SetProperty(ref _cropDetectOptions, value);
        }

        public string DenoiseOptions
        {
            get => _denoiseOptions;
            set => SetProperty(ref _denoiseOptions, value);
        }

        public string TonemapOptions
        {
            get => _tonemapOptions;
            set => SetProperty(ref _tonemapOptions, value);
        }

        #endregion
    }
}
