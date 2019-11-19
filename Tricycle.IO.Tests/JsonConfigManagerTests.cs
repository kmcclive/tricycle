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

        #endregion

        #region Fields

        IFileSystem _fileSystem;
        IFile _fileService;
        string _userFileName;
        string _defaultFileName;
        JsonConfigManager<Config> _configManager;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _fileService = Substitute.For<IFile>();
            _userFileName = Path.Combine("Users", "fred", "Preferences", "config.json");
            _defaultFileName = Path.Combine("Applications", "Tricycle.app", "Resources", "Config", "config.json");
            _configManager = new JsonConfigManager<Config>(_fileSystem, _defaultFileName, _userFileName);

            _fileSystem.File.Returns(_fileService);
            _fileService.Exists(Arg.Any<string>()).Returns(false);
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
        public void LoadDoesNotReadDefaultConfigWhenUserConfigExists()
        {
            _fileService.Exists(_userFileName).Returns(true);
            _configManager.Load();

            _fileService.DidNotReceive().ReadAllText(_defaultFileName);
        }

        [TestMethod]
        public void LoadReadsDefaultConfigWhenUserConfigDoesNotExist()
        {
            _fileService.Exists(_defaultFileName).Returns(true);
            _configManager.Load();

            _fileService.Received().ReadAllText(_defaultFileName);
        }

        [TestMethod]
        public void LoadWritesUserConfigWhenItDoesNotExist()
        {
            var config =
                "{" + Environment.NewLine +
                "  \"value\": 2" + Environment.NewLine +
                "}";

            _fileService.Exists(_defaultFileName).Returns(true);
            _fileService.ReadAllText(_defaultFileName).Returns(config);
            _configManager.Load();

            _fileService.Received().WriteAllText(_userFileName, config);
        }

        [TestMethod]
        public void LoadSetsConfig()
        {
            var config =
                "{" + Environment.NewLine +
                "  \"value\": 2" + Environment.NewLine +
                "}";

            _fileService.Exists(_userFileName).Returns(true);
            _fileService.ReadAllText(_userFileName).Returns(config);
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
        public void SaveWritesToUserConfigFile()
        {
            var json =
                "{" + Environment.NewLine +
                "  \"value\": 2" + Environment.NewLine +
                "}";

            _configManager.Config = new Config()
            {
                Value = 2
            };
            _configManager.Save();

            _fileService.Received().WriteAllText(_userFileName, json);
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
