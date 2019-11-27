using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.UI.Models;
using Tricycle.Utilities;
using Xamarin.Forms;

using TricycleVideoCodec = Tricycle.Models.Config.VideoCodec;
using TricycleAudioCodec = Tricycle.Models.Config.AudioCodec;
using System.Linq;

namespace Tricycle.UI.ViewModels
{
    public class ConfigViewModel : ViewModelBase
    {
        #region Constants

        static readonly ListItem EMPTY_ITEM = new ListItem(string.Empty);

        #endregion

        #region Fields

        IConfigManager<TricycleConfig> _tricycleConfigManager;
        IConfigManager<FFmpegConfig> _ffmpegConfigManager;

        bool _alertOnCompletion;
        bool _deleteIncompleteFiles;
        bool _preferForcedSubtitles;
        string _mp4FileExtension;
        string _mkvFileExtension;
        int? _sizeDivisor;
        QualityScaleViewModel _avcQualityScale;
        QualityScaleViewModel _hevcQualityScale;
        IList<VideoPresetViewModel> _sizePresets = new ObservableCollection<VideoPresetViewModel>();
        IList<VideoPresetViewModel> _aspectRatioPresets = new ObservableCollection<VideoPresetViewModel>();
        bool _passthruMatchingTracks;
        IList<AudioQualityPresetViewModel> _audioQualityPresets = new ObservableCollection<AudioQualityPresetViewModel>();
        IList<ListItem> _x264PresetOptions;
        ListItem _selectedX264Preset;
        IList<ListItem> _x265PresetOptions;
        ListItem _selectedX265Preset;
        string _aacCodec;
        string _ac3Codec;
        string _cropDetectOptions;
        string _denoiseOptions;
        string _tonemapOptions;

        IList<ListItem> _audioFormatOptions;
        IList<ListItem> _audioMixdownOptions;

        #endregion

        #region Constructors

        public ConfigViewModel(IConfigManager<TricycleConfig> tricycleConfigManager,
                               IConfigManager<FFmpegConfig> ffmpegConfigManager)
        {
            _tricycleConfigManager = tricycleConfigManager;
            _ffmpegConfigManager = ffmpegConfigManager;
            _x264PresetOptions = new List<ListItem>()
            {
                new ListItem("ultrafast"),
                new ListItem("superfast"),
                new ListItem("veryfast"),
                new ListItem("faster"),
                new ListItem("fast"),
                new ListItem("medium"),
                new ListItem("slow"),
                new ListItem("slower"),
                new ListItem("veryslow"),
                new ListItem("placebo"),
            };
            _x265PresetOptions = _x264PresetOptions;
            _audioFormatOptions = new List<ListItem>()
            {
                EMPTY_ITEM,
                GetAudioFormatOption(AudioFormat.Aac),
                GetAudioFormatOption(AudioFormat.Ac3)
            };
            _audioMixdownOptions = Enum.GetValues(typeof(AudioMixdown))
                                       .Cast<AudioMixdown>()
                                       .Select(m => GetAudioMixdownOption(m))
                                       .ToList();

            CompleteCommand = new Command(new Action(Complete));
        }

        #endregion

        #region Properties

        public bool AlertOnCompletion
        {
            get => _alertOnCompletion;
            set => SetProperty(ref _alertOnCompletion, value);
        }

        public bool DeleteIncompleteFiles
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

        public int? SizeDivisor
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

        public ICommand CompleteCommand { get; }

        #endregion

        #region Events

        public event Action Closed;

        #endregion

        #region Methods

        public void Initialize()
        {
            Load(_tricycleConfigManager.Config);
            Load(_ffmpegConfigManager.Config);
        }

        protected void Complete()
        {
            Closed?.Invoke();
        }

        protected void Load(TricycleConfig config)
        {
            AlertOnCompletion = config.CompletionAlert;
            DeleteIncompleteFiles = config.DeleteIncompleteFiles;
            PreferForcedSubtitles = config.ForcedSubtitlesOnly;
            Mp4FileExtension = config.DefaultFileExtensions?.GetValueOrDefault(ContainerFormat.Mp4);
            MkvFileExtension = config.DefaultFileExtensions?.GetValueOrDefault(ContainerFormat.Mkv);
            SizeDivisor = config.Video?.SizeDivisor;
            AvcQualityScale = GetQualityScale(config.Video?.Codecs?.GetValueOrDefault(VideoFormat.Avc));
            HevcQualityScale = GetQualityScale(config.Video?.Codecs?.GetValueOrDefault(VideoFormat.Hevc));
            PassthruMatchingTracks = config.Audio?.PassthruMatchingTracks ?? false;

            Load(SizePresets, config.Video?.SizePresets);
            Load(AspectRatioPresets, config.Video?.AspectRatioPresets);
            Load(config.Audio?.Codecs);
        }

        protected void Load(IList<VideoPresetViewModel> presets, IDictionary<string, Dimensions> dictionary)
        {
            presets.Clear();

            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                {
                    presets.Add(GetVideoPreset(pair));
                }
            }

            presets.Add(new VideoPresetViewModel());
        }

        protected void Load(IDictionary<AudioFormat, TricycleAudioCodec> dictionary)
        {
            AudioQualityPresets.Clear();

            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                {
                    var format = pair.Key;
                    var presets = pair.Value?.Presets;

                    if (presets != null)
                    {
                        foreach (var preset in presets)
                        {
                            AudioQualityPresets.Add(GetAudioQualityPreset(format, preset));
                        }
                    }
                }
            }

            AudioQualityPresets.Add(GetAudioQualityPreset(null, null));
        }

        protected void Load(FFmpegConfig config)
        {
            string x264Preset = config.Video?.Codecs?.GetValueOrDefault(VideoFormat.Avc)?.Preset;
            string x265Preset = config.Video?.Codecs?.GetValueOrDefault(VideoFormat.Hevc)?.Preset;

            SelectedX264Preset = string.IsNullOrWhiteSpace(x264Preset) ? null : new ListItem(x264Preset);
            SelectedX265Preset = string.IsNullOrWhiteSpace(x265Preset) ? null : new ListItem(x265Preset);
            AacCodec = config.Audio?.Codecs?.GetValueOrDefault(AudioFormat.Aac).Name;
            Ac3Codec = config.Audio?.Codecs?.GetValueOrDefault(AudioFormat.Ac3).Name;
            CropDetectOptions = config.Video?.CropDetectOptions;
            DenoiseOptions = config.Video?.DenoiseOptions;
            TonemapOptions = config.Video?.TonemapOptions;
        }

        protected QualityScaleViewModel GetQualityScale(TricycleVideoCodec codec)
        {
            return new QualityScaleViewModel()
            {
                Min = codec?.QualityRange.Min,
                Max = codec?.QualityRange.Max,
                StepCount = codec?.QualitySteps
            };
        }

        protected VideoPresetViewModel GetVideoPreset(KeyValuePair<string, Dimensions> pair)
        {
            return new VideoPresetViewModel()
            {
                Name = pair.Key,
                Width = pair.Value.Width,
                Height = pair.Value.Height
            };
        }

        protected AudioQualityPresetViewModel GetAudioQualityPreset(AudioFormat? format, AudioPreset preset)
        {
            return new AudioQualityPresetViewModel()
            {
                FormatOptions = _audioFormatOptions,
                MixdownOptions = _audioMixdownOptions,
                SelectedFormat = format.HasValue ? GetAudioFormatOption(format.Value) : EMPTY_ITEM,
                SelectedMixdown = GetAudioMixdownOption(preset?.Mixdown ?? AudioMixdown.Stereo),
                Quality = preset?.Quality
            };
        }

        protected ListItem GetAudioFormatOption(AudioFormat format)
        {
            return new ListItem(AudioUtility.GetFormatName(format), format);
        }

        protected ListItem GetAudioMixdownOption(AudioMixdown mixdown)
        {
            return new ListItem(AudioUtility.GetMixdownName(mixdown), mixdown);
        }

        #endregion
    }
}
