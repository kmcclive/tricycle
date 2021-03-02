using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.IO;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Utilities;

namespace Tricycle.UI.Tests
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
        TricycleConfigManager _configManager;
        TricycleConfig _userConfig;
        TricycleConfig _defaultConfig;

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
            _configManager = new TricycleConfigManager(_fileSystem, _serializer, _defaultFileName, _userFileName);

            _userConfig = new TricycleConfig();
            _defaultConfig = new TricycleConfig();

            var userText = Guid.NewGuid().ToString();
            var defaultText = Guid.NewGuid().ToString();

            _fileSystem.File.Returns(_fileService);
            _fileSystem.Directory.Returns(_directoryService);
            _fileService.Exists(Arg.Any<string>()).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(userText);
            _fileService.ReadAllText(_defaultFileName).Returns(defaultText);
            _directoryService.Exists(Arg.Any<string>()).Returns(true);
            _serializer.Deserialize<TricycleConfig>(userText).Returns(_userConfig);
            _serializer.Deserialize<TricycleConfig>(defaultText).Returns(_defaultConfig);
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
        public void CoalesceCopiesAudioCodecTagWhenEmpty()
        {
            var format = AudioFormat.Aac;
            var tag = "aac";
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec() }
                }
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec(){ Tag = tag } },
                    { AudioFormat.Ac3, new AudioCodec(){ Tag = "ac3" } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(tag, _configManager.Config?.Audio?.Codecs?.GetValueOrDefault(format)?.Tag);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyAudioCodecNameWhenNotEmpty()
        {
            var format = AudioFormat.Aac;
            var tag = "aac";
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec(){ Tag = tag } }
                }
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { format, new AudioCodec(){ Tag = "aac2" } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(tag, _configManager.Config?.Audio?.Codecs?.GetValueOrDefault(format)?.Tag);
        }

        [TestMethod]
        public void CoalesceDoesNotThrowExceptionWhenDefaultAudioCodecIsMissing()
        {
            _userConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { AudioFormat.Aac, new AudioCodec() }
                }
            };
            _defaultConfig.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { AudioFormat.Ac3, new AudioCodec() }
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
        public void CoalesceCopiesVideoCodecTagWhenEmpty()
        {
            var format = VideoFormat.Hevc;
            var tag = "hvc1";
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { Tag = tag } },
                    { VideoFormat.Avc, new VideoCodec() { Tag = "avc" } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(tag, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.Tag);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyVideoCodecTagWhenNotEmpty()
        {
            var format = VideoFormat.Hevc;
            var tag = "hvc1";
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { Tag = tag } }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { Tag = "hev1" } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(tag, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.Tag);
        }

        [TestMethod]
        public void CoalesceDoesNotThrowExceptionWhenDefaultVideoCodecIsMissing()
        {
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Hevc, new VideoCodec() }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Avc, new VideoCodec() },
                }
            };

            _configManager.Load();
        }

        [TestMethod]
        public void CoalesceCopiesVideoCodecQualityStepsWhenLessThan1()
        {
            var format = VideoFormat.Hevc;
            var qualitySteps = 7;
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualitySteps = qualitySteps } },
                    { VideoFormat.Avc, new VideoCodec() { QualitySteps = 3 } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(qualitySteps, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.QualitySteps);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyVideoCodecQualityStepsWhenGreaterThanOrEqualTo1()
        {
            var format = VideoFormat.Hevc;
            var qualitySteps = 1;
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualitySteps = qualitySteps } }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualitySteps = 3 } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(qualitySteps, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.QualitySteps);
        }

        [TestMethod]
        public void CoalesceCopiesVideoCodecQualityMinWhenNull()
        {
            var format = VideoFormat.Hevc;
            var qualityMin = 22;
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(null, 18) } }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(qualityMin, 18) } },
                    { VideoFormat.Avc, new VideoCodec() { QualityRange = new Range<decimal>(20, 18) } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(qualityMin, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.QualityRange.Min);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyVideoCodecQualityMinWhenNotNull()
        {
            var format = VideoFormat.Hevc;
            var qualityMin = 22;
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(qualityMin, 18) } }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(24, 18) } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(qualityMin, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.QualityRange.Min);
        }

        [TestMethod]
        public void CoalesceCopiesVideoCodecQualityMaxWhenNull()
        {
            var format = VideoFormat.Hevc;
            var qualityMax = 18;
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(22, null) } }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(22, qualityMax) } },
                    { VideoFormat.Avc, new VideoCodec() { QualityRange = new Range<decimal>(20, 16) } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(qualityMax, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.QualityRange.Max);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyVideoCodecQualityMaxWhenNotNull()
        {
            var format = VideoFormat.Hevc;
            var qualityMax = 16;
            _userConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(22, qualityMax) } }
                }
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { format, new VideoCodec() { QualityRange = new Range<decimal>(22, 18) } }
                }
            };

            _configManager.Load();

            Assert.AreEqual(qualityMax, _configManager.Config?.Video?.Codecs?.GetValueOrDefault(format)?.QualityRange.Max);
        }

        [TestMethod]
        public void CoalesceCopiesSizeDivisorWhenLessThan1()
        {
            _userConfig.Video = new VideoConfig();
            _defaultConfig.Video = new VideoConfig()
            {
                SizeDivisor = 8
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.SizeDivisor, _configManager.Config?.Video?.SizeDivisor);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyCropDetectOptionsWhenNotEmpty()
        {
            var sizeDivisor = 16;
            _userConfig.Video = new VideoConfig()
            {
                SizeDivisor = sizeDivisor
            };
            _defaultConfig.Video = new VideoConfig()
            {
                SizeDivisor = 8
            };

            _configManager.Load();

            Assert.AreEqual(sizeDivisor, _configManager.Config?.Video?.SizeDivisor);
        }

        [TestMethod]
        public void CoalesceCopiesDeinterlaceWhenNotDefined()
        {
            _userConfig.Video = new VideoConfig()
            {
                Deinterlace = (SmartSwitchOption)10
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Deinterlace = SmartSwitchOption.On
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.Video.Deinterlace, _configManager.Config?.Video?.Deinterlace);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyDeinterlaceWhenDefined()
        {
            var deinterlace = SmartSwitchOption.Off;
            _userConfig.Video = new VideoConfig()
            {
                Deinterlace = deinterlace
            };
            _defaultConfig.Video = new VideoConfig()
            {
                Deinterlace = SmartSwitchOption.On
            };

            _configManager.Load();

            Assert.AreEqual(deinterlace, _configManager.Config?.Video?.Deinterlace);
        }

        [TestMethod]
        public void CoalesceCopiesDestinationDirectoryModeWhenNotDefined()
        {
            _userConfig.DestinationDirectoryMode = (AutomationMode)10;
            _defaultConfig.DestinationDirectoryMode = AutomationMode.Auto;

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.DestinationDirectoryMode, _configManager.Config?.DestinationDirectoryMode);
        }

        [TestMethod]
        public void CoalesceDoesNotCopyDestinationDirectoryModeWhenDefined()
        {
            var mode = AutomationMode.Manual;
            _userConfig.DestinationDirectoryMode = mode;
            _defaultConfig.DestinationDirectoryMode = AutomationMode.Auto;

            _configManager.Load();

            Assert.AreEqual(mode, _configManager.Config?.DestinationDirectoryMode);
        }

        [TestMethod]
        public void CoalesceCopiesDefaultFileExtensionsWhenEmpty()
        {
            _userConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>();
            _defaultConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { ContainerFormat.Mkv, "mkv" }
            };

            _configManager.Load();

            Assert.AreEqual(_defaultConfig.DefaultFileExtensions.Count, _configManager.Config?.DefaultFileExtensions?.Count);
        }

        [TestMethod]
        public void CoalesceCopiesDefaultFileExtensionWhenEmpty()
        {
            var format = ContainerFormat.Mkv;
            var extension = "mkv";
            _userConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { format, string.Empty }
            };
            _defaultConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { format, extension },
                { ContainerFormat.Mp4, "mp4" }
            };

            _configManager.Load();

            Assert.AreEqual(extension, _configManager.Config?.DefaultFileExtensions?.GetValueOrDefault(format));
        }

        [TestMethod]
        public void CoalesceDoesNotCopyDefaultFileExtensionWhenNotEmpty()
        {
            var format = ContainerFormat.Mp4;
            var extension = "m4v";
            _userConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { format, extension }
            };
            _defaultConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { format, "mp4" }
            };

            _configManager.Load();

            Assert.AreEqual(extension, _configManager.Config?.DefaultFileExtensions?.GetValueOrDefault(format));
        }

        #endregion
    }
}
