using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.UI.Models;
using Tricycle.Utilities;
using Xamarin.Forms;
using FFmpegAudioCodec = Tricycle.Media.FFmpeg.Models.AudioCodec;
using FFmpegAudioConfig = Tricycle.Media.FFmpeg.Models.AudioConfig;
using FFmpegVideoCodec = Tricycle.Media.FFmpeg.Models.VideoCodec;
using FFmpegVideoConfig = Tricycle.Media.FFmpeg.Models.VideoConfig;
using TricycleAudioCodec = Tricycle.Models.Config.AudioCodec;
using TricycleAudioConfig = Tricycle.Models.Config.AudioConfig;
using TricycleVideoCodec = Tricycle.Models.Config.VideoCodec;
using TricycleVideoConfig = Tricycle.Models.Config.VideoConfig;

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

        bool _isLoading;
        bool _isDirty;
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

            _audioMixdownOptions.Insert(0, EMPTY_ITEM);

            CompleteCommand = new Command(new Action(Complete));
            CancelCommand = new Command(async() => await Cancel());
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
        public ICommand CancelCommand { get; }

        #endregion

        #region Events

        public event Action Closed;
        public event ConfirmEventHandler Confirm;

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            _isLoading = true;
            _isDirty = false;

            Load(_tricycleConfigManager.Config);
            Load(_ffmpegConfigManager.Config);

            _isLoading = false;
        }

        #endregion

        #region Protected

        protected override void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            base.SetProperty(ref field, value, propertyName);

            _isDirty |= !_isLoading;
        }

        #endregion

        #region Command Actions

        void Complete()
        {
            if (_isDirty)
            {
                _tricycleConfigManager.Config = GenerateTricycleConfig();
                _ffmpegConfigManager.Config = GenerateFFmpegConfig();

                _tricycleConfigManager.Save();
                _ffmpegConfigManager.Save();
            }

            Closed?.Invoke();
        }

        async Task Cancel()
        {
            bool proceed = !_isDirty ||
                await Confirm?.Invoke("Discard Changes", "Are you sure you want to lose your changes?");

            if (proceed)
            {
                Initialize();
                Closed?.Invoke();
            }
        }

        #endregion

        #region Helpers

        void Load(TricycleConfig config)
        {
            AvcQualityScale?.ClearHandlers();
            HevcQualityScale?.ClearHandlers();

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

        void Load(IList<VideoPresetViewModel> presets, IDictionary<string, Dimensions> dictionary)
        {
            presets.Clear();

            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                {
                    AddVideoPreset(presets, pair);
                }
            }

            AddVideoPreset(presets, null);
        }

        void Load(IDictionary<AudioFormat, TricycleAudioCodec> dictionary)
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

        void Load(FFmpegConfig config)
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

        QualityScaleViewModel GetQualityScale(TricycleVideoCodec codec)
        {
            var result = new QualityScaleViewModel()
            {
                Min = codec?.QualityRange.Min,
                Max = codec?.QualityRange.Max,
                StepCount = codec?.QualitySteps
            };

            result.Modified += () => _isDirty |= !_isLoading;

            return result;
        }

        void AddVideoPreset(IList<VideoPresetViewModel> presets, KeyValuePair<string, Dimensions>? pair)
        {
            var preset = new VideoPresetViewModel()
            {
                Name = pair?.Key,
                Width = pair?.Value.Width,
                Height = pair?.Value.Height,
                IsRemoveEnabled = pair.HasValue
            };

            preset.Modified += () => OnVideoPresetModified(presets, preset);
            preset.Removed += () => OnVideoPresetRemoved(presets, preset);

            presets.Add(preset);
        }

        AudioQualityPresetViewModel GetAudioQualityPreset(AudioFormat? format, AudioPreset preset)
        {
            var result = new AudioQualityPresetViewModel()
            {
                FormatOptions = _audioFormatOptions,
                MixdownOptions = _audioMixdownOptions,
                SelectedFormat = format.HasValue ? GetAudioFormatOption(format.Value) : EMPTY_ITEM,
                SelectedMixdown = preset?.Mixdown != null ? GetAudioMixdownOption(preset.Mixdown) : EMPTY_ITEM,
                Quality = preset?.Quality,
                IsRemoveEnabled = preset != null
            };

            result.Modified += () => OnAudioQualityPresetModified(result);
            result.Removed += () => OnAudioQualityPresetRemoved(result);

            return result;
        }

        ListItem GetAudioFormatOption(AudioFormat format)
        {
            return new ListItem(AudioUtility.GetFormatName(format), format);
        }

        ListItem GetAudioMixdownOption(AudioMixdown mixdown)
        {
            return new ListItem(AudioUtility.GetMixdownName(mixdown), mixdown);
        }

        TricycleConfig GenerateTricycleConfig()
        {
            var result = new TricycleConfig()
            {
                CompletionAlert = AlertOnCompletion,
                DeleteIncompleteFiles = DeleteIncompleteFiles,
                ForcedSubtitlesOnly = PreferForcedSubtitles,
                Audio = GenerateTricycleAudioConfig(),
                Video = GenerateTricycleVideoConfig()
            };

            if (!string.IsNullOrWhiteSpace(Mp4FileExtension))
            {
                result.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
                {
                    { ContainerFormat.Mp4, Mp4FileExtension }
                };
            }

            if (!string.IsNullOrWhiteSpace(MkvFileExtension))
            {
                if (result.DefaultFileExtensions == null)
                {
                    result.DefaultFileExtensions = new Dictionary<ContainerFormat, string>();
                }

                result.DefaultFileExtensions[ContainerFormat.Mkv] = MkvFileExtension;
            }

            return result;
        }

        TricycleAudioConfig GenerateTricycleAudioConfig()
        {
            var result = new TricycleAudioConfig()
            {
                PassthruMatchingTracks = PassthruMatchingTracks
            };

            foreach (var preset in AudioQualityPresets)
            {
                if ((preset.SelectedFormat == EMPTY_ITEM) ||
                    (preset.SelectedMixdown == EMPTY_ITEM) ||
                    !preset.Quality.HasValue)
                {
                    continue;
                }

                if (result.Codecs == null)
                {
                    result.Codecs = new Dictionary<AudioFormat, TricycleAudioCodec>();
                }

                var codec = result.Codecs.GetValueOrDefault((AudioFormat)preset.SelectedFormat.Value) ??
                            new TricycleAudioCodec()
                            {
                                Presets = new List<AudioPreset>()
                            };

                codec.Presets.Add(new AudioPreset()
                {
                    Mixdown = (AudioMixdown)preset.SelectedMixdown.Value,
                    Quality = preset.Quality.Value
                });
            }

            return result;
        }

        TricycleVideoConfig GenerateTricycleVideoConfig()
        {
            return new TricycleVideoConfig()
            {
                SizeDivisor = SizeDivisor ?? 8,
                Codecs = new Dictionary<VideoFormat, TricycleVideoCodec>()
                {
                    { VideoFormat.Avc, GenerateVideoCodec(AvcQualityScale) },
                    { VideoFormat.Hevc, GenerateVideoCodec(HevcQualityScale) }
                },
                SizePresets = GenerateVideoPresets(SizePresets),
                AspectRatioPresets = GenerateVideoPresets(AspectRatioPresets)
            };
        }

        TricycleVideoCodec GenerateVideoCodec(QualityScaleViewModel qualityScale)
        {
            var result = new TricycleVideoCodec();

            if (qualityScale.Min.HasValue && qualityScale.Max.HasValue && qualityScale.StepCount.HasValue)
            {
                result.QualityRange = new Range<decimal>(qualityScale.Min, qualityScale.Max);
                result.QualitySteps = qualityScale.StepCount.Value;
            }
            else
            {
                result.QualityRange = new Range<decimal>(22, 18);
                result.QualitySteps = 4;
            }

            return result;
        }

        IDictionary<string, Dimensions> GenerateVideoPresets(IList<VideoPresetViewModel> presets)
        {
            IDictionary<string, Dimensions> result = null;

            foreach (var preset in presets)
            {
                if (string.IsNullOrWhiteSpace(preset.Name) ||
                    !preset.Width.HasValue ||
                    !preset.Height.HasValue)
                {
                    continue;
                }

                if (result == null)
                {
                    result = new Dictionary<string, Dimensions>();
                }

                result[preset.Name] = new Dimensions(preset.Width.Value, preset.Height.Value);
            }

            return result;
        }

        FFmpegConfig GenerateFFmpegConfig()
        {
            return new FFmpegConfig()
            {
                Audio = GenerateFFmpegAudioConfig(),
                Video = GenerateFFmpegVideoConfig()
            };
        }

        FFmpegAudioConfig GenerateFFmpegAudioConfig()
        {
            return new FFmpegAudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, FFmpegAudioCodec>()
                    {
                        {
                            AudioFormat.Aac,
                            new FFmpegAudioCodec()
                            {
                                Name = string.IsNullOrWhiteSpace(AacCodec) ? "aac" : AacCodec
                            }
                        },
                        {
                            AudioFormat.Ac3,
                            new FFmpegAudioCodec()
                            {
                                Name = string.IsNullOrWhiteSpace(Ac3Codec) ? "ac3" : Ac3Codec
                            }
                        }
                    }
            };
        }

        FFmpegVideoConfig GenerateFFmpegVideoConfig()
        {
            return new FFmpegVideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, FFmpegVideoCodec>()
                {
                    {
                        VideoFormat.Avc,
                        new FFmpegVideoCodec()
                        {
                            Preset = SelectedX264Preset.ToString()
                        }
                    },
                    {
                        VideoFormat.Hevc,
                        new FFmpegVideoCodec()
                        {
                            Preset = SelectedX265Preset.ToString()
                        }
                    }
                },
                CropDetectOptions = string.IsNullOrWhiteSpace(CropDetectOptions) ? null : CropDetectOptions,
                DenoiseOptions = string.IsNullOrWhiteSpace(DenoiseOptions) ? "hqdn3d=4:4:3:3" : DenoiseOptions,
                TonemapOptions = string.IsNullOrWhiteSpace(TonemapOptions) ? "hable:desat=0" : TonemapOptions
            };
        }

        #endregion

        #region Event Handlers

        void OnAudioQualityPresetModified(AudioQualityPresetViewModel preset)
        {
            if (_isLoading)
            {
                return;
            }

            _isDirty = true;
            preset.IsRemoveEnabled = true;

            if (!AudioQualityPresets.Any(p => p.SelectedFormat == EMPTY_ITEM &&
                                              p.SelectedMixdown == EMPTY_ITEM &&
                                              !p.Quality.HasValue))
            {
                AudioQualityPresets.Add(GetAudioQualityPreset(null, null));
            }
        }

        void OnAudioQualityPresetRemoved(AudioQualityPresetViewModel preset)
        {
            _isDirty = true;

            preset.ClearHandlers();
            AudioQualityPresets.Remove(preset);
        }

        void OnVideoPresetModified(IList<VideoPresetViewModel> presets, VideoPresetViewModel preset)
        {
            if (_isLoading)
            {
                return;
            }

            _isDirty = true;
            preset.IsRemoveEnabled = true;

            if (!presets.Any(p => string.IsNullOrEmpty(p.Name) &&
                                  !p.Width.HasValue &&
                                  !p.Height.HasValue))
            {
                AddVideoPreset(presets, null);
            }
        }

        void OnVideoPresetRemoved(IList<VideoPresetViewModel> presets, VideoPresetViewModel preset)
        {
            _isDirty = true;

            preset.ClearHandlers();
            presets.Remove(preset);
        }

        #endregion

        #endregion
    }
}
