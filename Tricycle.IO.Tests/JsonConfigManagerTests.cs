using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Tricycle.IO.Tests
{
    [TestClass]
    public class JsonConfigManagerTests
    {
        #region Nested Types

        class Config
        {
            public int Value { get; set; }
        }

        class MockJsonConfigManager : JsonConfigManager<Config>
        {
            public bool WasCoalesceCalled { get; set; }
            public Config UserConfig { get; set; }
            public Config DefaultConfig { get; set; }

            public MockJsonConfigManager(IFileSystem fileSystem,
                                         ISerializer<string> serializer,
                                         string defaultFileName,
                                         string userFileName)
                : base(fileSystem, serializer, defaultFileName, userFileName)
            {

            }

            protected override void Coalesce(Config userConfig, Config defaultConfig)
            {
                WasCoalesceCalled = true;
                UserConfig = userConfig;
                DefaultConfig = defaultConfig;
            }
        }

        #endregion

        #region Fields

        IFileSystem _fileSystem;
        IFile _fileService;
        IDirectory _directoryService;
        ISerializer<string> _serializer;
        string _userDirectory;
        string _userFileName;
        string _defaultFileName;
        MockJsonConfigManager _configManager;

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
            _configManager = new MockJsonConfigManager(_fileSystem, _serializer, _defaultFileName, _userFileName);

            _fileSystem.File.Returns(_fileService);
            _fileSystem.Directory.Returns(_directoryService);
            _fileService.Exists(Arg.Any<string>()).Returns(false);
            _directoryService.Exists(Arg.Any<string>()).Returns(true);
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void LoadReadsUserConfigWhenItExists()
        {
            _fileService.Exists(_userFileName).Returns(true);
            _configManager.Load();

            _fileService.Received().ReadAllText(_userFileName);
        }

        [TestMethod]
        public void LoadDoesNotReadUserConfigWhenItDoesNotExist()
        {
            _configManager.Load();

            _fileService.DidNotReceive().ReadAllText(_userFileName);
        }

        [TestMethod]
        public void LoadDeserializesUserConfigWhenItExists()
        {
            var data = Guid.NewGuid().ToString();

            _fileService.Exists(_userFileName).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(data);
            _configManager.Load();

            _serializer.Received().Deserialize<Config>(data);
        }

        [TestMethod]
        public void LoadReadsDefaultConfigWhenItExists()
        {
            _fileService.Exists(_defaultFileName).Returns(true);
            _configManager.Load();

            _fileService.Received().ReadAllText(_defaultFileName);
        }

        [TestMethod]
        public void LoadDoesNotReadDefaultConfigWhenItDoesNotExists()
        {
            _configManager.Load();

            _fileService.DidNotReceive().ReadAllText(_defaultFileName);
        }

        [TestMethod]
        public void LoadDeserializesDefaultConfigWhenItExists()
        {
            var data = Guid.NewGuid().ToString();

            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(data);
            _configManager.Load();

            _serializer.Received().Deserialize<Config>(data);
        }

        [TestMethod]
        public void LoadCreatesUserConfigDirectoryWhenItDoesNotExist()
        {
            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(new Config());
            _directoryService.Exists(_userDirectory).Returns(false);
            _configManager.Load();

            _directoryService.Received().CreateDirectory(_userDirectory);
        }

        [TestMethod]
        public void LoadDoesNotCreateUserConfigDirectoryWhenItExists()
        {
            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(new Config());
            _configManager.Load();

            _directoryService.DidNotReceive().CreateDirectory(Arg.Any<string>());
        }

        [TestMethod]
        public void LoadCallsCoalesceWhenBothConfigsExist()
        {
            _fileService.Exists(_userFileName).Returns(true);
            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(string.Empty);
            _fileService.ReadAllText(_defaultFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(new Config());
            _configManager.Load();

            Assert.IsTrue(_configManager.WasCoalesceCalled);
            Assert.IsNotNull(_configManager.UserConfig);
            Assert.IsNotNull(_configManager.DefaultConfig);
        }

        [TestMethod]
        public void LoadDoesNotCallCoalesceWhenUserConfigDoesNotExist()
        {
            _fileService.Exists(_userFileName).Returns(false);
            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(new Config());
            _configManager.Load();

            Assert.IsFalse(_configManager.WasCoalesceCalled);
        }

        [TestMethod]
        public void LoadDoesNotCallCoalesceWhenDefaultConfigDoesNotExist()
        {
            _fileService.Exists(_userFileName).Returns(true);
            _fileService.Exists(_defaultFileName).Returns(false);
            _fileService.ReadAllText(_userFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(new Config());
            _configManager.Load();

            Assert.IsFalse(_configManager.WasCoalesceCalled);
        }

        [TestMethod]
        public void LoadDeserializesUserConfigWhenItDoesNotExist()
        {
            var config = new Config()
            {
                Value = 2
            };

            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(config);
            _configManager.Load();

            _serializer.Received().Serialize(config);
        }

        [TestMethod]
        public void LoadWritesUserConfigWhenItDoesNotExist()
        {
            var data = Guid.NewGuid().ToString();

            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(data);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(new Config());
            _serializer.Serialize(Arg.Any<object>()).Returns(data);
            _configManager.Load();

            _fileService.Received().WriteAllText(_userFileName, data);
        }

        [TestMethod]
        public void LoadSetsConfig()
        {
            var config = new Config()
            {
                Value = 2
            };

            _fileService.Exists(_userFileName).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(string.Empty);
            _serializer.Deserialize<Config>(Arg.Any<string>()).Returns(config);
            _configManager.Load();

            Assert.AreEqual(2, _configManager.Config?.Value);
        }

        [TestMethod]
        public void SettingConfigRaisesConfigChanged()
        {
            var expected = new Config()
            {
                Value = 2
            };
            Config actual = null;

            _configManager.ConfigChanged += c =>
            {
                actual = c;
            };
            _configManager.Config = expected;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SaveCreatesUserDirectoryWhenItDoesNotExist()
        {
            _configManager.Config = new Config()
            {
                Value = 2
            };
            _directoryService.Exists(_userDirectory).Returns(false);
            _configManager.Save();

            _directoryService.Received().CreateDirectory(_userDirectory);
        }

        [TestMethod]
        public void SaveDoesNotCreateUserDirectoryWhenItExists()
        {
            _configManager.Config = new Config()
            {
                Value = 2
            };
            _configManager.Save();

            _directoryService.DidNotReceive().CreateDirectory(Arg.Any<string>());
        }

        [TestMethod]
        public void SaveSerializesConfig()
        {
            _configManager.Config = new Config()
            {
                Value = 2
            };
            _configManager.Save();

            _serializer.Received().Serialize(_configManager.Config);
        }

        [TestMethod]
        public void SaveWritesToUserConfigFile()
        {
            var data = Guid.NewGuid().ToString();

            _configManager.Config = new Config();
            _serializer.Serialize(Arg.Any<Config>()).Returns(data);
            _configManager.Save();

            _fileService.Received().WriteAllText(_userFileName, data);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SaveThrowsExceptionWhenConfigIsNotSet()
        {
            _configManager.Save();
        }

        #endregion
    }
}
