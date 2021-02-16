using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Models;
using Tricycle.Utilities;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class FFmpegConfigManagerTests
    {

        #region Fields

        IFileSystem _fileSystem;
        IFile _fileService;
        IDirectory _directoryService;
        ISerializer<string> _serializer;
        string _userDirectory;
        string _userFileName;
        string _defaultFileName;
        FFmpegConfigManager _configManager;
        FFmpegConfig _userConfig;
        FFmpegConfig _defaultConfig;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _fileService = Substitute.For<IFile>();
            _directoryService = Substitute.For<IDirectory>();
            _serializer = Substitute.For<ISerializer<string>>();
            _userDirectory = Guid.NewGuid().ToString();
            _userFileName = Guid.NewGuid().ToString();
            _defaultFileName = Guid.NewGuid().ToString();
            _configManager = new FFmpegConfigManager(_fileSystem, _serializer, _defaultFileName, _userFileName);

            _userConfig = new FFmpegConfig();
            _defaultConfig = new FFmpegConfig();

            var userText = Guid.NewGuid().ToString();
            var defaultText = Guid.NewGuid().ToString();

            _fileSystem.File.Returns(_fileService);
            _fileSystem.Directory.Returns(_directoryService);
            _fileService.Exists(Arg.Any<string>()).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(userText);
            _fileService.ReadAllText(_defaultFileName).Returns(defaultText);
            _directoryService.Exists(Arg.Any<string>()).Returns(true);
            _serializer.Deserialize<FFmpegConfig>(userText).Returns(_userConfig);
            _serializer.Deserialize<FFmpegConfig>(defaultText).Returns(_defaultConfig);
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void CoalesceCopiesAudioWhenNull()
        {
            _defaultConfig.Audio = new AudioConfig();

            _configManager.Load();

            Assert.IsNotNull(_configManager.Config?.Audio);
        }

        [TestMethod]
        public void CoalesceCopiesAudioCodecsWhenEmpty()
        {
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { AudioFormat.Aac, new AudioCodec() }
                }
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Audio.Codecs.Count, _configManager.Config?.Audio?.Codecs?.Count);
        }

        [TestMethod]
        public void CoalesceCopiesAudioCodecNameWhenEmpty()
        {
            var format = AudioFormat.Aac;
            var name = "aac";
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec(string.Empty) }
                }
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec(name) },
                    { AudioFormat.Ac3, new AudioCodec("ac3") }
                }
            };

            _configManager.Load();

            Assert.AreEqual(name, _configManager.Config?.Audio?.Codecs?.GetValueOrDefault(format)?.Name);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyAudioCodecNameWhenNotEmpty()
        {
            var format = AudioFormat.Aac;
            var name = "aac";
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec(name) }
                }
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec("aac") }
                }
            };

            _configManager.Load();

            Assert.AreEqual(name, _configManager.Config?.Audio?.Codecs?.GetValueOrDefault(format)?.Name);
        }

        [TestMethod]
        public void CoalesceDoesNotThrowExceptionWhenDefaultAudioCodecIsMissing()
        {
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { AudioFormat.Aac, new AudioCodec("aac") }
                }
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { AudioFormat.Ac3, new AudioCodec("ac3") }
                }
            };

            _configManager.Load();
        }

        [TestMethod]
        public void CoalesceCopiesVideoWhenNull()
        {
            _defaultConfig.Video = new VideoConfig();

            _configManager.Load();

            Assert.IsNotNull(_configManager.Config?.Video);
        }

        [TestMethod]
        public void CoalesceCopiesVideoCodecsWhenEmpty()
        {
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Avc, new VideoCodec() }
                }
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.Codecs.Count, _configManager.Config?.Video?.Codecs?.Count);
        }

        [TestMethod]
        public void CoalesceCopiesVideoCodecPresetWhenEmpty()
        {
            var format = VideoFormat.Avc;
            var preset = "medium";
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec(string.Empty) }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec(preset) },
                    { VideoFormat.Hevc, new VideoCodec("fast") }
                }
            };

            _configManager.Load();

            Assert.AreEqual(preset, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.Preset);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyVideoCodecPresetWhenNotEmpty()
        {
            var format = VideoFormat.Avc;
            var preset = "medium";
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec(preset) }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec("fast") },
                }
            };

            _configManager.Load();

            Assert.AreEqual(preset, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.Preset);
        }

        [TestMethod]
        public void CoalesceDoesNotThrowExceptionWhenDefaultVideoCodecIsMissing()
        {
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Avc, new VideoCodec("medium") }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Hevc, new VideoCodec("fast") },
                }
            };

            _configManager.Load();
        }

        [TestMethod]
        public void CoalesceCopiesCropDetectOptionsWhenEmpty()
        {
            _userConfig.Video = new VideoConfig()
            {
                CropDetectOptions = string.Empty
            };
            _defaultConfig.Video = new VideoConfig()
            {
                CropDetectOptions = "0.125:2:0"
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.CropDetectOptions, _configManager.Config?.Video?.CropDetectOptions);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyCropDetectOptionsWhenNotEmpty()
        {
            var cropDetectOptions = "0.125:2:0";
            _userConfig.Video = new VideoConfig()
            {
                CropDetectOptions = cropDetectOptions
            };
            _defaultConfig.Video = new VideoConfig()
            {
                CropDetectOptions = "0.1:2:0"
            };

            _configManager.Load();

            Assert.AreEqual(cropDetectOptions, _configManager.Config?.Video?.CropDetectOptions);
        }

        [TestMethod]
        public void CoalesceCopiesDeinterlaceOptionsWhenEmpty()
        {
            _userConfig.Video = new VideoConfig()
            {
                DeinterlaceOptions = string.Empty
            };
            _defaultConfig.Video = new VideoConfig()
            {
                DeinterlaceOptions = "bwdif"
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.DeinterlaceOptions, _configManager.Config?.Video?.DeinterlaceOptions);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyDeinterlaceOptionsWhenNotEmpty()
        {
            var deinterlaceOptions = "bwdif";
            _userConfig.Video = new VideoConfig()
            {
                DeinterlaceOptions = deinterlaceOptions
            };
            _defaultConfig.Video = new VideoConfig()
            {
                DeinterlaceOptions = "yadif"
            };

            _configManager.Load();

            Assert.AreEqual(deinterlaceOptions, _configManager.Config?.Video?.DeinterlaceOptions);
        }

        [TestMethod]
        public void CoalesceCopiesDenoiseOptionsWhenEmpty()
        {
            _userConfig.Video = new VideoConfig()
            {
                DenoiseOptions = string.Empty
            };
            _defaultConfig.Video = new VideoConfig()
            {
                DenoiseOptions = "hqdn3d"
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.DenoiseOptions, _configManager.Config?.Video?.DenoiseOptions);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyDenoiseOptionsWhenNotEmpty()
        {
            var denoiseOptions = "hqdn3d=1:2:3:4";
            _userConfig.Video = new VideoConfig()
            {
                DenoiseOptions = denoiseOptions
            };
            _defaultConfig.Video = new VideoConfig()
            {
                DenoiseOptions = "hqdn3d=4:4:3:3"
            };

            _configManager.Load();

            Assert.AreEqual(denoiseOptions, _configManager.Config?.Video?.DenoiseOptions);
        }

        [TestMethod]
        public void CoalesceCopiesTonemapOptionsWhenEmpty()
        {
            _userConfig.Video = new VideoConfig()
            {
                TonemapOptions = string.Empty
            };
            _defaultConfig.Video = new VideoConfig()
            {
                TonemapOptions = "hable"
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.TonemapOptions, _configManager.Config?.Video?.TonemapOptions);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyTonemapOptionsWhenNotEmpty()
        {
            var tonemapOptions = "reinhard";
            _userConfig.Video = new VideoConfig()
            {
                TonemapOptions = tonemapOptions
            };
            _defaultConfig.Video = new VideoConfig()
            {
                TonemapOptions = "hable"
            };

            _configManager.Load();

            Assert.AreEqual(tonemapOptions, _configManager.Config?.Video?.TonemapOptions);
        }

        #endregion
    }
}
