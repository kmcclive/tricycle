using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.IO;
using Tricycle.IO.Models;
using Tricycle.Media;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;
using Tricycle.Models.Templates;
using Tricycle.UI.Models;
using Tricycle.UI.ViewModels;
using Tricycle.Utilities;

namespace Tricycle.UI.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        #region Constants

        const string DEFAULT_TEMPLATE_NAME = "Test";

        #endregion

        #region Fields

        MainViewModel _viewModel;
        IFileBrowser _fileBrowser;
        FileBrowserResult _fileBrowserResult;
        IMediaInspector _mediaInspector;
        VideoStreamInfo _videoStream;
        AudioStreamInfo _audioStream;
        MediaInfo _mediaInfo;
        IMediaTranscoder _mediaTranscoder;
        ICropDetector _cropDetector;
        CropParameters _cropParameters;
        IInterlaceDetector _interlaceDetector;
        ITranscodeCalculator _transcodeCalculator;
        IFileSystem _fileSystem;
        IFile _fileService;
        IAppManager _appManager;
        IConfigManager<TricycleConfig> _tricycleConfigManager;
        TricycleConfig _tricycleConfig;
        IConfigManager<Dictionary<string, JobTemplate>> _templateManager;
        JobTemplate _template;
        string _defaultDestinationDirectory;
        TranscodeJob _transcodeJob;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _fileBrowser = Substitute.For<IFileBrowser>();
            _mediaInspector = Substitute.For<IMediaInspector>();
            _mediaTranscoder = Substitute.For<IMediaTranscoder>();
            _cropDetector = Substitute.For<ICropDetector>();
            _interlaceDetector = Substitute.For<IInterlaceDetector>();
            _transcodeCalculator = Substitute.For<ITranscodeCalculator>();
            _fileSystem = Substitute.For<IFileSystem>();
            _appManager = Substitute.For<IAppManager>();
            _tricycleConfig = CreateDefaultTricycleConfig();
            _tricycleConfigManager = Substitute.For<IConfigManager<TricycleConfig>>();
            _tricycleConfigManager.Config = _tricycleConfig;
            _templateManager = Substitute.For<IConfigManager<Dictionary<string, JobTemplate>>>();
            _defaultDestinationDirectory = Path.Combine("Users", "fred", "Movies");
            _viewModel = new MainViewModel(_fileBrowser,
                                           _mediaInspector,
                                           _mediaTranscoder,
                                           _cropDetector,
                                           _interlaceDetector,
                                           _transcodeCalculator,
                                           _fileSystem,
                                           MockDevice.Self,
                                           _appManager,
                                           _tricycleConfigManager,
                                           _templateManager,
                                           _defaultDestinationDirectory)
            {
                IsPageVisible = true
            };

            _fileBrowserResult = new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            };
            _videoStream = new VideoStreamInfo();
            _audioStream = new AudioStreamInfo()
            {
                FormatName = "DTS",
                ChannelCount = 1
            };
            _mediaInfo = new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    _videoStream,
                    _audioStream
                }
            };
            _cropParameters = new CropParameters();
            _fileService = Substitute.For<IFile>();
            _template = new JobTemplate();

            _fileBrowser.BrowseToOpen().Returns(_fileBrowserResult);
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(_mediaInfo);
            _mediaTranscoder.When(x => x.Start(Arg.Any<TranscodeJob>())).Do(x => _transcodeJob = (TranscodeJob)x[0]);
            _cropDetector.Detect(Arg.Any<MediaInfo>()).Returns(_cropParameters);
            _fileSystem.File.Returns(_fileService);
            _appManager.When(x => x.RaiseFileOpened(Arg.Any<string>()))
                       .Do(x => _appManager.FileOpened += Raise.Event<Action<string>>(x[0]));
            _fileService.Exists(Arg.Any<string>()).Returns(false);
            _templateManager.Config = new Dictionary<string, JobTemplate>()
            {
                { DEFAULT_TEMPLATE_NAME, _template }
            };
            _templateManager.When(x => x.Save())
                .Do(x => _template = DictionaryUtility.GetValueOrDefault(_templateManager.Config, DEFAULT_TEMPLATE_NAME));
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
        public void HidesStatusInitially()
        {
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.Status));
        }

        [TestMethod]
        public void HidesProgressInitially()
        {
            Assert.AreEqual(_viewModel.Progress, 0);
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
        public void InspectsCorrectSourceForConfirmedFileBrowser()
        {
            string fileName = "test.mkv";

            _fileBrowserResult.FileName = fileName;
            SelectSource();
            _mediaInspector.Received().Inspect(fileName);
        }

        [TestMethod]
        public void DetectsCorrectSourceForConfirmedFileBrowser()
        {
            SelectSource();
            _cropDetector.Received().Detect(_mediaInfo);
        }

        [TestMethod]
        public void DetectsInterlacingOfCorrectSourceForConfirmedFileBrowser()
        {
            SelectSource();
            _interlaceDetector.Received().Detect(_mediaInfo);
        }

        [TestMethod]
        public void ShowsSpinnerWhenReadingSource()
        {
            _mediaInspector.When(x => x.Inspect(Arg.Any<string>()))
                           .Do(x => Assert.IsTrue(_viewModel.IsSpinnerVisible));
            SelectSource();

            if (!_mediaInspector.ReceivedCalls().Any())
            {
                Assert.Inconclusive("Assert was not called.");
            }
        }

        [TestMethod]
        public void HidesSpinnerWhenDoneReadingSource()
        {
            SelectSource();

            Assert.IsFalse(_viewModel.IsSpinnerVisible);
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
        public void RaisesSourceSelectedWithFalseWhenSourceIsNull()
        {
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(default(MediaInfo));
            SelectSource();

            _appManager.Received().RaiseSourceSelected(false);
        }

        [TestMethod]
        public void RaisesSourceSelectedWithFalseWhenSourceHasNoVideo()
        {
            _mediaInfo.Streams = new StreamInfo[] { new AudioStreamInfo() };
            SelectSource();

            _appManager.Received().RaiseSourceSelected(false);
        }

        [TestMethod]
        public void RaisesSourceSelectedWithTrueWhenSourceIsValid()
        {
            SelectSource();

            _appManager.Received().RaiseSourceSelected(true);
        }

        [TestMethod]
        public void ShowsSourceInfoWhenSourceIsValid()
        {
            SelectSource();

            Assert.IsTrue(_viewModel.IsSourceInfoVisible);
        }

        [TestMethod]
        public void EnablesPreviewCommandWhenSourceIsValid()
        {
            bool enabled = false;

            _viewModel.PreviewCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviewCommand.CanExecute(null);
            SelectSource();

            Assert.IsTrue(enabled);
        }

        [TestMethod]
        public void DisablesPreviewCommandWhenSourceIsNull()
        {
            bool enabled = false;

            _viewModel.PreviewCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviewCommand.CanExecute(null);
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(default(MediaInfo));

            SelectSource();

            Assert.IsFalse(enabled);
        }

        [TestMethod]
        public void DisablesPreviewCommandWhenSourceHasNoVideo()
        {
            bool enabled = false;

            _viewModel.PreviewCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviewCommand.CanExecute(null);
            _mediaInfo.Streams = new StreamInfo[] { new AudioStreamInfo() };
            SelectSource();

            Assert.IsFalse(enabled);
        }

        [TestMethod]
        public void RaisesModalOpenedWhenPreviewIsClicked()
        {
            SelectSource();
            _viewModel.PreviewCommand.Execute(null);

            _appManager.Received().RaiseModalOpened(Modal.Preview);
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
        public void Displays480iSourceSizeFor480Interlaced()
        {
            _videoStream.Dimensions = new Dimensions(640, 480);
            _interlaceDetector.Detect(Arg.Any<MediaInfo>()).Returns(true);
            SelectSource();

            Assert.AreEqual("480i", _viewModel.SourceSize);
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
            Assert.AreEqual("MP4", _viewModel.ContainerFormatOptions[0]?.Name);
            Assert.AreEqual("MKV", _viewModel.ContainerFormatOptions[1]?.Name);
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
        public void EnablesStartForValidSource()
        {
            SelectSource();

            Assert.IsTrue(_viewModel.StartCommand.CanExecute(null));
        }

        [TestMethod]
        public void DisablesStartForNoConfiguredVideoFormats()
        {
            _tricycleConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
            };
            SelectSource();

            Assert.IsFalse(_viewModel.StartCommand.CanExecute(null));
        }

        [TestMethod]
        public void EnablesVideoConfigForValidSource()
        {
            SelectSource();

            Assert.IsTrue(_viewModel.IsVideoConfigEnabled);
        }

        [TestMethod]
        public void DisablesVideoConfigForInvalidSource()
        {
            SelectSource();

            _mediaInfo.Streams = new StreamInfo[0];
            SelectSource();

            Assert.IsFalse(_viewModel.IsVideoConfigEnabled);
        }

        [TestMethod]
        public void PopulatesOnlyConfiguredVideoFormats()
        {
            _tricycleConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Hevc, new VideoCodec() }
                }
            };
            SelectSource();

            Assert.AreEqual(1, _viewModel.VideoFormatOptions?.Count);
            Assert.AreEqual("HEVC", _viewModel.VideoFormatOptions[0]?.Name);
        }

        [TestMethod]
        public void SelectsADefaultVideoFormat()
        {
            SelectSource();

            Assert.AreEqual("AVC", _viewModel.SelectedVideoFormat?.Name);
        }

        [TestMethod]
        public void SelectsHevcVideoFormatForHdrSource()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();

            Assert.AreEqual("HEVC", _viewModel.SelectedVideoFormat?.Name);
        }

        [TestMethod]
        public void DisablesHdrWhenAvcVideoFormatIsSelected()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem("AVC", VideoFormat.Avc);

            Assert.IsFalse(_viewModel.IsHdrEnabled);
        }

        [TestMethod]
        public void EnablesHdrWhenHevcVideoFormatIsSelected()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem("AVC", VideoFormat.Avc);
            _viewModel.SelectedVideoFormat = new ListItem("HEVC", VideoFormat.Hevc);

            Assert.IsTrue(_viewModel.IsHdrEnabled);
        }

        [TestMethod]
        public void SetsQualityStepCountWhenSourceIsSelected()
        {
            var stepCount = 6;

            _tricycleConfig.Video.Codecs[VideoFormat.Avc].QualitySteps = stepCount;
            SelectSource();

            Assert.AreEqual(6, _viewModel.QualityStepCount);
        }

        [TestMethod]
        public void SetsQualityStepCountWhenVideoFormatIsChanged()
        {
            var stepCount = 6;

            _tricycleConfig.Video.Codecs[VideoFormat.Hevc].QualitySteps = stepCount;
            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem("HEVC", VideoFormat.Hevc);

            Assert.AreEqual(6, _viewModel.QualityStepCount);
        }

        [TestMethod]
        public void SetsQualityToMedianByDefault()
        {
            var stepCount = 4;

            _tricycleConfig.Video.Codecs[VideoFormat.Avc].QualitySteps = stepCount;
            SelectSource();

            Assert.AreEqual(0.5, _viewModel.Quality);
        }

        [TestMethod]
        public void DisablesHdrForSdrSource()
        {
            _videoStream.DynamicRange = DynamicRange.Standard;
            SelectSource();

            Assert.IsFalse(_viewModel.IsHdrEnabled);
            Assert.IsFalse(_viewModel.IsHdrChecked);
        }

        [TestMethod]
        public void UnchecksHdrForSdrSource()
        {
            _videoStream.DynamicRange = DynamicRange.Standard;
            SelectSource();

            Assert.IsFalse(_viewModel.IsHdrChecked);
        }

        [TestMethod]
        public void ChecksHdrByDefaultForHdrSource()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();

            Assert.IsTrue(_viewModel.IsHdrChecked);
        }

        [TestMethod]
        public void SelectsOriginalSizeByDefault()
        {
            SelectSource();

            Assert.AreEqual("Same as source", _viewModel.SelectedSize?.Name);
        }

        [TestMethod]
        public void PopulatesSizeOptionsInOrder()
        {
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "720p", new Dimensions(1280, 720) },
                { "1080p", new Dimensions(1920, 1080) }
            };
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            SelectSource();

            Assert.AreEqual(3, _viewModel.SizeOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.SizeOptions[0]?.Name);
            Assert.AreEqual("1080p", _viewModel.SizeOptions[1]?.Name);
            Assert.AreEqual("720p", _viewModel.SizeOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitsSizeOptionsBasedOnSourceWidth()
        {
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "4K", new Dimensions(3840, 2160) },
                { "1080p", new Dimensions(1920, 1080) },
                { "720p", new Dimensions(1280, 720) }
            };
            _videoStream.Dimensions = new Dimensions(1920, 800);
            SelectSource();

            Assert.AreEqual(3, _viewModel.SizeOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.SizeOptions[0]?.Name);
            Assert.AreEqual("1080p", _viewModel.SizeOptions[1]?.Name);
            Assert.AreEqual("720p", _viewModel.SizeOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitsSizeOptionsBasedOnSourceHeight()
        {
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "4K", new Dimensions(3840, 2160) },
                { "1080p", new Dimensions(1920, 1080) },
                { "720p", new Dimensions(1280, 720) }
            };
            _videoStream.Dimensions = new Dimensions(1440, 1080);
            SelectSource();

            Assert.AreEqual(3, _viewModel.SizeOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.SizeOptions[0]?.Name);
            Assert.AreEqual("1080p", _viewModel.SizeOptions[1]?.Name);
            Assert.AreEqual("720p", _viewModel.SizeOptions[2]?.Name);
        }

        [TestMethod]
        public void SelectsAutoCropOptionByDefault()
        {
            SelectSource();
            Assert.AreEqual(_viewModel.SelectedCropOption?.Value, CropOption.Auto);
        }

        [TestMethod]
        public void ShowsAutoCropControlsByDefault()
        {
            SelectSource();
            Assert.IsTrue(_viewModel.IsAutoCropControlVisible);
        }

        [TestMethod]
        public void HidesManualCropControlsByDefault()
        {
            SelectSource();
            Assert.IsFalse(_viewModel.IsManualCropControlVisible);
        }

        [TestMethod]
        public void ShowsManualCropControlsWhenSelectionChanges()
        {
            SelectSource();

            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);

            Assert.IsTrue(_viewModel.IsManualCropControlVisible);
        }

        [TestMethod]
        public void HidesAutoCropControlsWhenSelectionChanges()
        {
            SelectSource();

            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);

            Assert.IsFalse(_viewModel.IsAutoCropControlVisible);
        }

        [TestMethod]
        public void ShowsAutoCropControlsWhenSelectionChanges()
        {
            SelectSource();

            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.SelectedCropOption = new ListItem(CropOption.Auto);

            Assert.IsTrue(_viewModel.IsAutoCropControlVisible);
        }

        [TestMethod]
        public void HidesManualCropControlsWhenSelectionChanges()
        {
            SelectSource();

            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.SelectedCropOption = new ListItem(CropOption.Auto);

            Assert.IsFalse(_viewModel.IsManualCropControlVisible);
        }

        [TestMethod]
        public void DisablesAutocropByDefault()
        {
            _cropDetector.Detect(Arg.Any<MediaInfo>()).Returns(default(CropParameters));
            SelectSource();

            Assert.IsFalse(_viewModel.IsAutocropEnabled);
        }

        [TestMethod]
        public void UnchecksAutocropByDefault()
        {
            _cropDetector.Detect(Arg.Any<MediaInfo>()).Returns(default(CropParameters));
            SelectSource();

            Assert.IsFalse(_viewModel.IsAutocropChecked);
        }

        [TestMethod]
        public void DisablesAutocropWhenNoBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.IsFalse(_viewModel.IsAutocropEnabled);
        }

        [TestMethod]
        public void UnchecksAutocropWhenNoBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.IsFalse(_viewModel.IsAutocropEnabled);
        }

        [TestMethod]
        public void EnablesAutocropWhenBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _cropParameters.Size = new Dimensions(3840, 1632);
            _cropParameters.Start = new Coordinate<int>(0, 264);
            SelectSource();

            Assert.IsTrue(_viewModel.IsAutocropEnabled);
        }

        [TestMethod]
        public void ChecksAutocropByDefaultWhenBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _cropParameters.Size = new Dimensions(3840, 1632);
            _cropParameters.Start = new Coordinate<int>(0, 264);
            SelectSource();

            Assert.IsTrue(_viewModel.IsAutocropChecked);
        }

        [TestMethod]
        public void SelectsOriginalAspectRatioByDefault()
        {
            SelectSource();

            Assert.AreEqual("Same as source", _viewModel.SelectedAspectRatio?.Name);
        }

        [TestMethod]
        public void PopulatesAspectRatioOptionsInOrder()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "4:3", new Dimensions(4, 3) },
                { "16:9", new Dimensions(16, 9) }
            };
            _videoStream.Dimensions = new Dimensions(3840, 1632);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.AreEqual(3, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("16:9", _viewModel.AspectRatioOptions[1]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitsAspectRatioOptionsBasedOnSource()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "21:9", new Dimensions(21, 9) },
                { "16:9", new Dimensions(16, 9) },
                { "4:3", new Dimensions(4, 3) }
            };
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.AreEqual(3, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("16:9", _viewModel.AspectRatioOptions[1]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitsAspectRatioOptionsBasedOnBars()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "16:9", new Dimensions(16, 9) },
                { "4:3", new Dimensions(4, 3) }
            };
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(2880, 2160);
            SelectSource();

            Assert.AreEqual(2, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[1]?.Name);
        }

        [TestMethod]
        public void LimitsAspectRatioOptionsForAnamorphicSource()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "21:9", new Dimensions(21, 9) },
                { "16:9", new Dimensions(16, 9) },
                { "4:3", new Dimensions(4, 3) }
            };
            _videoStream.Dimensions = new Dimensions(853, 480);
            _videoStream.StorageDimensions = new Dimensions(720, 480);
            _cropParameters.Size = new Dimensions(853, 464);
            SelectSource();

            Assert.AreEqual(3, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("16:9", _viewModel.AspectRatioOptions[1]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[2]?.Name);
        }

        [TestMethod]
        public void ZeroesOutManualCropControlsWhenNoBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.AreEqual("0", _viewModel.CropTop);
            Assert.AreEqual("0", _viewModel.CropBottom);
            Assert.AreEqual("0", _viewModel.CropLeft);
            Assert.AreEqual("0", _viewModel.CropRight);
        }

        [TestMethod]
        public void PopulatesManualCropControlsWhenHorizontalBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(3840, 1606);
            _cropParameters.Start = new Coordinate<int>(0, 278);
            SelectSource();

            Assert.AreEqual("278", _viewModel.CropTop);
            Assert.AreEqual("276", _viewModel.CropBottom);
            Assert.AreEqual("0", _viewModel.CropLeft);
            Assert.AreEqual("0", _viewModel.CropRight);
        }

        [TestMethod]
        public void PopulatesManualCropControlsWhenVerticalBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(1440, 1080);
            _cropParameters.Start = new Coordinate<int>(242, 0);
            SelectSource();

            Assert.AreEqual("0", _viewModel.CropTop);
            Assert.AreEqual("0", _viewModel.CropBottom);
            Assert.AreEqual("242", _viewModel.CropLeft);
            Assert.AreEqual("238", _viewModel.CropRight);
        }

        [TestMethod]
        public void PopulatesSubtitleOptions()
        {
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new StreamInfo()
                {
                    StreamType = StreamType.Subtitle,
                    Language = "eng"
                },
                new StreamInfo()
                {
                    StreamType = StreamType.Subtitle,
                    Language = "spa"
                }
            };
            SelectSource();

            Assert.AreEqual(3, _viewModel.SubtitleOptions?.Count);
            Assert.AreEqual("None", _viewModel.SubtitleOptions[0]?.Name);
            Assert.AreEqual("1: English", _viewModel.SubtitleOptions[1]?.Name);
            Assert.AreEqual("2: Spanish", _viewModel.SubtitleOptions[2]?.Name);
        }

        [TestMethod]
        public void ChecksForcedSubtitlesWhenEnabled()
        {
            _tricycleConfig.ForcedSubtitlesOnly = true;
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new StreamInfo()
                {
                    StreamType = StreamType.Subtitle,
                    Language = "eng"
                }
            };
            SelectSource();

            Assert.IsTrue(_viewModel.IsForcedSubtitlesChecked);
        }

        [TestMethod]
        public void DoesNotCheckForcedSubtitlesWhenDisabled()
        {
            _tricycleConfig.ForcedSubtitlesOnly = false;
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new StreamInfo()
                {
                    StreamType = StreamType.Subtitle,
                    Language = "eng"
                }
            };
            SelectSource();

            Assert.IsFalse(_viewModel.IsForcedSubtitlesChecked);
        }

        [TestMethod]
        public void DoesNotCheckForcedSubtitlesWhenNoSubtitlesExist()
        {
            _tricycleConfig.ForcedSubtitlesOnly = true;
            SelectSource();

            Assert.IsFalse(_viewModel.IsForcedSubtitlesChecked);
        }

        [TestMethod]
        public void DisablesForcedSubtitlesBeforeSubtitleSelection()
        {
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new StreamInfo()
                {
                    StreamType = StreamType.Subtitle,
                    Language = "eng"
                }
            };
            SelectSource();

            Assert.IsFalse(_viewModel.IsForcedSubtitlesEnabled);
        }

        [TestMethod]
        public void EnablesForcedSubtitlesForSelectedSubtitle()
        {
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new StreamInfo()
                {
                    StreamType = StreamType.Subtitle,
                    Language = "eng"
                }
            };
            SelectSource();
            _viewModel.SelectedSubtitle = new ListItem(_mediaInfo.Streams.Last());

            Assert.IsTrue(_viewModel.IsForcedSubtitlesEnabled);
        }

        [TestMethod]
        public void ListsNoAudioOutputsWhenSourceHasNoTracks()
        {
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream
            };
            SelectSource();

            Assert.IsTrue(_viewModel.AudioOutputs?.Any() != true);
        }

        [TestMethod]
        public void ListsNoAudioOutputsWhenNoCodecsAreConfigured()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>();
            SelectSource();

            Assert.IsTrue(_viewModel.AudioOutputs?.Any() != true);
        }

        [TestMethod]
        public void ListsNoAudioOutputsWhenNoCodecsHavePresets()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[0]
                    }
                }
            };
            SelectSource();

            Assert.IsTrue(_viewModel.AudioOutputs?.Any() != true);
        }

        [TestMethod]
        public void AddsDefaultAudioOutputWhenSourceHasTracks()
        {
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.IsNotNull(audioOutput);
            Assert.AreEqual(_audioStream, audioOutput.SelectedTrack?.Value);
        }

        [TestMethod]
        public void AddsPlaceholderAudioOutputWhenSourceHasTracks()
        {
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.LastOrDefault();

            Assert.IsNotNull(audioOutput);
            Assert.AreEqual("None", audioOutput.SelectedTrack?.Name);
        }

        [TestMethod]
        public void AddsPlaceholderAudioOutputWhenAnotherTrackIsSelected()
        {
            SelectSource();
            _viewModel.AudioOutputs.LastOrDefault().SelectedTrack = new ListItem(_audioStream);

            Assert.AreEqual(3, _viewModel.AudioOutputs?.Count);
            Assert.AreEqual("None", _viewModel.AudioOutputs.LastOrDefault().SelectedTrack?.Name);
        }

        [TestMethod]
        public void PopulatesAudioTrackOptions()
        {
            var audioStream2 = new AudioStreamInfo()
            {
                FormatName = "LPCM",
                ChannelCount = 1
            };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                _audioStream,
                audioStream2
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual(3, audioOutput?.TrackOptions?.Count);
            Assert.AreEqual("None", audioOutput.TrackOptions[0]?.Name);
            Assert.AreEqual(_audioStream, audioOutput.TrackOptions[1]?.Value);
            Assert.AreEqual(audioStream2, audioOutput.TrackOptions[2]?.Value);
        }

        [TestMethod]
        public void LimitsAudioTrackOptionsByConfiguredCodecs()
        {
            var audioStream1 = new AudioStreamInfo()
            {
                FormatName = "DTS",
                ChannelCount = 6
            };
            var audioStream2 = new AudioStreamInfo()
            {
                FormatName = "LPCM",
                ChannelCount = 2
            };

            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                audioStream1,
                audioStream2
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual(2, audioOutput?.TrackOptions?.Count);
            Assert.AreEqual("None", audioOutput.TrackOptions[0]?.Name);
            Assert.AreEqual(audioStream1, audioOutput.TrackOptions[1]?.Value);
        }

        [TestMethod]
        public void DisplaysCorrectNameForAudioTrackWithUniqueProfile()
        {
            _audioStream.FormatName = "DTS";
            _audioStream.ProfileName = "MA";
            _audioStream.ChannelCount = 1;
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: DTS MA Mono", audioOutput?.SelectedTrack?.Name);
        }

        [TestMethod]
        public void DisplaysCorrectNameForAudioTrackWithDuplicateProfile()
        {
            _audioStream.FormatName = "DTS";
            _audioStream.ProfileName = "DTS-MA";
            _audioStream.ChannelCount = 8;
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: DTS-MA 7.1", audioOutput?.SelectedTrack?.Name);
        }

        [TestMethod]
        public void DisplaysCorrectNameForAacAudioTrack()
        {
            _audioStream.Format = AudioFormat.Aac;
            _audioStream.ChannelCount = 2;
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: AAC Stereo", audioOutput?.SelectedTrack?.Name);
        }

        [TestMethod]
        public void DisplaysCorrectNameForAc3AudioTrack()
        {
            _audioStream.Format = AudioFormat.Ac3;
            _audioStream.ChannelCount = 6;
            _audioStream.Language = "eng";
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: Dolby Digital 5.1 (English)", audioOutput?.SelectedTrack?.Name);
        }

        [TestMethod]
        public void DisplaysCorrectNameForTrueHdAudioTrack()
        {
            _audioStream.FormatName = "truehd";
            _audioStream.ChannelCount = 8;
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: Dolby TrueHD 7.1", audioOutput?.SelectedTrack?.Name);
        }

        [TestMethod]
        public void PopulatesAudioFormatOptions()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                },
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                },
                {
                    AudioFormat.HeAac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual(3, audioOutput?.FormatOptions?.Count);
            Assert.AreEqual("AAC", audioOutput.FormatOptions[0]?.Name);
            Assert.AreEqual("Dolby Digital", audioOutput.FormatOptions[1]?.Name);
            Assert.AreEqual("HE-AAC", audioOutput.FormatOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitsAudioFormatOptionsByConfiguredCodecs()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Stereo
                            }
                        }
                    }
                },
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new AudioStreamInfo()
                {
                    FormatName = "DTS",
                    ChannelCount = 2
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual(1, audioOutput?.FormatOptions?.Count);
            Assert.AreEqual("AAC", audioOutput.FormatOptions[0]?.Name);
        }

        [TestMethod]
        public void SelectsAudioFormatByDefault()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("AAC", audioOutput?.SelectedFormat?.Name);
        }

        [TestMethod]
        public void PopulatesAudioMixdownOptionsInOrder()
        {
            _audioStream.ChannelCount = 8;
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            },
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Stereo
                            },
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual(3, audioOutput?.MixdownOptions?.Count);
            Assert.AreEqual("Surround", audioOutput.MixdownOptions[0]?.Name);
            Assert.AreEqual("Stereo", audioOutput.MixdownOptions[1]?.Name);
            Assert.AreEqual("Mono", audioOutput.MixdownOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitsAudioMixdownOptionsByTrack()
        {
            _audioStream.ChannelCount = 2;
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            },
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Stereo
                            },
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual(2, audioOutput?.MixdownOptions?.Count);
            Assert.AreEqual("Stereo", audioOutput.MixdownOptions[0]?.Name);
            Assert.AreEqual("Mono", audioOutput.MixdownOptions[1]?.Name);
        }

        [TestMethod]
        public void ChangesAudioMixdownOptionsWhenFormatChanges()
        {
            _audioStream.ChannelCount = 8;
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                },
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Stereo
                            },
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            audioOutput.SelectedFormat = new ListItem(AudioFormat.Ac3);

            Assert.AreEqual(2, audioOutput?.MixdownOptions?.Count);
            Assert.AreEqual("Surround", audioOutput.MixdownOptions[0]?.Name);
            Assert.AreEqual("Stereo", audioOutput.MixdownOptions[1]?.Name);
        }

        [TestMethod]
        public void SelectsAudioMixdownByDefault()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                }
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("Mono", audioOutput?.SelectedMixdown?.Name);
        }

        [TestMethod]
        public void SelectsContainerFormatForTemplate()
        {
            var format = ContainerFormat.Mkv;

            _template.Format = format;
            _templateManager.Config["Other"] = new JobTemplate() { Format = ContainerFormat.Mp4 };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(format), _viewModel.SelectedContainerFormat);
        }

        [TestMethod]
        public void SelectsVideoFormatForTemplate()
        {
            var format = VideoFormat.Hevc;

            _template.Video = new VideoTemplate { Format = format };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(format), _viewModel.SelectedVideoFormat);
        }

        [TestMethod]
        public void SelectsVideoQualityForTemplate()
        {
            var format = VideoFormat.Hevc;

            _tricycleConfig.Video.Codecs[format] = new VideoCodec()
            {
                QualityRange = new Range<decimal>(22, 18)
            };
            _template.Video = new VideoTemplate
            {
                Format = format,
                Quality = 19,
            };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(0.75, _viewModel.Quality);
        }

        [TestMethod]
        public void ChecksHdrForTemplateWhenAllowed()
        {
            _template.Video = new VideoTemplate
            {
                Format = VideoFormat.Hevc,
                Hdr = true
            };
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            ApplyTemplate();

            Assert.IsTrue(_viewModel.IsHdrChecked);
        }

        [TestMethod]
        public void DoesNotCheckHdrForTemplateWhenNotAllowed()
        {
            _template.Video = new VideoTemplate
            {
                Format = VideoFormat.Hevc,
                Hdr = true
            };
            _videoStream.DynamicRange = DynamicRange.Standard;
            SelectSource();
            ApplyTemplate();

            Assert.IsFalse(_viewModel.IsHdrChecked);
        }

        [TestMethod]
        public void SetsManualCropParametersForTemplate()
        {
            _template.Video = new VideoTemplate
            {
                ManualCrop = new ManualCropTemplate()
                {
                    Top = 8,
                    Bottom = 12,
                    Left = 2,
                    Right = 4
                }
            };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(CropOption.Manual), _viewModel.SelectedCropOption);
            Assert.AreEqual(_template.Video.ManualCrop.Top.ToString(), _viewModel.CropTop);
            Assert.AreEqual(_template.Video.ManualCrop.Bottom.ToString(), _viewModel.CropBottom);
            Assert.AreEqual(_template.Video.ManualCrop.Left.ToString(), _viewModel.CropLeft);
            Assert.AreEqual(_template.Video.ManualCrop.Right.ToString(), _viewModel.CropRight);
        }

        [TestMethod]
        public void ChecksCropBarsForTemplateWhenAllowed()
        {
            _template.Video = new VideoTemplate
            {
                CropBars = true
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            ApplyTemplate();

            Assert.IsTrue(_viewModel.IsAutocropChecked);
        }

        [TestMethod]
        public void DoesNotCheckCropBarsForTemplateWhenNotAllowed()
        {
            _template.Video = new VideoTemplate
            {
                CropBars = true
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();
            ApplyTemplate();

            Assert.IsFalse(_viewModel.IsAutocropChecked);
        }

        [TestMethod]
        public void SelectsSizeForTemplate()
        {
            var size = "1080p";

            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { size, _videoStream.Dimensions }
            };
            _template.Video = new VideoTemplate { SizePreset = size };
            
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(_videoStream.Dimensions), _viewModel.SelectedSize);
        }

        [TestMethod]
        public void SelectsNextBestSizeForTemplate()
        {
            var size = "1080p";
            var expected = new Dimensions(1280, 720);

            _videoStream.Dimensions = new Dimensions(1422, 800);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { size, new Dimensions(1920, 1080) },
                { "720p",  expected }
            };
            _template.Video = new VideoTemplate { SizePreset = size };

            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(expected), _viewModel.SelectedSize);
        }

        [TestMethod]
        public void SelectsAspectRatioForTemplate()
        {
            var name = "16:9";
            var dimensions = new Dimensions(16, 9);

            _videoStream.Dimensions = new Dimensions(1920, 800);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { name, dimensions }
            };
            _template.Video = new VideoTemplate { AspectRatioPreset = name };

            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(dimensions), _viewModel.SelectedAspectRatio);
        }

        [TestMethod]
        public void SelectsNextBestAspectRatioForTemplate()
        {
            var name = "16:9";
            var dimensions = new Dimensions(4, 3);

            _videoStream.Dimensions = new Dimensions(1440, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = _videoStream.Dimensions;
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { name, new Dimensions(16, 9) },
                { "4:3", dimensions }
            };
            _template.Video = new VideoTemplate { AspectRatioPreset = name };

            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(dimensions), _viewModel.SelectedAspectRatio);
        }

        [TestMethod]
        public void ChecksDenoiseForTemplate()
        {
            _template.Video = new VideoTemplate { Denoise = true };
            SelectSource();
            ApplyTemplate();

            Assert.IsTrue(_viewModel.IsDenoiseChecked);
        }

        [TestMethod]
        public void SelectsSubtitleForTemplate()
        {
            var language = "eng";
            var stream = new SubtitleStreamInfo() { Index = 3, Language = language };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new SubtitleStreamInfo() { Index = 2, Language = "spa" },
                stream
            };
            _template.Subtitles = new SubtitleTemplate() { Language = language };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(new ListItem(stream), _viewModel.SelectedSubtitle);
        }

        [TestMethod]
        public void ChecksForcedSubtitlesForTemplate()
        {
            _tricycleConfig.ForcedSubtitlesOnly = false;
            _template.Subtitles = new SubtitleTemplate() { ForcedOnly = true };
            SelectSource();
            ApplyTemplate();

            Assert.IsTrue(_viewModel.IsForcedSubtitlesChecked);
        }

        [TestMethod]
        public void ClearsAudioForTemplate()
        {
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(1, _viewModel.AudioOutputs?.Count);
        }

        [TestMethod]
        public void AddsMatchingAudioOutputForTemplate()
        {
            var audioTemplate = new AudioTemplate()
            {
                Format = AudioFormat.Aac,
                Language = "eng",
                Mixdown = AudioMixdown.Stereo,
                RelativeIndex = 1
            };
            var stream = new AudioStreamInfo()
            {
                Index = 3,
                Language = audioTemplate.Language,
                FormatName = "ac-3",
                ChannelCount = 6
            };

            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    audioTemplate.Format,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset() { Mixdown = audioTemplate.Mixdown }
                        }
                    }
                }
            };
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new AudioStreamInfo()
                {
                    Index = 1,
                    Language = "eng",
                    FormatName = "ac-3",
                    ChannelCount = 6
                },
                new AudioStreamInfo()
                {
                    Index = 2,
                    Language = "spa",
                    FormatName = "ac-3",
                    ChannelCount = 6
                },
                stream
            };
            _template.AudioTracks = new AudioTemplate[]
            {
                audioTemplate
            };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(2, _viewModel.AudioOutputs?.Count);

            var output = _viewModel.AudioOutputs[0];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(stream), output.SelectedTrack);
            Assert.AreEqual(new ListItem(audioTemplate.Format), output.SelectedFormat);
            Assert.AreEqual(new ListItem(audioTemplate.Mixdown), output.SelectedMixdown);
        }

        [TestMethod]
        public void AddsNextBestAudioOutputForTemplate()
        {
            var audioTemplate = new AudioTemplate()
            {
                Format = AudioFormat.Aac,
                Language = "eng",
                Mixdown = AudioMixdown.Stereo,
                RelativeIndex = 1
            };
            var stream = new AudioStreamInfo()
            {
                Index = 2,
                Language = audioTemplate.Language,
                FormatName = "ac-3",
                ChannelCount = 1
            };

            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    audioTemplate.Format,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset() { Mixdown = AudioMixdown.Mono },
                            new AudioPreset() { Mixdown = audioTemplate.Mixdown }
                        }
                    }
                }
            };
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new AudioStreamInfo()
                {
                    Index = 1,
                    Language = "spa",
                    FormatName = "ac-3",
                    ChannelCount = 6
                },
                stream
            };
            _template.AudioTracks = new AudioTemplate[]
            {
                audioTemplate
            };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(2, _viewModel.AudioOutputs?.Count);

            var output = _viewModel.AudioOutputs[0];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(stream), output.SelectedTrack);
            Assert.AreEqual(new ListItem(audioTemplate.Format), output.SelectedFormat);
            Assert.AreEqual(new ListItem(AudioMixdown.Mono), output.SelectedMixdown);
        }

        [TestMethod]
        public void AddsMultipleAudioOutputsForTemplate()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Aac,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset() { Mixdown = AudioMixdown.Stereo }
                        }
                    }
                },
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset() { Mixdown = AudioMixdown.Surround5dot1 }
                        }
                    }
                }
            };
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                new AudioStreamInfo()
                {
                    Index = 1,
                    Language = "eng",
                    FormatName = "ac-3",
                    ChannelCount = 6
                },
                new AudioStreamInfo()
                {
                    Index = 2,
                    Language = "spa",
                    FormatName = "ac-3",
                    ChannelCount = 2
                },
                new AudioStreamInfo()
                {
                    Index = 3,
                    Language = "eng",
                    FormatName = "ac-3",
                    ChannelCount = 2
                }
            };
            _template.AudioTracks = new AudioTemplate[]
            {
                new AudioTemplate()
                {
                    Format = AudioFormat.Aac,
                    Language = "eng",
                    Mixdown = AudioMixdown.Stereo,
                    RelativeIndex = 0
                },
                new AudioTemplate()
                {
                    Format = AudioFormat.Ac3,
                    Language = "eng",
                    Mixdown = AudioMixdown.Surround5dot1,
                    RelativeIndex = 0
                },
                new AudioTemplate()
                {
                    Format = AudioFormat.Aac,
                    Language = "eng",
                    Mixdown = AudioMixdown.Stereo,
                    RelativeIndex = 1
                },
                new AudioTemplate()
                {
                    Format = AudioFormat.Aac,
                    Language = "spa",
                    Mixdown = AudioMixdown.Stereo,
                    RelativeIndex = 0
                }
            };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(5, _viewModel.AudioOutputs?.Count);

            var output = _viewModel.AudioOutputs[0];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(_mediaInfo.Streams[1]), output.SelectedTrack);
            Assert.AreEqual(new ListItem(AudioFormat.Aac), output.SelectedFormat);
            Assert.AreEqual(new ListItem(AudioMixdown.Stereo), output.SelectedMixdown);

            output = _viewModel.AudioOutputs[1];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(_mediaInfo.Streams[1]), output.SelectedTrack);
            Assert.AreEqual(new ListItem(AudioFormat.Ac3), output.SelectedFormat);
            Assert.AreEqual(new ListItem(AudioMixdown.Surround5dot1), output.SelectedMixdown);

            output = _viewModel.AudioOutputs[2];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(_mediaInfo.Streams[3]), output.SelectedTrack);
            Assert.AreEqual(new ListItem(AudioFormat.Aac), output.SelectedFormat);
            Assert.AreEqual(new ListItem(AudioMixdown.Stereo), output.SelectedMixdown);

            output = _viewModel.AudioOutputs[3];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(_mediaInfo.Streams[2]), output.SelectedTrack);
            Assert.AreEqual(new ListItem(AudioFormat.Aac), output.SelectedFormat);
            Assert.AreEqual(new ListItem(AudioMixdown.Stereo), output.SelectedMixdown);
        }

        [TestMethod]
        public void RemovesDuplicateAudioOutputsForTemplate()
        {
            var stream = new AudioStreamInfo()
            {
                Index = 1,
                Language = "eng",
                FormatName = "ac-3",
                ChannelCount = 2
            };

            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset() { Mixdown = AudioMixdown.Stereo },
                            new AudioPreset() { Mixdown = AudioMixdown.Surround5dot1 }
                        }
                    }
                }
            };
            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                stream
            };
            _template.AudioTracks = new AudioTemplate[]
            {
                new AudioTemplate()
                {
                    Format = AudioFormat.Ac3,
                    Language = "eng",
                    Mixdown = AudioMixdown.Stereo,
                    RelativeIndex = 0
                },
                new AudioTemplate()
                {
                    Format = AudioFormat.Ac3,
                    Language = "eng",
                    Mixdown = AudioMixdown.Surround5dot1,
                    RelativeIndex = 0
                }
            };
            SelectSource();
            ApplyTemplate();

            Assert.AreEqual(2, _viewModel.AudioOutputs?.Count);

            var output = _viewModel.AudioOutputs[0];

            Assert.IsNotNull(output);
            Assert.AreEqual(new ListItem(stream), output.SelectedTrack);
            Assert.AreEqual(new ListItem(AudioFormat.Ac3), output.SelectedFormat);
            Assert.AreEqual(new ListItem(AudioMixdown.Stereo), output.SelectedMixdown);
        }

        [TestMethod]
        public void CallsSaveOnTemplateManager()
        {
            SelectSource();
            SaveTemplate();

            _templateManager.Received().Save();
        }

        [TestMethod]
        public void AddsTemplateOnSave()
        {
            var name = "Other";

            SelectSource();
            SaveTemplate(name);

            Assert.IsTrue(_templateManager.Config?.ContainsKey(name) == true);
        }

        [TestMethod]
        public void SavesContainerFormatForTemplate()
        {
            var format = ContainerFormat.Mkv;

            SelectSource();
            _viewModel.SelectedContainerFormat = new ListItem(format);

            SaveTemplate();

            Assert.AreEqual(format, _template?.Format);
        }

        [TestMethod]
        public void SavesVideoFormatForTemplate()
        {
            var format = VideoFormat.Hevc;

            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem(format);

            SaveTemplate();

            Assert.AreEqual(format, _template?.Video?.Format);
        }

        [TestMethod]
        public void SavesVideoQualityForTemplate()
        {
            var format = VideoFormat.Hevc;

            _tricycleConfig.Video.Codecs[format] = new VideoCodec()
            {
                QualityRange = new Range<decimal>(22, 18)
            };
            SelectSource();
            _viewModel.Quality = 0.75;

            SaveTemplate();

            Assert.AreEqual(19, _template?.Video?.Quality);
        }

        [TestMethod]
        public void SavesHdrForTemplate()
        {
            var hdr = true;

            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem(VideoFormat.Hevc);
            _viewModel.IsHdrChecked = hdr;

            SaveTemplate();

            Assert.AreEqual(hdr, _template?.Video?.Hdr);
        }

        [TestMethod]
        public void SavesManualCropParametersForTemplate()
        {
            var expected = new ManualCropTemplate()
            {
                Top = 8,
                Bottom = 12,
                Left = 2,
                Right = 4
            };

            SelectSource();
            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.CropTop = expected.Top.ToString();
            _viewModel.CropBottom = expected.Bottom.ToString();
            _viewModel.CropLeft = expected.Left.ToString();
            _viewModel.CropRight = expected.Right.ToString();

            SaveTemplate();

            var actual = _template?.Video?.ManualCrop;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Top, actual.Top);
            Assert.AreEqual(expected.Bottom, actual.Bottom);
            Assert.AreEqual(expected.Left, actual.Left);
            Assert.AreEqual(expected.Right, actual.Right);
        }

        [TestMethod]
        public void SavesCropBarsForTemplate()
        {
            var cropBars = true;

            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.IsAutocropChecked = cropBars;

            SaveTemplate();

            Assert.AreEqual(cropBars, _template?.Video?.CropBars);
        }

        [TestMethod]
        public void SavesSizeForTemplate()
        {
            var size = "1080p";

            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { size, _videoStream.Dimensions }
            };
            SelectSource();
            _viewModel.SelectedSize = new ListItem(size, _videoStream.Dimensions);

            SaveTemplate();

            Assert.AreEqual(size, _template?.Video?.SizePreset);
        }

        [TestMethod]
        public void SavesAspectRatioForTemplate()
        {
            var name = "16:9";
            var dimensions = new Dimensions(16, 9);

            _videoStream.Dimensions = new Dimensions(1920, 800);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { name, dimensions }
            };
            SelectSource();
            _viewModel.SelectedAspectRatio = new ListItem(name, dimensions);

            SaveTemplate();

            Assert.AreEqual(name, _template?.Video?.AspectRatioPreset);
        }

        [TestMethod]
        public void SavesDenoiseForTemplate()
        {
            var denoise = true;

            SelectSource();
            _viewModel.IsDenoiseChecked = denoise;

            SaveTemplate();

            Assert.AreEqual(denoise, _template?.Video?.Denoise);
        }

        [TestMethod]
        public void SavesSubtitleLanguageForTemplate()
        {
            var language = "eng";
            var stream = new SubtitleStreamInfo() { Index = 1, Language = language };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                stream,
                new SubtitleStreamInfo() { Index = 2, Language = "spa" }
            };
            SelectSource();
            _viewModel.SelectedSubtitle = new ListItem(stream);

            SaveTemplate();

            Assert.AreEqual(language, _template?.Subtitles?.Language);
        }

        [TestMethod]
        public void SavesForcedSubtitlesForTemplate()
        {
            var forced = true;
            var stream = new SubtitleStreamInfo() { Index = 1, Language = "eng" };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                stream,
                new SubtitleStreamInfo() { Index = 2, Language = "spa" }
            };
            SelectSource();
            _viewModel.SelectedSubtitle = new ListItem(stream);
            _viewModel.IsForcedSubtitlesChecked = true;

            SaveTemplate();

            Assert.AreEqual(forced, _template?.Subtitles?.ForcedOnly);
        }

        [TestMethod]
        public void CallsTranscoderStartForStartCommand()
        {
            SelectSource();
            Start();

            _mediaTranscoder.Received().Start(Arg.Any<TranscodeJob>());
        }

        [TestMethod]
        public void SetsSourceInfoForJob()
        {
            SelectSource();
            Start();

            Assert.AreEqual(_mediaInfo, _transcodeJob?.SourceInfo);
        }

        [TestMethod]
        public void SetsOutputFileNameForJob()
        {
            var fileName = Path.Combine("Volumes", "Media", "test.m4v");

            SelectSource();
            _fileBrowser.BrowseToSave(Arg.Any<string>(), Arg.Any<string>()).Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = fileName
            });
            SelectDestination();
            Start();

            Assert.AreEqual(fileName, _transcodeJob?.OutputFileName);
        }

        [TestMethod]
        public void SetsFormatForJob()
        {
            var format = ContainerFormat.Mkv;

            SelectSource();
            _viewModel.SelectedContainerFormat = new ListItem(format);
            Start();

            Assert.AreEqual(format, _transcodeJob?.Format);
        }

        [TestMethod]
        public void SetsMetadataForMp4Job()
        {
            AppState.AppName = "Tricycle";
            AppState.AppVersion = new Version(1, 2, 3, 4);

            SelectSource();
            _viewModel.SelectedContainerFormat = new ListItem(ContainerFormat.Mp4);
            Start();

            Assert.AreEqual("Tricycle 1.2.3.4", _transcodeJob?.Metadata?.GetValueOrDefault("encoding_tool"));
        }

        [TestMethod]
        public void SetsMetadataForMkvJob()
        {
            AppState.AppName = "Tricycle";
            AppState.AppVersion = new Version(1, 2, 3, 4);

            SelectSource();
            _viewModel.SelectedContainerFormat = new ListItem(ContainerFormat.Mkv);
            Start();

            Assert.AreEqual("Tricycle 1.2.3.4", _transcodeJob?.Metadata?.GetValueOrDefault("encoder"));
        }

        [TestMethod]
        public void SetsVideoSourceStreamIndexForJob()
        {
            _videoStream.Index = 2;
            SelectSource();
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(_videoStream.Index, videoOutput?.SourceStreamIndex);
        }

        [TestMethod]
        public void SetsVideoFormatForJob()
        {
            var format = VideoFormat.Hevc;

            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem(format);
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(format, videoOutput?.Format);
        }

        [TestMethod]
        public void SetsVideoQualityForJob()
        {
            var format = VideoFormat.Hevc;

            _tricycleConfig.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    {
                        format,
                        new VideoCodec()
                        {
                            QualityRange = new Range<decimal>(25, 15),
                            QualitySteps = 4
                        }
                    }
                }
            };
            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem(format);
            _viewModel.Quality = 0.75;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(17.5M, videoOutput?.Quality);
        }

        [TestMethod]
        public void SetsVideoDynamicRangeForHdrJob()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.IsHdrChecked = true;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(DynamicRange.High, videoOutput?.DynamicRange);
        }

        [TestMethod]
        public void SetsVideoDynamicRangeForSdrJob()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.IsHdrChecked = false;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(DynamicRange.Standard, videoOutput?.DynamicRange);
        }

        [TestMethod]
        public void SetsVideoCopyHdrMetadataForHdrJob()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.IsHdrChecked = true;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(true, videoOutput?.CopyHdrMetadata);
        }

        [TestMethod]
        public void SetsVideoCopyHdrMetadataForSdrJob()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.IsHdrChecked = false;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(false, videoOutput?.CopyHdrMetadata);
        }

        [TestMethod]
        public void SetsVideoTonemapForHdrJob()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.IsHdrChecked = true;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(false, videoOutput?.Tonemap);
        }

        [TestMethod]
        public void SetsVideoTonemapForSdrJob()
        {
            _videoStream.DynamicRange = DynamicRange.High;
            SelectSource();
            _viewModel.IsHdrChecked = false;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(true, videoOutput?.Tonemap);
        }

        [TestMethod]
        public void SetsVideoDeinterlaceForJobWhenOff()
        {
            _tricycleConfig.Video.Deinterlace = SmartSwitchOption.Off;
            _interlaceDetector.Detect(Arg.Any<MediaInfo>()).Returns(true);
            SelectSource();
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.IsFalse(videoOutput.Deinterlace);
        }

        [TestMethod]
        public void SetsVideoDeinterlaceForJobWhenOn()
        {
            _tricycleConfig.Video.Deinterlace = SmartSwitchOption.On;
            _interlaceDetector.Detect(Arg.Any<MediaInfo>()).Returns(false);
            SelectSource();
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.IsTrue(videoOutput.Deinterlace);
        }

        [TestMethod]
        public void SetsVideoDeinterlaceForJobWhenAutoAndInterlaced()
        {
            bool interlaced = true;

            _tricycleConfig.Video.Deinterlace = SmartSwitchOption.Auto;
            _interlaceDetector.Detect(Arg.Any<MediaInfo>()).Returns(interlaced);
            SelectSource();
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(interlaced, videoOutput.Deinterlace);
        }

        [TestMethod]
        public void SetsVideoDeinterlaceForJobWhenAutoAndNotInterlaced()
        {
            bool interlaced = false;

            _tricycleConfig.Video.Deinterlace = SmartSwitchOption.Auto;
            _interlaceDetector.Detect(Arg.Any<MediaInfo>()).Returns(interlaced);
            SelectSource();
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(interlaced, videoOutput.Deinterlace);
        }

        [TestMethod]
        public void SetsVideoDenoiseForJobWhenChecked()
        {
            SelectSource();
            _viewModel.IsDenoiseChecked = true;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(true, videoOutput?.Denoise);
        }

        [TestMethod]
        public void SetsVideoDenoiseForJobWhenUnchecked()
        {
            SelectSource();
            _viewModel.IsDenoiseChecked = false;
            Start();

            var videoOutput = _transcodeJob?.Streams?.FirstOrDefault() as VideoOutputStream;

            Assert.AreEqual(false, videoOutput?.Denoise);
        }

        [TestMethod]
        public void CallsCalculateCropParametersForCropJob()
        {
            _videoStream.Dimensions = new Dimensions(853, 480);
            _videoStream.StorageDimensions = new Dimensions(720, 480);
            _cropParameters.Size = new Dimensions(720, 464);
            _cropParameters.Start = new Coordinate<int>(0, 6);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(_videoStream.Dimensions,
                                                                    _videoStream.StorageDimensions,
                                                                    _cropParameters,
                                                                    null,
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectDivisorToCalculateCropParameters()
        {
            int divisor = 4;

            _tricycleConfig.Video.SizeDivisor = divisor;
            _videoStream.Dimensions = new Dimensions(853, 480);
            _videoStream.StorageDimensions = new Dimensions(720, 480);
            _cropParameters.Size = new Dimensions(720, 464);
            _cropParameters.Start = new Coordinate<int>(0, 6);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<Dimensions>(),
                                                                    Arg.Any<CropParameters>(),
                                                                    Arg.Any<double?>(),
                                                                    divisor);
        }

        [TestMethod]
        public void PassesCorrectCropParametersToCalculateCropParametersForManualCropJob()
        {
            var expectedCropParameters = new CropParameters()
            {
                Size = new Dimensions(1422, 800),
                Start = new Coordinate<int>(248, 138)
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.CropTop = "138";
            _viewModel.CropBottom = "142";
            _viewModel.CropLeft = "248";
            _viewModel.CropRight = "250";
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<Dimensions>(),
                                                                    expectedCropParameters,
                                                                    Arg.Any<double?>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectCropParametersToCalculateCropParametersForEmptyCoordinates()
        {
            var expectedCropParameters = new CropParameters()
            {
                Size = new Dimensions(1920, 1080),
                Start = new Coordinate<int>(0, 0)
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.CropTop = string.Empty;
            _viewModel.CropBottom = string.Empty;
            _viewModel.CropLeft = string.Empty;
            _viewModel.CropRight = string.Empty;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<Dimensions>(),
                                                                    expectedCropParameters,
                                                                    Arg.Any<double?>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectCropParametersToCalculateCropParametersForNullCoordinates()
        {
            var expectedCropParameters = new CropParameters()
            {
                Size = new Dimensions(1920, 1080),
                Start = new Coordinate<int>(0, 0)
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.Dimensions;
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.CropTop = null;
            _viewModel.CropBottom = null;
            _viewModel.CropLeft = null;
            _viewModel.CropRight = null;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<Dimensions>(),
                                                                    expectedCropParameters,
                                                                    Arg.Any<double?>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void TranslatesCropParametersForManualAnamorphicCropJob()
        {
            var expectedCropParameters = new CropParameters()
            {
                Size = new Dimensions(587, 464),
                Start = new Coordinate<int>(66, 6)
            };
            _videoStream.Dimensions = new Dimensions(853, 480);
            _videoStream.StorageDimensions = new Dimensions(720, 480);
            SelectSource();
            _viewModel.SelectedCropOption = new ListItem(CropOption.Manual);
            _viewModel.CropTop = "6";
            _viewModel.CropBottom = "10";
            _viewModel.CropLeft = "78";
            _viewModel.CropRight = "79";
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<Dimensions>(),
                                                                    expectedCropParameters,
                                                                    Arg.Any<double?>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void CallsCalculateCropParametersForAspectRatioJob()
        {
            var aspectRatio = new Dimensions(4, 3);

            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "4:3", aspectRatio }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _videoStream.StorageDimensions = _videoStream.StorageDimensions;
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedAspectRatio = new ListItem(aspectRatio);
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(_videoStream.Dimensions,
                                                                    _videoStream.StorageDimensions,
                                                                    null,
                                                                    4 / 3d,
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void CallsCalculateScaledDimensionsForCustomSizeJob()
        {
            var size = new Dimensions(1280, 720);

            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "720p", size }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedSize = new ListItem(size);
            Start();

            _transcodeCalculator.Received().CalculateScaledDimensions(Arg.Any<Dimensions>(),
                                                                      Arg.Any<Dimensions>(),
                                                                      Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectSourceDimensionsToCalculateScaledDimensionsForCustomSizeJob()
        {
            var size = new Dimensions(1280, 720);

            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "720p", size }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedSize = new ListItem(size);
            Start();

            _transcodeCalculator.Received().CalculateScaledDimensions(_videoStream.Dimensions,
                                                                      Arg.Any<Dimensions>(),
                                                                      Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectSourceDimensionsToCalculateScaledDimensionsForCropJob()
        {
            var sourceDimensions = new Dimensions(1920, 800);
            var size = new Dimensions(1280, 720);

            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "720p", size }
            };
            _videoStream.Dimensions = sourceDimensions;
            _videoStream.StorageDimensions = sourceDimensions;
            _cropParameters.Size = new Dimensions(1920, 804);
            _cropParameters.Start = new Coordinate<int>(0, 138);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            _viewModel.SelectedSize = new ListItem(size);
            _transcodeCalculator.CalculateCropParameters(Arg.Any<Dimensions>(),
                                                         Arg.Any<Dimensions>(),
                                                         Arg.Any<CropParameters>(),
                                                         Arg.Any<double?>(),
                                                         Arg.Any<int>())
                                .Returns(new CropParameters() { Size = sourceDimensions });
            Start();

            _transcodeCalculator.Received().CalculateScaledDimensions(sourceDimensions,
                                                                      Arg.Any<Dimensions>(),
                                                                      Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectTargetDimensionsToCalculateScaledDimensionsForCustomSizeJob()
        {
            var size = new Dimensions(1280, 720);

            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "720p", size }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedSize = new ListItem(size);
            Start();

            _transcodeCalculator.Received().CalculateScaledDimensions(Arg.Any<Dimensions>(),
                                                                      size,
                                                                      Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectDivisorToCalculateScaledDimensions()
        {
            int divisor = 4;
            var size = new Dimensions(1280, 720);

            _tricycleConfig.Video.SizeDivisor = divisor;
            _tricycleConfig.Video.SizePresets = new Dictionary<string, Dimensions>()
            {
                { "720p", size }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedSize = new ListItem(size);
            Start();

            _transcodeCalculator.Received().CalculateScaledDimensions(Arg.Any<Dimensions>(),
                                                                      Arg.Any<Dimensions>(),
                                                                      divisor);
        }

        [TestMethod]
        public void CallsCalculateScaledDimensionsForAnamorphicSource()
        {
            _videoStream.Dimensions = new Dimensions(640, 480);
            _videoStream.StorageDimensions = new Dimensions(720, 480);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            Start();

            _transcodeCalculator.Received().CalculateScaledDimensions(_videoStream.Dimensions,
                                                                      _videoStream.Dimensions,
                                                                      Arg.Any<int>());
        }

        [TestMethod]
        public void SetsSubtitlesForJobWhenSelected()
        {
            var subtitle = new StreamInfo()
            {
                Index = 2,
                StreamType = StreamType.Subtitle,
                Language = "eng"
            };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                subtitle
            };
            SelectSource();
            _viewModel.SelectedSubtitle = new ListItem(subtitle);
            Start();

            Assert.IsNotNull(_transcodeJob.Subtitles);
            Assert.AreEqual(2, _transcodeJob.Subtitles.SourceStreamIndex);
        }

        [TestMethod]
        public void DoesNotSetSubtitlesForJobWhenNotSelected()
        {
            var subtitle = new StreamInfo()
            {
                Index = 2,
                StreamType = StreamType.Subtitle,
                Language = "eng"
            };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                subtitle
            };
            SelectSource();
            Start();

            Assert.IsNull(_transcodeJob.Subtitles);
        }

        [TestMethod]
        public void SetsForcedSubtitlesForJobWhenEnabled()
        {
            var subtitle = new StreamInfo()
            {
                Index = 2,
                StreamType = StreamType.Subtitle,
                Language = "eng"
            };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                subtitle
            };
            SelectSource();
            _viewModel.SelectedSubtitle = new ListItem(subtitle);
            _viewModel.IsForcedSubtitlesChecked = true;
            Start();

            Assert.IsNotNull(_transcodeJob.Subtitles);
            Assert.AreEqual(true, _transcodeJob.Subtitles.ForcedOnly);
        }

        [TestMethod]
        public void DoesNotSetForcedSubtitlesForJobWhenDisabled()
        {
            var subtitle = new StreamInfo()
            {
                Index = 2,
                StreamType = StreamType.Subtitle,
                Language = "eng"
            };

            _mediaInfo.Streams = new StreamInfo[]
            {
                _videoStream,
                subtitle
            };
            SelectSource();
            _viewModel.SelectedSubtitle = new ListItem(subtitle);
            _viewModel.IsForcedSubtitlesChecked = false;
            Start();

            Assert.IsNotNull(_transcodeJob.Subtitles);
            Assert.AreEqual(false, _transcodeJob.Subtitles.ForcedOnly);
        }

        [TestMethod]
        public void SetsCorrectNumberOfAudioOutputsForJob()
        {
            SelectSource();

            var audioViewModel = _viewModel.AudioOutputs.Last();

            audioViewModel.SelectedTrack = audioViewModel.TrackOptions.LastOrDefault();
            Start();

            Assert.AreEqual(2, _transcodeJob?.Streams?.Count(s => s is AudioOutputStream));
        }

        [TestMethod]
        public void SetsAudioSourceStreamIndexForJob()
        {
            _audioStream.Index = 2;
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.OfType<AudioOutputStream>().FirstOrDefault();

            Assert.AreEqual(_audioStream.Index, audioOutput?.SourceStreamIndex);
        }

        [TestMethod]
        public void PassesThruAudioWhenConfiguredTo()
        {
            var format = AudioFormat.Ac3;

            _tricycleConfig.Audio.PassthruMatchingTracks = true;
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    format,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            _audioStream.Format = format;
            _audioStream.ChannelCount = 6;
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.LastOrDefault();

            Assert.AreEqual(typeof(OutputStream), audioOutput?.GetType());
        }

        [TestMethod]
        public void DoesNotPassThruAudioWhenConfiguredNotTo()
        {
            _tricycleConfig.Audio.PassthruMatchingTracks = false;
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Surround5dot1
                            }
                        }
                    }
                }
            };
            _audioStream.FormatName = "ac-3";
            _audioStream.ChannelCount = 6;
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.LastOrDefault();

            Assert.AreEqual(typeof(AudioOutputStream), audioOutput?.GetType());
        }

        [TestMethod]
        public void SetsAudioFormatForJob()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                }
            };
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.OfType<AudioOutputStream>().FirstOrDefault();

            Assert.AreEqual(AudioFormat.Ac3, audioOutput?.Format);
        }

        [TestMethod]
        public void SetsAudioMixdownForJob()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                }
            };
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.OfType<AudioOutputStream>().FirstOrDefault();

            Assert.AreEqual(AudioMixdown.Mono, audioOutput?.Mixdown);
        }

        [TestMethod]
        public void SetsAudioQualityForJob()
        {
            decimal quality = 100;

            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono,
                                Quality = quality
                            }
                        }
                    }
                }
            };
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.OfType<AudioOutputStream>().FirstOrDefault();

            Assert.AreEqual(quality, audioOutput?.Quality);
        }

        [TestMethod]
        public void SetsAudioMetadataForJob()
        {
            _tricycleConfig.Audio.Codecs = new Dictionary<AudioFormat, AudioCodec>()
            {
                {
                    AudioFormat.Ac3,
                    new AudioCodec()
                    {
                        Presets = new AudioPreset[]
                        {
                            new AudioPreset()
                            {
                                Mixdown = AudioMixdown.Mono
                            }
                        }
                    }
                }
            };
            SelectSource();
            Start();

            var audioOutput = _transcodeJob?.Streams?.OfType<AudioOutputStream>().FirstOrDefault();

            Assert.AreEqual("Mono", audioOutput?.Metadata?.GetValueOrDefault("title"));
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageInitially()
        {
            SelectSource();

            Assert.AreEqual("Images/play.png", _viewModel.StartImageSource);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenRunning()
        {
            SelectSource();
            Start();

            Assert.AreEqual("Images/stop.png", _viewModel.StartImageSource);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenStopped()
        {
            SelectSource();
            Start();
            Stop();

            Assert.AreEqual("Images/play.png", _viewModel.StartImageSource);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenJobCompletes()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.AreEqual("Images/play.png", _viewModel.StartImageSource);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenJobFails()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.AreEqual("Images/play.png", _viewModel.StartImageSource);
        }

        [TestMethod]
        public void DisablesControlsWhenRunning()
        {
            SelectSource();
            Start();

            Assert.IsFalse(_viewModel.IsVideoConfigEnabled);
            Assert.IsFalse(_viewModel.IsContainerFormatEnabled);

            foreach (var audio in _viewModel.AudioOutputs)
            {
                Assert.IsFalse(audio.IsEnabled);
            }
        }

        [TestMethod]
        public void EnablesControlsWhenStopped()
        {
            SelectSource();
            Start();
            Stop();

            Assert.IsTrue(_viewModel.IsVideoConfigEnabled);
            Assert.IsTrue(_viewModel.IsContainerFormatEnabled);

            foreach (var audio in _viewModel.AudioOutputs)
            {
                Assert.IsTrue(audio.IsEnabled);
            }
        }

        [TestMethod]
        public void EnablesControlsWhenJobCompletes()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.IsTrue(_viewModel.IsVideoConfigEnabled);
            Assert.IsTrue(_viewModel.IsContainerFormatEnabled);

            foreach (var audio in _viewModel.AudioOutputs)
            {
                Assert.IsTrue(audio.IsEnabled);
            }
        }

        [TestMethod]
        public void EnablesControlsWhenJobFails()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.IsTrue(_viewModel.IsVideoConfigEnabled);
            Assert.IsTrue(_viewModel.IsContainerFormatEnabled);

            foreach (var audio in _viewModel.AudioOutputs)
            {
                Assert.IsTrue(audio.IsEnabled);
            }
        }

        [TestMethod]
        public void DisplaysAlertWhenJobFailsToStart()
        {
            string actualTitle = null;
            string actualMessage = null;

            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            _mediaTranscoder.When(x => x.Start(Arg.Any<TranscodeJob>()))
                            .Do(x => throw new NotSupportedException());
            SelectSource();
            Start();

            Assert.AreEqual("Job Error", actualTitle);
            Assert.AreEqual(@"Oops! Your job couldn't be started for some reason. ¯\_(ツ)_/¯", actualMessage);
        }

        [TestMethod]
        public void DisplaysAlertWhenJobFailsToStop()
        {
            string actualTitle = null;
            string actualMessage = null;

            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            _mediaTranscoder.When(x => x.Stop())
                            .Do(x => throw new NotSupportedException());
            SelectSource();
            Start();
            Stop();

            Assert.AreEqual("Job Error", actualTitle);
            Assert.AreEqual(@"Oops! Your job couldn't be stopped for some reason. ¯\_(ツ)_/¯", actualMessage);
        }

        [TestMethod]
        public void DisplaysProgress()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);

            Assert.AreEqual(status.Percent, _viewModel.Progress);
        }

        [TestMethod]
        public void DisplaysStatus()
        {
            var status = new TranscodeStatus()
            {
                Eta = new TimeSpan(0, 2, 30, 35, 36),
                Speed = 0.225,
                Size = 881852416,
                EstimatedTotalSize = 5583457485,
                Percent = 0.1579
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);

            Assert.AreEqual("Transcoding... 0.23x | 02:30:35 | ~5.2 GB | 15.8%", _viewModel.Status);
        }

        [TestMethod]
        public void DisplaysStatusWithoutEta()
        {
            var status = new TranscodeStatus()
            {
                Speed = 0.225,
                Size = 881852416,
                EstimatedTotalSize = 5583457485,
                Percent = 0.1579
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);

            Assert.AreEqual("Transcoding... 0.23x | ~5.2 GB | 15.8%", _viewModel.Status);
        }

        [TestMethod]
        public void DisplaysStatusWithoutSize()
        {
            var status = new TranscodeStatus()
            {
                Eta = new TimeSpan(0, 2, 30, 35, 36),
                Speed = 0.225,
                Percent = 0.1579
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);

            Assert.AreEqual("Transcoding... 0.23x | 02:30:35 | 15.8%", _viewModel.Status);
        }

        [TestMethod]
        public void ResetsProgressWhenJobCompletes()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.AreEqual(0, _viewModel.Progress);
        }

        [TestMethod]
        public void ResetsProgressWhenJobFails()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.AreEqual(0, _viewModel.Progress);
        }

        [TestMethod]
        public void ResetsProgressWhenJobIsStopped()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            Stop();

            Assert.AreEqual(0, _viewModel.Progress);
        }

        [TestMethod]
        public void ResetsStatusWhenJobCompletes()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.Status));
        }

        [TestMethod]
        public void ResetsStatusWhenJobFails()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.Status));
        }

        [TestMethod]
        public void ResetsStatusWhenJobIsStopped()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            Stop();

            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.Status));
        }

        [TestMethod]
        public void ShowsSpinnerWhenJobIsStarted()
        {
            SelectSource();
            Start();

            Assert.IsTrue(_viewModel.IsSpinnerVisible);
        }

        [TestMethod]
        public void HidesSpinnerWhenJobIsStopped()
        {
            SelectSource();
            Start();
            Stop();

            Assert.IsFalse(_viewModel.IsSpinnerVisible);
        }

        [TestMethod]
        public void HidesSpinnerWhenJobCompletes()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.IsFalse(_viewModel.IsSpinnerVisible);
        }

        [TestMethod]
        public void HidesSpinnerWhenJobFails()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.IsFalse(_viewModel.IsSpinnerVisible);
        }

        [TestMethod]
        public void CallsTranscodeStopForStartCommandWhenRunning()
        {
            SelectSource();
            Start();
            Stop();

            _mediaTranscoder.Received().Stop();
        }

        [TestMethod]
        public void DeletesDestinationWhenJobIsStopped()
        {
            _tricycleConfig.DeleteIncompleteFiles = true;
            SelectSource();
            _fileService.Exists(_viewModel.DestinationName).Returns(true);
            Start();
            Stop();

            _fileService.Received().Delete(_viewModel.DestinationName);
        }

        [TestMethod]
        public void DoesNotDeleteDestinationWhenJobIsStoppedIfDisabled()
        {
            _tricycleConfig.DeleteIncompleteFiles = false;
            SelectSource();
            _fileService.Exists(_viewModel.DestinationName).Returns(true);
            Start();
            Stop();

            _fileService.DidNotReceive().Delete(_viewModel.DestinationName);
        }

        [TestMethod]
        public void DeletesDestinationWhenJobFails()
        {
            _tricycleConfig.DeleteIncompleteFiles = true;
            SelectSource();
            _fileService.Exists(_viewModel.DestinationName).Returns(true);
            Start();

            var destinationName = _viewModel.DestinationName;

            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            _fileService.Received().Delete(destinationName);
        }

        [TestMethod]
        public void DoesNotDeleteDestinationWhenJobFailsIfDisabled()
        {
            _tricycleConfig.DeleteIncompleteFiles = false;
            SelectSource();
            _fileService.Exists(_viewModel.DestinationName).Returns(true);
            Start();

            var destinationName = _viewModel.DestinationName;

            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            _fileService.DidNotReceive().Delete(destinationName);
        }

        [TestMethod]
        public void DisplaysAlertWhenJobFails()
        {
            string actualTitle = null;
            string actualMessage = null;
            string expectedMessage = "Encountered bad sector";

            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            SelectSource();
            Start();
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(expectedMessage);

            Assert.AreEqual("Transcode Failed", actualTitle);
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void DisplaysAlertWhenJobCompletes()
        {
            string actualTitle = null;
            string actualMessage = null;

            _tricycleConfig.CompletionAlert = true;
            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            SelectSource();
            Start();
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.AreEqual("Transcode Complete", actualTitle);
            Assert.AreEqual("Good news! Your shiny, new video is ready.", actualMessage);
        }

        [TestMethod]
        public void DoesNotDisplayAlertWhenJobCompletesIfDisabled()
        {
            bool alerted = false;

            _tricycleConfig.CompletionAlert = false;
            _viewModel.Alert += (title, message) =>
            {
                alerted = true;
            };
            SelectSource();
            Start();
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.IsFalse(alerted);
        }

        [TestMethod]
        public void ConfirmsBeforeJobIsStopped()
        {
            string actualTitle = null;
            string actualMessage = null;

            _viewModel.Confirm += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;

                return Task.FromResult(false);
            };
            SelectSource();
            Start();
            Stop();

            Assert.AreEqual("Stop Transcode", actualTitle);
            Assert.AreEqual(@"Whoa... Are you sure you want to stop and lose your progress?", actualMessage);
        }

        [TestMethod]
        public void StopsTranscodeWhenJobIsStoppedAndConfirmed()
        {
            _viewModel.Confirm += (title, message) => Task.FromResult(true);
            SelectSource();
            Start();
            Stop();

            _mediaTranscoder.Received().Stop();
        }

        [TestMethod]
        public void DoesNotStopTranscodeWhenJobIsStoppedButNotConfirmed()
        {
            _viewModel.Confirm += (title, message) => Task.FromResult(false);
            SelectSource();
            Start();
            Stop();

            _mediaTranscoder.DidNotReceive().Stop();
        }

        [TestMethod]
        public void ConfirmsWhenJobIsRunningAndAppIsQuit()
        {
            string actualTitle = null;
            string actualMessage = null;

            _viewModel.Confirm += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;

                return Task.FromResult(false);
            };
            SelectSource();
            Start();
            _appManager.Quitting += Raise.Event<Action>();

            Assert.AreEqual("Stop Transcode", actualTitle);
            Assert.AreEqual(@"Whoa... Are you sure you want to stop and lose your progress?", actualMessage);
        }

        [TestMethod]
        public void StopsTranscodeWhenAppIsQuitAndConfirmed()
        {
            _viewModel.Confirm += (title, message) => Task.FromResult(true);
            SelectSource();
            Start();
            _appManager.Quitting += Raise.Event<Action>();

            _mediaTranscoder.Received().Stop();
        }

        [TestMethod]
        public void DoesNotStopTranscodeWhenAppIsQuitButNotConfirmed()
        {
            _viewModel.Confirm += (title, message) => Task.FromResult(false);
            SelectSource();
            Start();
            _appManager.Quitting += Raise.Event<Action>();

            _mediaTranscoder.DidNotReceive().Stop();
        }

        [TestMethod]
        public void CallsRaiseQuitConfirmedWhenAppIsQuitAndConfirmed()
        {
            _viewModel.Confirm += (title, message) => Task.FromResult(true);
            SelectSource();
            Start();
            _appManager.Quitting += Raise.Event<Action>();

            _appManager.Received().RaiseQuitConfirmed();
        }

        [TestMethod]
        public void DoesNotCallRaiseQuitConfirmedWhenAppIsQuitButNotConfirmed()
        {
            _viewModel.Confirm += (title, message) => Task.FromResult(false);
            SelectSource();
            Start();
            _appManager.Quitting += Raise.Event<Action>();

            _appManager.DidNotReceive().RaiseQuitConfirmed();
        }

        [TestMethod]
        public void DoesNotCallRaiseQuitConfirmedWhenAppIsQuitAndPageIsNotVisible()
        {
            _viewModel.IsPageVisible = false;
            _appManager.IsModalOpen.Returns(true);
            _appManager.Quitting += Raise.Event<Action>();

            _appManager.DidNotReceive().RaiseQuitConfirmed();
        }

        [TestMethod]
        public void ReadsSourceWhenFileOpenedEventIsRaised()
        {
            string fileName = "/Users/fred/Movies/test.mkv";

            _appManager.FileOpened += Raise.Event<Action<string>>(fileName);

            _mediaInspector.Received().Inspect(fileName);
        }

        [TestMethod]
        public void RaisesFileOpenedEventWhenSourceIsSelected()
        {
            string fileName = "/Users/fred/Movies/test.mkv";

            _fileBrowserResult.FileName = fileName;
            SelectSource();

            _appManager.Received().RaiseFileOpened(fileName);
        }

        [TestMethod]
        public void RaisesBusyEventWhenSourceIsSelected()
        {
            SelectSource();

            _appManager.Received().RaiseBusy();
        }

        [TestMethod]
        public void RaisesReadyEventAfterSourceIsRead()
        {
            SelectSource();

            _appManager.Received().RaiseReady();
        }

        [TestMethod]
        public void RaisesBusyEventWhenJobIsStarted()
        {
            SelectSource();
            _appManager.ClearReceivedCalls();
            Start();

            _appManager.Received().RaiseBusy();
        }

        [TestMethod]
        public void RaisesReadyEventWhenJobIsStopped()
        {
            SelectSource();
            Start();
            _appManager.ClearReceivedCalls();
            Stop();

            _appManager.Received().RaiseReady();
        }

        [TestMethod]
        public void InspectsSourceAgainWhenConfigChanges()
        {
            _mediaInfo.FileName = "test.mkv";
            SelectSource();

            _mediaInspector.ClearReceivedCalls();
            _tricycleConfigManager.ConfigChanged += Raise.Event<Action<TricycleConfig>>(_tricycleConfig);

            _mediaInspector.Received().Inspect(_mediaInfo.FileName);
        }

        [TestMethod]
        public void DetectsSourceAgainWhenConfigChanges()
        {
            SelectSource();

            _cropDetector.ClearReceivedCalls();
            _tricycleConfigManager.ConfigChanged += Raise.Event<Action<TricycleConfig>>(_tricycleConfig);

            _cropDetector.Received().Detect(_mediaInfo);
        }

        #endregion

        #region Helper Methods

        TricycleConfig CreateDefaultTricycleConfig()
        {
            return new TricycleConfig()
            {
                Video = new VideoConfig()
                {
                    Codecs = new Dictionary<VideoFormat, VideoCodec>()
                    {
                        { VideoFormat.Avc, new VideoCodec() },
                        { VideoFormat.Hevc, new VideoCodec() }
                    }
                },
                Audio = new AudioConfig()
                {
                    Codecs = new Dictionary<AudioFormat, AudioCodec>()
                    {
                        {
                            AudioFormat.Aac,
                            new AudioCodec()
                            {
                                Presets = new AudioPreset[]
                                {
                                    new AudioPreset()
                                    {
                                        Mixdown = AudioMixdown.Mono
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        void SelectSource()
        {
            _viewModel.SourceSelectCommand.Execute(null);
        }

        void SelectDestination()
        {
            _viewModel.DestinationSelectCommand.Execute(null);
        }

        void Start()
        {
            _viewModel.StartCommand.Execute(null);
        }

        void Stop()
        {
            _viewModel.StartCommand.Execute(null);
        }

        void ApplyTemplate()
        {
            ApplyTemplate(DEFAULT_TEMPLATE_NAME);
        }

        void ApplyTemplate(string name)
        {
            _appManager.TemplateApplied += Raise.Event<Action<string>>(name);
        }

        void SaveTemplate()
        {
            SaveTemplate(DEFAULT_TEMPLATE_NAME);
        }

        void SaveTemplate(string name)
        {
            _appManager.TemplateSaved += Raise.Event<Action<string>>(name);
        }

        #endregion
    }
}
