﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.UI.Models;
using Tricycle.UI.ViewModels;
using Tricycle.Utilities;
using FFmpegAudioCodec = Tricycle.Media.FFmpeg.Models.AudioCodec;
using FFmpegAudioConfig = Tricycle.Media.FFmpeg.Models.AudioConfig;
using FFmpegVideoCodec = Tricycle.Media.FFmpeg.Models.VideoCodec;
using FFmpegVideoConfig = Tricycle.Media.FFmpeg.Models.VideoConfig;
using TricycleAudioCodec = Tricycle.Models.Config.AudioCodec;
using TricycleAudioConfig = Tricycle.Models.Config.AudioConfig;
using TricycleVideoCodec = Tricycle.Models.Config.VideoCodec;
using TricycleVideoConfig = Tricycle.Models.Config.VideoConfig;

namespace Tricycle.UI.Tests.ViewModels
{
    [TestClass]
    public class ConfigViewModelTests
    {
        #region Fields

        ConfigViewModel _viewModel;
        IConfigManager<TricycleConfig> _tricycleConfigManager;
        IConfigManager<FFmpegConfig> _ffmpegConfigManager;
        TricycleConfig _tricycleConfig;
        FFmpegConfig _ffmpegConfig;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _tricycleConfigManager = Substitute.For<IConfigManager<TricycleConfig>>();
            _ffmpegConfigManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _viewModel = new ConfigViewModel(_tricycleConfigManager, _ffmpegConfigManager);
            _tricycleConfig = new TricycleConfig()
            {
                Audio = new TricycleAudioConfig(),
                Video = new TricycleVideoConfig()
            };
            _ffmpegConfig = new FFmpegConfig()
            {
                Audio = new FFmpegAudioConfig(),
                Video = new FFmpegVideoConfig()
            };

            _tricycleConfigManager.Config = _tricycleConfig;
            _ffmpegConfigManager.Config = _ffmpegConfig;
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void LoadsAlertOnCompletionFromConfig()
        {
            _tricycleConfig.CompletionAlert = true;
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.CompletionAlert, _viewModel.AlertOnCompletion);
        }

        [TestMethod]
        public void LoadsDeleteIncompleteFilesFromConfig()
        {
            _tricycleConfig.DeleteIncompleteFiles = true;
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.DeleteIncompleteFiles, _viewModel.DeleteIncompleteFiles);
        }

        [TestMethod]
        public void LoadsPreferForcedSubtitlesFromConfig()
        {
            _tricycleConfig.ForcedSubtitlesOnly = true;
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.ForcedSubtitlesOnly, _viewModel.PreferForcedSubtitles);
        }

        [TestMethod]
        public void LoadsMp4FileExtensionFromConfig()
        {
            string extension = "m4v";

            _tricycleConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { ContainerFormat.Mp4, extension }
            };
            _viewModel.Initialize();

            Assert.AreEqual(extension, _viewModel.Mp4FileExtension);
        }

        [TestMethod]
        public void LoadsMkvFileExtensionFromConfig()
        {
            string extension = "mkv2";

            _tricycleConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { ContainerFormat.Mkv, extension }
            };
            _viewModel.Initialize();

            Assert.AreEqual(extension, _viewModel.MkvFileExtension);
        }

        [TestMethod]
        public void LoadsSizeDivisorFromConfig()
        {
            _tricycleConfig.Video.SizeDivisor = 2;
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.Video.SizeDivisor.ToString(), _viewModel.SizeDivisor);
        }

        [TestMethod]
        public void LoadsAvcQualityScaleFromConfig()
        {
            var codec = new TricycleVideoCodec()
            {
                QualityRange = new Range<decimal>(24, 16),
                QualitySteps = 6
            };

            _tricycleConfig.Video.Codecs = new Dictionary<VideoFormat, TricycleVideoCodec>()
            {
                { VideoFormat.Avc, codec }
            };
            _viewModel.Initialize();

            Assert.AreEqual(codec.QualityRange.Min.ToString(), _viewModel.AvcQualityScale?.Min);
            Assert.AreEqual(codec.QualityRange.Max.ToString(), _viewModel.AvcQualityScale?.Max);
            Assert.AreEqual(codec.QualitySteps.ToString(), _viewModel.AvcQualityScale?.StepCount);
        }

        [TestMethod]
        public void LoadsHevcQualityScaleFromConfig()
        {
            var codec = new TricycleVideoCodec()
            {
                QualityRange = new Range<decimal>(24, 16),
                QualitySteps = 6
            };

            _tricycleConfig.Video.Codecs = new Dictionary<VideoFormat, TricycleVideoCodec>()
            {
                { VideoFormat.Hevc, codec }
            };
            _viewModel.Initialize();

            Assert.AreEqual(codec.QualityRange.Min.ToString(), _viewModel.HevcQualityScale?.Min);
            Assert.AreEqual(codec.QualityRange.Max.ToString(), _viewModel.HevcQualityScale?.Max);
            Assert.AreEqual(codec.QualitySteps.ToString(), _viewModel.HevcQualityScale?.StepCount);
        }

        [TestMethod]
        public void LoadsSizePresetsFromConfig()
        {
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "4K", new Dimensions(3840, 2160) },
                { "1080p", new Dimensions(1920, 1080) },
                { "720p", new Dimensions(1280, 720) }
            };
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.Video.SizePresets.Count + 1, _viewModel.SizePresets?.Count);

            int i = 0;

            foreach (var pair in _tricycleConfig.Video.SizePresets)
            {
                var preset = _viewModel.SizePresets?[i];

                Assert.AreEqual(pair.Key, preset?.Name);
                Assert.AreEqual(pair.Value.Width.ToString(), preset?.Width);
                Assert.AreEqual(pair.Value.Height.ToString(), preset?.Height);

                i++;
            }

            var emptyPreset = _viewModel.SizePresets[_tricycleConfig.Video.SizePresets.Count];

            Assert.IsNotNull(emptyPreset);
            Assert.IsNull(emptyPreset.Name);
            Assert.IsNull(emptyPreset.Width);
            Assert.IsNull(emptyPreset.Height);
        }

        [TestMethod]
        public void LoadsAspectRatioPresetsFromConfig()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "21:9", new Dimensions(21, 9) },
                { "16:9", new Dimensions(16, 9) },
                { "4:3", new Dimensions(4, 3) }
            };
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.Video.AspectRatioPresets.Count + 1, _viewModel.AspectRatioPresets?.Count);

            int i = 0;

            foreach (var pair in _tricycleConfig.Video.AspectRatioPresets)
            {
                var preset = _viewModel.AspectRatioPresets?[i];

                Assert.AreEqual(pair.Key, preset?.Name);
                Assert.AreEqual(pair.Value.Width.ToString(), preset?.Width);
                Assert.AreEqual(pair.Value.Height.ToString(), preset?.Height);

                i++;
            }

            var emptyPreset = _viewModel.AspectRatioPresets[_tricycleConfig.Video.AspectRatioPresets.Count];

            Assert.IsNotNull(emptyPreset);
            Assert.IsNull(emptyPreset.Name);
            Assert.IsNull(emptyPreset.Width);
            Assert.IsNull(emptyPreset.Height);
        }

        [TestMethod]
        public void LoadsPassthruMatchingTracksFromConfig()
        {
            _tricycleConfig.Audio.PassthruMatchingTracks = true;
            _viewModel.Initialize();

            Assert.AreEqual(_tricycleConfig.Audio.PassthruMatchingTracks, _viewModel.PassthruMatchingTracks);
        }

        [TestMethod]
        public void LoadsAudioQualityPresetsFromConfig()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, TricycleAudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new TricycleAudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset
                            {
                                Mixdown = AudioMixdown.Stereo,
                                Quality = 160
                            }
                        }
                    }
                },
                {
                    AudioFormat.Ac3,
                    new TricycleAudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset
                            {
                                Mixdown = AudioMixdown.Stereo,
                                Quality = 180
                            },
                            new AudioPreset
                            {
                                Mixdown = AudioMixdown.Surround5dot1,
                                Quality = 640
                            }
                        }
                    }
                }
            };
            _viewModel.Initialize();

            Assert.AreEqual(4, _viewModel.AudioQualityPresets?.Count);

            var preset = _viewModel.AudioQualityPresets[0];

            Assert.AreEqual("AAC", preset?.SelectedFormat?.ToString());
            Assert.AreEqual("Stereo", preset?.SelectedMixdown?.ToString());
            Assert.AreEqual("160", preset?.Quality);

            preset = _viewModel.AudioQualityPresets[1];

            Assert.AreEqual("Dolby Digital", preset?.SelectedFormat?.ToString());
            Assert.AreEqual("Stereo", preset?.SelectedMixdown?.ToString());
            Assert.AreEqual("180", preset?.Quality);

            preset = _viewModel.AudioQualityPresets[2];

            Assert.AreEqual("Dolby Digital", preset?.SelectedFormat?.ToString());
            Assert.AreEqual("Surround", preset?.SelectedMixdown?.ToString());
            Assert.AreEqual("640", preset?.Quality);

            preset = _viewModel.AudioQualityPresets[3];

            Assert.IsNotNull(preset);
            Assert.AreEqual(string.Empty, preset?.SelectedFormat?.ToString());
            Assert.AreEqual(string.Empty, preset?.SelectedMixdown?.ToString());
            Assert.IsNull(preset?.Quality);
        }

        [TestMethod]
        public void LoadsSelectedX264PresetFromConfig()
        {
            string preset = "medium";

            _ffmpegConfig.Video.Codecs = new Dictionary<VideoFormat, FFmpegVideoCodec>()
            {
                {
                    VideoFormat.Avc,
                    new FFmpegVideoCodec()
                    {
                        Preset = preset
                    }
                }
            };
            _viewModel.Initialize();

            Assert.AreEqual(preset, _viewModel.SelectedX264Preset?.ToString());
        }

        [TestMethod]
        public void LoadsSelectedX265PresetFromConfig()
        {
            string preset = "medium";

            _ffmpegConfig.Video.Codecs = new Dictionary<VideoFormat, FFmpegVideoCodec>()
            {
                {
                    VideoFormat.Hevc,
                    new FFmpegVideoCodec()
                    {
                        Preset = preset
                    }
                }
            };
            _viewModel.Initialize();

            Assert.AreEqual(preset, _viewModel.SelectedX265Preset?.ToString());
        }

        [TestMethod]
        public void LoadsAacCodecFromConfig()
        {
            string codec = "aac";

            _ffmpegConfig.Audio.Codecs = new Dictionary<AudioFormat, FFmpegAudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new FFmpegAudioCodec()
                    {
                        Name = codec
                    }
                }
            };
            _viewModel.Initialize();

            Assert.AreEqual(codec, _viewModel.AacCodec);
        }

        [TestMethod]
        public void LoadsAc3CodecFromConfig()
        {
            string codec = "ac3";

            _ffmpegConfig.Audio.Codecs = new Dictionary<AudioFormat, FFmpegAudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new FFmpegAudioCodec()
                    {
                        Name = codec
                    }
                }
            };
            _viewModel.Initialize();

            Assert.AreEqual(codec, _viewModel.Ac3Codec);
        }

        [TestMethod]
        public void LoadsCropDetectOptionsFromConfig()
        {
            _ffmpegConfig.Video.CropDetectOptions = "24:16:0";
            _viewModel.Initialize();

            Assert.AreEqual(_ffmpegConfig.Video.CropDetectOptions, _viewModel.CropDetectOptions);
        }

        [TestMethod]
        public void LoadsDenoiseOptionsFromConfig()
        {
            _ffmpegConfig.Video.DenoiseOptions = "nlmeans";
            _viewModel.Initialize();

            Assert.AreEqual(_ffmpegConfig.Video.DenoiseOptions, _viewModel.DenoiseOptions);
        }

        [TestMethod]
        public void LoadsTonemapOptionsFromConfig()
        {
            _ffmpegConfig.Video.TonemapOptions = "reinhard";
            _viewModel.Initialize();

            Assert.AreEqual(_ffmpegConfig.Video.TonemapOptions, _viewModel.TonemapOptions);
        }

        [TestMethod]
        public void PopulatesFormatOptionsForAudioQualityPresets()
        {
            _viewModel.Initialize();

            Assert.AreEqual(1, _viewModel.AudioQualityPresets?.Count);

            var preset = _viewModel.AudioQualityPresets[0];

            Assert.AreEqual(3, preset?.FormatOptions?.Count);
            Assert.AreEqual(string.Empty, preset?.FormatOptions[0]?.ToString());
            Assert.AreEqual("AAC", preset?.FormatOptions[1]?.ToString());
            Assert.AreEqual("Dolby Digital", preset?.FormatOptions[2]?.ToString());
        }

        [TestMethod]
        public void PopulatesMixdownOptionsForAudioQualityPresets()
        {
            _viewModel.Initialize();

            Assert.AreEqual(1, _viewModel.AudioQualityPresets?.Count);

            var preset = _viewModel.AudioQualityPresets[0];

            Assert.AreEqual(4, preset?.MixdownOptions?.Count);
            Assert.AreEqual(string.Empty, preset?.MixdownOptions[0]?.ToString());
            Assert.AreEqual("Mono", preset?.MixdownOptions[1]?.ToString());
            Assert.AreEqual("Stereo", preset?.MixdownOptions[2]?.ToString());
            Assert.AreEqual("Surround", preset?.MixdownOptions[3]?.ToString());
        }

        [TestMethod]
        public void PopulatesX264PresetOptions()
        {
            Assert.AreEqual(10, _viewModel.X264PresetOptions?.Count);
            Assert.AreEqual(1, _viewModel.X264PresetOptions.Count(o => o?.ToString() == "medium"));
        }

        [TestMethod]
        public void PopulatesX265PresetOptions()
        {
            Assert.AreEqual(10, _viewModel.X265PresetOptions?.Count);
            Assert.AreEqual(1, _viewModel.X265PresetOptions.Count(o => o?.ToString() == "medium"));
        }

        [TestMethod]
        public void SavesAlertOnCompletionToConfig()
        {
            bool alertOnCompletion = true;

            _viewModel.Initialize();
            _viewModel.AlertOnCompletion = alertOnCompletion;
            Complete();

            Assert.AreEqual(alertOnCompletion, _tricycleConfigManager.Config?.CompletionAlert);
        }

        [TestMethod]
        public void SavesDeleteIncompleteFilesToConfig()
        {
            bool deleteIncompleteFiles = true;

            _viewModel.Initialize();
            _viewModel.DeleteIncompleteFiles = deleteIncompleteFiles;
            Complete();

            Assert.AreEqual(deleteIncompleteFiles, _tricycleConfigManager.Config?.DeleteIncompleteFiles);
        }

        [TestMethod]
        public void SavesPreferForcedSubtitlesToConfig()
        {
            bool preferForcedSubtitles = true;

            _viewModel.Initialize();
            _viewModel.PreferForcedSubtitles = preferForcedSubtitles;
            Complete();

            Assert.AreEqual(preferForcedSubtitles, _tricycleConfigManager.Config?.ForcedSubtitlesOnly);
        }

        [TestMethod]
        public void SavesMp4FileExtensionToConfig()
        {
            string extension = "m4v";

            _viewModel.Initialize();
            _viewModel.Mp4FileExtension = extension;
            Complete();

            Assert.AreEqual(extension,
                            _tricycleConfigManager.Config?.DefaultFileExtensions?.GetValueOrDefault(ContainerFormat.Mp4));
        }

        [TestMethod]
        public void SavesMkvFileExtensionToConfig()
        {
            string extension = "mkv2";

            _viewModel.Initialize();
            _viewModel.MkvFileExtension = extension;
            Complete();

            Assert.AreEqual(extension,
                            _tricycleConfigManager.Config?.DefaultFileExtensions?.GetValueOrDefault(ContainerFormat.Mkv));
        }

        [TestMethod]
        public void SavesSizeDivisorToConfig()
        {
            int divisor = 2;

            _viewModel.Initialize();
            _viewModel.SizeDivisor = divisor.ToString();
            Complete();

            Assert.AreEqual(divisor, _tricycleConfigManager.Config?.Video?.SizeDivisor);
        }

        [TestMethod]
        public void SavesAvcQualityScaleToConfig()
        {
            var range = new Range<decimal>(24, 16);
            var steps = 6;

            _viewModel.Initialize();
            _viewModel.AvcQualityScale.Min = range.Min.ToString();
            _viewModel.AvcQualityScale.Max = range.Max.ToString();
            _viewModel.AvcQualityScale.StepCount = steps.ToString();
            Complete();

            var codec = _tricycleConfigManager.Config?.Video?.Codecs?.GetValueOrDefault(VideoFormat.Avc);

            Assert.AreEqual(range, codec?.QualityRange);
            Assert.AreEqual(steps, codec?.QualitySteps);
        }

        [TestMethod]
        public void SavesHevcQualityScaleToConfig()
        {
            var range = new Range<decimal>(24, 16);
            var steps = 6;

            _viewModel.Initialize();
            _viewModel.HevcQualityScale.Min = range.Min.ToString();
            _viewModel.HevcQualityScale.Max = range.Max.ToString();
            _viewModel.HevcQualityScale.StepCount = steps.ToString();
            Complete();

            var codec = _tricycleConfigManager.Config?.Video?.Codecs?.GetValueOrDefault(VideoFormat.Hevc);

            Assert.AreEqual(range, codec?.QualityRange);
            Assert.AreEqual(steps, codec?.QualitySteps);
        }

        [TestMethod]
        public void SavesSizePresetsToConfig()
        {
            _viewModel.Initialize();

            var preset = _viewModel.SizePresets?.Count == 1 ? _viewModel.SizePresets[0] : null;

            if (preset == null)
            {
                Assert.Inconclusive("An empty size preset was not created.");
            }

            var dimensions = new Dimensions(1920, 1080);

            preset.Name = "1080p";
            preset.Width = dimensions.Width.ToString();
            preset.Height = dimensions.Height.ToString();

            Complete();

            var savedPresets = _tricycleConfigManager.Config?.Video?.SizePresets;

            Assert.AreEqual(1, savedPresets?.Count);
            Assert.AreEqual(dimensions, savedPresets.GetValueOrDefault(preset.Name));
        }

        [TestMethod]
        public void SavesAspectRatioPresetsToConfig()
        {
            _viewModel.Initialize();

            var preset = _viewModel.AspectRatioPresets?.Count == 1 ? _viewModel.AspectRatioPresets[0] : null;

            if (preset == null)
            {
                Assert.Inconclusive("An empty aspect ratio preset was not created.");
            }

            var dimensions = new Dimensions(16, 9);

            preset.Name = "16:9";
            preset.Width = dimensions.Width.ToString();
            preset.Height = dimensions.Height.ToString();

            Complete();

            var savedPresets = _tricycleConfigManager.Config?.Video?.AspectRatioPresets;

            Assert.AreEqual(1, savedPresets?.Count);
            Assert.AreEqual(dimensions, savedPresets.GetValueOrDefault(preset.Name));
        }

        [TestMethod]
        public void SavesPassthruMatchingTracksToConfig()
        {
            bool passthruMatchingTracks = true;

            _viewModel.Initialize();
            _viewModel.PassthruMatchingTracks = passthruMatchingTracks;
            Complete();

            Assert.AreEqual(passthruMatchingTracks, _tricycleConfigManager.Config?.Audio?.PassthruMatchingTracks);
        }

        [TestMethod]
        public void SavesAacAudioQualityPresetsToConfig()
        {
            _viewModel.Initialize();

            var preset = _viewModel.AudioQualityPresets?.Count == 1 ? _viewModel.AudioQualityPresets[0] : null;

            if (preset == null)
            {
                Assert.Inconclusive("An empty audio quality preset was not created.");
            }

            var format = AudioFormat.Aac;
            var expectedPreset = new AudioPreset()
            {
                Mixdown = AudioMixdown.Stereo,
                Quality = 160
            };

            preset.SelectedFormat = new ListItem(format);
            preset.SelectedMixdown = new ListItem(expectedPreset.Mixdown);
            preset.Quality = expectedPreset.Quality.ToString();

            Complete();

            var savedPresets = _tricycleConfigManager.Config?.Audio?.Codecs?.GetValueOrDefault(format)?.Presets;

            Assert.AreEqual(1, savedPresets?.Count);
            Assert.AreEqual(expectedPreset.Mixdown, savedPresets[0]?.Mixdown);
            Assert.AreEqual(expectedPreset.Quality, savedPresets[0]?.Quality);
        }

        [TestMethod]
        public void SavesAc3AudioQualityPresetsToConfig()
        {
            _viewModel.Initialize();

            var preset = _viewModel.AudioQualityPresets?.Count == 1 ? _viewModel.AudioQualityPresets[0] : null;

            if (preset == null)
            {
                Assert.Inconclusive("An empty audio quality preset was not created.");
            }

            var format = AudioFormat.Ac3;
            var expectedPreset = new AudioPreset()
            {
                Mixdown = AudioMixdown.Surround5dot1,
                Quality = 640
            };

            preset.SelectedFormat = new ListItem(format);
            preset.SelectedMixdown = new ListItem(expectedPreset.Mixdown);
            preset.Quality = expectedPreset.Quality.ToString();

            Complete();

            var savedPresets = _tricycleConfigManager.Config?.Audio?.Codecs?.GetValueOrDefault(format)?.Presets;

            Assert.AreEqual(1, savedPresets?.Count);
            Assert.AreEqual(expectedPreset.Mixdown, savedPresets[0]?.Mixdown);
            Assert.AreEqual(expectedPreset.Quality, savedPresets[0]?.Quality);
        }

        [TestMethod]
        public void SavesSelectedX264PresetToConfig()
        {
            string preset = "fast";

            _viewModel.Initialize();
            _viewModel.SelectedX264Preset = new ListItem(preset);
            Complete();

            Assert.AreEqual(preset,
                            _ffmpegConfigManager?.Config?.Video?.Codecs?.GetValueOrDefault(VideoFormat.Avc)?.Preset);
        }

        [TestMethod]
        public void SavesSelectedX265PresetToConfig()
        {
            string preset = "slow";

            _viewModel.Initialize();
            _viewModel.SelectedX265Preset = new ListItem(preset);
            Complete();

            Assert.AreEqual(preset,
                            _ffmpegConfigManager?.Config?.Video?.Codecs?.GetValueOrDefault(VideoFormat.Hevc)?.Preset);
        }

        [TestMethod]
        public void SavesAacCodecToConfig()
        {
            string codec = "libfdk_aac";

            _viewModel.Initialize();
            _viewModel.AacCodec = codec;
            Complete();

            Assert.AreEqual(codec,
                            _ffmpegConfigManager?.Config?.Audio?.Codecs?.GetValueOrDefault(AudioFormat.Aac)?.Name);
        }

        [TestMethod]
        public void SavesAc3CodecToConfig()
        {
            string codec = "ac3_fixed";

            _viewModel.Initialize();
            _viewModel.Ac3Codec = codec;
            Complete();

            Assert.AreEqual(codec,
                            _ffmpegConfigManager?.Config?.Audio?.Codecs?.GetValueOrDefault(AudioFormat.Ac3)?.Name);
        }

        [TestMethod]
        public void SavesCropDetectOptionsToConfig()
        {
            string options = "24:16:0";

            _viewModel.Initialize();
            _viewModel.CropDetectOptions = options;
            Complete();

            Assert.AreEqual(options, _ffmpegConfigManager?.Config?.Video?.CropDetectOptions);
        }

        [TestMethod]
        public void SavesDenoiseOptionsToConfig()
        {
            string options = "nlmeans";

            _viewModel.Initialize();
            _viewModel.DenoiseOptions = options;
            Complete();

            Assert.AreEqual(options, _ffmpegConfigManager?.Config?.Video?.DenoiseOptions);
        }

        [TestMethod]
        public void SavesTonemapOptionsToConfig()
        {
            string options = "reinhard";

            _viewModel.Initialize();
            _viewModel.TonemapOptions = options;
            Complete();

            Assert.AreEqual(options, _ffmpegConfigManager?.Config?.Video?.TonemapOptions);
        }

        [TestMethod]
        public void CallsSaveOnTricycleConfigManagerWhenCompleted()
        {
            _viewModel.Initialize();
            _viewModel.AlertOnCompletion = true;
            Complete();

            _tricycleConfigManager.Received().Save();
        }

        [TestMethod]
        public void CallsSaveOnFFmpegConfigManagerWhenCompleted()
        {
            _viewModel.Initialize();
            _viewModel.AacCodec = "aac";
            Complete();

            _ffmpegConfigManager.Received().Save();
        }

        [TestMethod]
        public void RaisesClosedEventWhenCompleted()
        {
            bool closed = false;

            _viewModel.Initialize();
            _viewModel.Closed += () => closed = true;
            Complete();

            Assert.IsTrue(closed);
        }

        [TestMethod]
        public void RaisesClosedEventWhenCancelled()
        {
            bool closed = false;

            _viewModel.Initialize();
            _viewModel.Closed += () => closed = true;
            Cancel();

            Assert.IsTrue(closed);
        }

        [TestMethod]
        public void DoesNotRaiseClosedEventWhenCancelledAndNotConfirmed()
        {
            bool closed = false;

            _viewModel.Initialize();
            _viewModel.AlertOnCompletion = true;
            _viewModel.Confirm += (title, message) => Task.FromResult(false);
            _viewModel.Closed += () => closed = true;
            Cancel();

            Assert.IsFalse(closed);
        }

        [TestMethod]
        public void DoesNotCallSaveOnTricycleConfigManagerWhenCancelled()
        {
            _viewModel.Initialize();
            _viewModel.AlertOnCompletion = true;
            _viewModel.Confirm += (title, message) => Task.FromResult(true);
            Cancel();

            _tricycleConfigManager.DidNotReceive().Save();
        }

        [TestMethod]
        public void DoesNotCallSaveOnFFmpegConfigManagerWhenCancelled()
        {
            _viewModel.Initialize();
            _viewModel.AacCodec = "aac";
            _viewModel.Confirm += (title, message) => Task.FromResult(true);
            Cancel();

            _ffmpegConfigManager.DidNotReceive().Save();
        }

        [TestMethod]
        public void ResetsValuesWhenCancelled()
        {
            _tricycleConfig.CompletionAlert = false;

            _viewModel.Initialize();
            _viewModel.AlertOnCompletion = !_tricycleConfig.CompletionAlert;
            _viewModel.Confirm += (title, message) => Task.FromResult(true);
            Cancel();

            Assert.AreEqual(_tricycleConfig.CompletionAlert, _viewModel.AlertOnCompletion);
        }

        #endregion

        #region Helper Methods

        public void Cancel()
        {
            _viewModel.CancelCommand.Execute(null);
        }

        public void Complete()
        {
            _viewModel.CompleteCommand.Execute(null);
        }

        #endregion
    }
}
