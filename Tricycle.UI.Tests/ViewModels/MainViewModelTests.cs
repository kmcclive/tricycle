using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.IO;
using Tricycle.IO.Models;
using Tricycle.Media;
using Tricycle.Media.Models;
using Tricycle.UI.Models;
using Tricycle.UI.ViewModels;

namespace Tricycle.UI.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        #region Fields

        MainViewModel _viewModel;
        IFileBrowser _fileBrowser;
        FileBrowserResult _fileBrowserResult;
        IMediaInspector _mediaInspector;
        VideoStreamInfo _videoStream;
        MediaInfo _mediaInfo;
        ICropDetector _cropDetector;
        IFileSystem _fileSystem;
        IFile _fileService;
        TricycleConfig _tricycleConfig;
        string _defaultDestinationDirectory;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _fileBrowser = Substitute.For<IFileBrowser>();
            _mediaInspector = Substitute.For<IMediaInspector>();
            _cropDetector = Substitute.For<ICropDetector>();
            _fileSystem = Substitute.For<IFileSystem>();
            _tricycleConfig = new TricycleConfig();
            _defaultDestinationDirectory = Path.Combine("Users", "fred", "Movies");
            _viewModel = new MainViewModel(_fileBrowser,
                                           _mediaInspector,
                                           _cropDetector,
                                           _fileSystem,
                                           _tricycleConfig,
                                           _defaultDestinationDirectory);
            _fileBrowserResult = new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            };
            _videoStream = new VideoStreamInfo();
            _mediaInfo = new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    _videoStream
                }
            };
            _fileService = Substitute.For<IFile>();

            _fileBrowser.BrowseToOpen().Returns(_fileBrowserResult);
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(_mediaInfo);
            _fileSystem.File.Returns(_fileService);
            _fileService.Exists(Arg.Any<string>()).Returns(false);
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void DisablesVideoConfigInitially()
        {
            Assert.IsFalse(_viewModel.IsVideoConfigEnabled);
        }

        [TestMethod]
        public void DisablesHdrInitially()
        {
            Assert.IsFalse(_viewModel.IsHdrEnabled);
        }

        [TestMethod]
        public void ListsNoAudioOutputsInitially()
        {
            Assert.IsTrue(_viewModel.AudioOutputs?.Any() != true);
        }

        [TestMethod]
        public void DisablesContainerFormatInitially()
        {
            Assert.IsFalse(_viewModel.IsContainerFormatEnabled);
        }

        [TestMethod]
        public void DisablesDestinationSelectionInitially()
        {
            Assert.IsFalse(_viewModel.DestinationSelectCommand.CanExecute(null));
        }

        [TestMethod]
        public void DisablesStartInitially()
        {
            Assert.IsFalse(_viewModel.StartCommand.CanExecute(null));
        }

        [TestMethod]
        public void HidesProgressInitially()
        {
            Assert.IsFalse(_viewModel.IsProgressVisible);
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.ProgressText));
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.RateText));
        }

        [TestMethod]
        public void EnablesSourceSelectionInitially()
        {
            Assert.IsTrue(_viewModel.SourceSelectCommand.CanExecute(null));
        }

        [TestMethod]
        public void OpensFileBrowserWhenSelectingSource()
        {
            SelectSource();
            _fileBrowser.Received().BrowseToOpen();
        }

        [TestMethod]
        public void DoesNotReadSourceForCancelledFileBrowser()
        {
            _fileBrowserResult.Confirmed = false;
            SelectSource();
            _mediaInspector.DidNotReceive().Inspect(Arg.Any<string>());
        }

        [TestMethod]
        public void ReadsCorrectSourceForConfirmedFileBrowser()
        {
            string fileName = "test.mkv";

            _fileBrowserResult.FileName = fileName;
            SelectSource();
            _mediaInspector.Received().Inspect(fileName);
        }

        [TestMethod]
        public void DisplaysAlertWhenSourceInfoIsNull()
        {
            string actualTitle = null;
            string actualMessage = null;

            _mediaInspector.Inspect(Arg.Any<string>()).Returns(default(MediaInfo));
            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            SelectSource();

            Assert.AreEqual("Invalid Source", actualTitle);
            Assert.AreEqual("The selected file could not be opened.", actualMessage);
        }

        [TestMethod]
        public void DisplaysAlertWhenSourceHasNoVideo()
        {
            string actualTitle = null;
            string actualMessage = null;

            _mediaInfo.Streams = new StreamInfo[] { new AudioStreamInfo() };
            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            SelectSource();

            Assert.AreEqual("Invalid Source", actualTitle);
            Assert.AreEqual("The selected file could not be opened.", actualMessage);
        }

        [TestMethod]
        public void ShowsSourceInfoWhenSourceIsValid()
        {
            SelectSource();

            Assert.IsTrue(_viewModel.IsSourceInfoVisible);
        }

        [TestMethod]
        public void DisplaysCorrectSourceName()
        {
            var fileName = "test.mkv";

            _fileBrowserResult.FileName = fileName;
            _mediaInfo.FileName = fileName;
            SelectSource();

            Assert.AreEqual(fileName, _viewModel.SourceName);
        }

        [TestMethod]
        public void DisplaysCorrectSourceDuration()
        {
            _mediaInfo.Duration = new TimeSpan(1, 42, 17);
            SelectSource();

            Assert.AreEqual("01:42:17", _viewModel.SourceDuration);
        }

        [TestMethod]
        public void Displays4KSourceSizeFor3840Width()
        {
            _videoStream.Dimensions = new Dimensions(3840, 1646);
            SelectSource();

            Assert.AreEqual("4K", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays4KSourceSizeFor2160Height()
        {
            _videoStream.Dimensions = new Dimensions(2880, 2160);
            SelectSource();

            Assert.AreEqual("4K", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays1080pSourceSizeFor1920Width()
        {
            _videoStream.Dimensions = new Dimensions(1920, 822);
            SelectSource();

            Assert.AreEqual("1080p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays1080pSourceSizeFor1080Height()
        {
            _videoStream.Dimensions = new Dimensions(1440, 1080);
            SelectSource();

            Assert.AreEqual("1080p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays720pSourceSizeFor1280Width()
        {
            _videoStream.Dimensions = new Dimensions(1280, 548);
            SelectSource();

            Assert.AreEqual("720p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays720pSourceSizeFor720Height()
        {
            _videoStream.Dimensions = new Dimensions(960, 720);
            SelectSource();

            Assert.AreEqual("720p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays480pSourceSizeFor853Width()
        {
            _videoStream.Dimensions = new Dimensions(853, 366);
            SelectSource();

            Assert.AreEqual("480p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays480pSourceSizeFor480Height()
        {
            _videoStream.Dimensions = new Dimensions(640, 480);
            SelectSource();

            Assert.AreEqual("480p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void DisplaysCustomSourceSize()
        {
            _videoStream.Dimensions = new Dimensions(568, 320);
            SelectSource();

            Assert.AreEqual("320p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void ShowsHdrLabelForHdr()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();

            Assert.IsTrue(_viewModel.IsSourceHdr);
        }

        [TestMethod]
        public void HidesHdrLabelForSdr()
        {
            _videoStream.DynamicRange = DynamicRange.Standard;
            SelectSource();

            Assert.IsFalse(_viewModel.IsSourceHdr);
        }

        [TestMethod]
        public void PopulatesContainerFormatOptions()
        {
            SelectSource();

            Assert.AreEqual(2, _viewModel.ContainerFormatOptions?.Count);
            Assert.AreEqual("MP4", _viewModel.ContainerFormatOptions[0].Name);
            Assert.AreEqual("MKV", _viewModel.ContainerFormatOptions[1].Name);
        }

        [TestMethod]
        public void SelectsMp4ContainerFormatByDefault()
        {
            SelectSource();

            Assert.AreEqual("MP4", _viewModel.SelectedContainerFormat.Name);
        }

        [TestMethod]
        public void SetsDefaultDestinationName()
        {
            var fileName = Path.Combine("Volumes", "Media", "test.mkv");

            _tricycleConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { ContainerFormat.Mp4, "m4v" }
            };
            _fileBrowserResult.FileName = fileName;
            _mediaInfo.FileName = fileName;
            SelectSource();

            Assert.AreEqual(Path.Combine(_defaultDestinationDirectory, "test.m4v"), _viewModel.DestinationName);
        }

        [TestMethod]
        public void IncrementsDestinationNameWhenFileExists()
        {
            var sourceFileName = Path.Combine("Volumes", "Media", "test.mkv");

            _tricycleConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { ContainerFormat.Mp4, "m4v" }
            };
            _fileBrowserResult.FileName = sourceFileName;
            _mediaInfo.FileName = sourceFileName;
            _fileService.Exists(Path.Combine(_defaultDestinationDirectory, "test.m4v")).Returns(true);
            _fileService.Exists(Path.Combine(_defaultDestinationDirectory, "test 2.m4v")).Returns(true);
            SelectSource();

            Assert.AreEqual(Path.Combine(_defaultDestinationDirectory, "test 3.m4v"), _viewModel.DestinationName);
        }

        [TestMethod]
        public void UpdatesDestinationExtensionWhenContainerFormatChanges()
        {
            var sourceFileName = Path.Combine("Volumes", "Media", "test.m2ts");

            _tricycleConfig.DefaultFileExtensions = new Dictionary<ContainerFormat, string>()
            {
                { ContainerFormat.Mkv, "mkv" }
            };
            _fileBrowserResult.FileName = sourceFileName;
            _mediaInfo.FileName = sourceFileName;
            SelectSource();
            _viewModel.SelectedContainerFormat = new ListItem("MKV", ContainerFormat.Mkv);

            Assert.AreEqual(Path.Combine(_defaultDestinationDirectory, "test.mkv"), _viewModel.DestinationName);
        }

        [TestMethod]
        public void EnablesDestinationSelectionWhenSourceIsValid()
        {
            SelectSource();

            Assert.IsTrue(_viewModel.DestinationSelectCommand.CanExecute(null));
        }

        [TestMethod]
        public void OpensFileBrowserWhenSelectingDestination()
        {
            SelectSource();
            _fileBrowser.BrowseToSave(Arg.Any<string>(), Arg.Any<string>()).Returns(new FileBrowserResult());
            SelectDestination();

            _fileBrowser.Received().BrowseToSave(Arg.Any<string>(), Arg.Any<string>());
        }

        [TestMethod]
        public void SetsDefaultLocationForDestinationFileBrowser()
        {
            var fileName = Path.Combine("Volumes", "Media", "test.mkv");

            _fileBrowserResult.FileName = fileName;
            _mediaInfo.FileName = fileName;
            SelectSource();
            _fileBrowser.BrowseToSave(Arg.Any<string>(), Arg.Any<string>()).Returns(new FileBrowserResult());
            SelectDestination();

            _fileBrowser.Received().BrowseToSave(_defaultDestinationDirectory, "test.mp4");
        }

        [TestMethod]
        public void DoesNotChangeDestinationNameForCancelledFileBrowser()
        {
            SelectSource();

            var oldDestination = _viewModel.DestinationName;

            _fileBrowser.BrowseToSave(Arg.Any<string>(), Arg.Any<string>()).Returns(new FileBrowserResult()
            {
                Confirmed = false
            });
            SelectDestination();

            Assert.AreEqual(oldDestination, _viewModel.DestinationName);
        }

        [TestMethod]
        public void UpdatesDestinationNameForConfirmedFileBrowser()
        {
            var fileName = Path.Combine("Volumes", "Media", "test.m4v");

            SelectSource();
            _fileBrowser.BrowseToSave(Arg.Any<string>(), Arg.Any<string>()).Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = fileName
            });
            SelectDestination();

            Assert.AreEqual(fileName, _viewModel.DestinationName);
        }

        [TestMethod]
        public void EnablesStartWhenSourceIsValid()
        {
            SelectSource();

            Assert.IsTrue(_viewModel.StartCommand.CanExecute(null));
        }

        #endregion

        #region Helper Methods

        void SelectSource()
        {
            _viewModel.SourceSelectCommand.Execute(null);
        }

        void SelectDestination()
        {
            _viewModel.DestinationSelectCommand.Execute(null);
        }

        #endregion
    }
}
