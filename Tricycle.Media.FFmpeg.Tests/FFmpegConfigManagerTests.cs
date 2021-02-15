using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;

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
        FFmpegConfig _coalescedConfig;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _fileService = Substitute.For<IFile>();
            _directoryService = Substitute.For<IDirectory>();
            _serializer = Substitute.For<ISerializer<string>>();
            _userDirectory = Path.Combine("Users", "fred", "Library", "Preferences");
            _userFileName = Path.Combine(_userDirectory, "config.json");
            _defaultFileName = Path.Combine("Applications", "Tricycle.app", "Resources", "Config", "config.json");
            _configManager = new FFmpegConfigManager(_fileSystem, _serializer, _defaultFileName, _userFileName);

            _userConfig = new FFmpegConfig();
            _defaultConfig = new FFmpegConfig();

            _fileSystem.File.Returns(_fileService);
            _fileSystem.Directory.Returns(_directoryService);
            _fileService.Exists(Arg.Any<string>()).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(JsonConvert.SerializeObject(_userConfig));
            _fileService.ReadAllText(_defaultFileName).Returns(JsonConvert.SerializeObject(_defaultConfig));
            _fileService.When(x => x.WriteAllText(_userFileName, Arg.Any<string>()))
                        .Do(x => _coalescedConfig = JsonConvert.DeserializeObject<FFmpegConfig>(x[1].ToString()));
            _directoryService.Exists(Arg.Any<string>()).Returns(true);
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void CoalesceCopiesAudioWhenNull()
        {
            _defaultConfig.Audio = new AudioConfig();
            _configManager.Load();

            Assert.IsNotNull(_coalescedConfig.Audio);
        }

        #endregion
    }
}
