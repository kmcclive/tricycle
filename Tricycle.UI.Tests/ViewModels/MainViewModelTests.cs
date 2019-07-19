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
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;
using Tricycle.UI.Models;
using Tricycle.UI.ViewModels;
using Tricycle.Utilities;

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
        AudioStreamInfo _audioStream;
        MediaInfo _mediaInfo;
        IMediaTranscoder _mediaTranscoder;
        ICropDetector _cropDetector;
        CropParameters _cropParameters;
        ITranscodeCalculator _transcodeCalculator;
        IFileSystem _fileSystem;
        IFile _fileService;
        TricycleConfig _tricycleConfig;
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
            _transcodeCalculator = Substitute.For<ITranscodeCalculator>();
            _fileSystem = Substitute.For<IFileSystem>();
            _tricycleConfig = CreateDefaultTricycleConfig();
            _defaultDestinationDirectory = Path.Combine("Users", "fred", "Movies");
            _viewModel = new MainViewModel(_fileBrowser,
                                           _mediaInspector,
                                           _mediaTranscoder,
                                           _cropDetector,
                                           _transcodeCalculator,
                                           _fileSystem,
                                           MockDevice.Self,
                                           _tricycleConfig,
                                           _defaultDestinationDirectory);

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

            _fileBrowser.BrowseToOpen().Returns(_fileBrowserResult);
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(_mediaInfo);
            _mediaTranscoder.When(x => x.Start(Arg.Any<TranscodeJob>())).Do(x => _transcodeJob = (TranscodeJob)x[0]);
            _cropDetector.Detect(Arg.Any<MediaInfo>()).Returns(_cropParameters);
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
                Codecs = new VideoCodec[0]
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
                Codecs = new VideoCodec[]
                {
                    new VideoCodec()
                    {
                        Format = VideoFormat.Hevc
                    }
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

            _tricycleConfig.Video.Codecs.First(c => c.Format == VideoFormat.Avc).QualitySteps = stepCount;
            SelectSource();

            Assert.AreEqual(6, _viewModel.QualityStepCount);
        }

        [TestMethod]
        public void SetsQualityStepCountWhenVideoFormatIsChanged()
        {
            var stepCount = 6;

            _tricycleConfig.Video.Codecs.First(c => c.Format == VideoFormat.Hevc).QualitySteps = stepCount;
            SelectSource();
            _viewModel.SelectedVideoFormat = new ListItem("HEVC", VideoFormat.Hevc);

            Assert.AreEqual(6, _viewModel.QualityStepCount);
        }

        [TestMethod]
        public void SetsQualityToMedianByDefault()
        {
            var stepCount = 4;

            _tricycleConfig.Video.Codecs.First(c => c.Format == VideoFormat.Avc).QualitySteps = stepCount;
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
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.IsFalse(_viewModel.IsAutocropEnabled);
        }

        [TestMethod]
        public void UnchecksAutocropWhenNoBarsAreFound()
        {
            _videoStream.Dimensions = new Dimensions(3840, 2160);
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
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.AreEqual(3, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("16:9", _viewModel.AspectRatioOptions[1]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitAspectRatioOptionsBasedOnSource()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "21:9", new Dimensions(21, 9) },
                { "16:9", new Dimensions(16, 9) },
                { "4:3", new Dimensions(4, 3) }
            };
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _cropParameters.Size = _videoStream.Dimensions;
            SelectSource();

            Assert.AreEqual(3, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("16:9", _viewModel.AspectRatioOptions[1]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[2]?.Name);
        }

        [TestMethod]
        public void LimitAspectRatioOptionsBasedOnBars()
        {
            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "16:9", new Dimensions(16, 9) },
                { "4:3", new Dimensions(4, 3) }
            };
            _videoStream.Dimensions = new Dimensions(3840, 2160);
            _cropParameters.Size = new Dimensions(2880, 2160);
            SelectSource();

            Assert.AreEqual(2, _viewModel.AspectRatioOptions?.Count);
            Assert.AreEqual("Same as source", _viewModel.AspectRatioOptions[0]?.Name);
            Assert.AreEqual("4:3", _viewModel.AspectRatioOptions[1]?.Name);
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[0];
            SelectSource();

            Assert.IsTrue(_viewModel.AudioOutputs?.Any() != true);
        }

        [TestMethod]
        public void ListsNoAudioOutputsWhenNoCodecsHavePresets()
        {
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
                    Presets = new AudioPreset[0]
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

            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Surround5dot1
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
            _audioStream.FormatName = "aac";
            _audioStream.ChannelCount = 2;
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: AAC Stereo", audioOutput?.SelectedTrack?.Name);
        }

        [TestMethod]
        public void DisplaysCorrectNameForAc3AudioTrack()
        {
            _audioStream.FormatName = "ac-3";
            _audioStream.ChannelCount = 6;
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("1: Dolby Digital 5.1", audioOutput?.SelectedTrack?.Name);
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
                        }
                    }
                },
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
                        }
                    }
                },
                new AudioCodec()
                {
                    Format = AudioFormat.HeAac,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
                        }
                    }
                },
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Stereo
                        }
                    }
                },
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Surround5dot1
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
                        }
                    }
                },
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("AAC", audioOutput?.SelectedFormat?.Name);
        }

        [TestMethod]
        public void PopulatesAudioMixdownOptionsInOrder()
        {
            _audioStream.ChannelCount = 8;
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
                        }
                    }
                },
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Aac,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
                        }
                    }
                },
            };
            SelectSource();

            var audioOutput = _viewModel.AudioOutputs?.FirstOrDefault();

            Assert.AreEqual("Mono", audioOutput?.SelectedMixdown?.Name);
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
                Codecs = new VideoCodec[]
                {
                    new VideoCodec()
                    {
                        Format = format,
                        QualityRange = new Range<decimal>(25, 15),
                        QualitySteps = 4
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
        public void CallsCalculateCropParametersForCropJob()
        {
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<CropParameters>(),
                                                                    Arg.Any<double>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectSourceDimensionsToCalculateCropParametersForCropJob()
        {
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(_videoStream.Dimensions,
                                                                    Arg.Any<CropParameters>(),
                                                                    Arg.Any<double>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectCropParametersToCalculateCropParametersForCropJob()
        {
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    _cropParameters,
                                                                    Arg.Any<double>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectAspectRatioToCalculateCropParametersForCropJob()
        {
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<CropParameters>(),
                                                                    2.4,
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectDivisorToCalculateCropParameters()
        {
            int divisor = 4;

            _tricycleConfig.Video.SizeDivisor = divisor;
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _cropParameters.Size = new Dimensions(1920, 800);
            _cropParameters.Start = new Coordinate<int>(0, 140);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<CropParameters>(),
                                                                    Arg.Any<double>(),
                                                                    divisor);
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
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedAspectRatio = new ListItem(aspectRatio);
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<CropParameters>(),
                                                                    Arg.Any<double>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectSourceDimensionsToCalculateCropParametersForAspectRatioJob()
        {
            var aspectRatio = new Dimensions(4, 3);

            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "4:3", aspectRatio }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedAspectRatio = new ListItem(aspectRatio);
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(_videoStream.Dimensions,
                                                                    Arg.Any<CropParameters>(),
                                                                    Arg.Any<double>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectCropParametersToCalculateCropParametersForAspectRatioJob()
        {
            var aspectRatio = new Dimensions(4, 3);

            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "4:3", aspectRatio }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedAspectRatio = new ListItem(aspectRatio);
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    null,
                                                                    Arg.Any<double>(),
                                                                    Arg.Any<int>());
        }

        [TestMethod]
        public void PassesCorrectAspectRatioToCalculateCropParametersForAspectRatioJob()
        {
            var aspectRatio = new Dimensions(4, 3);

            _tricycleConfig.Video.AspectRatioPresets = new Dictionary<string, Dimensions>()
            {
                { "4:3", aspectRatio }
            };
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            SelectSource();
            _viewModel.IsAutocropChecked = false;
            _viewModel.SelectedAspectRatio = new ListItem(aspectRatio);
            Start();

            _transcodeCalculator.Received().CalculateCropParameters(Arg.Any<Dimensions>(),
                                                                    Arg.Any<CropParameters>(),
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
            _videoStream.Dimensions = new Dimensions(1920, 1080);
            _cropParameters.Size = new Dimensions(1920, 804);
            _cropParameters.Start = new Coordinate<int>(0, 138);
            SelectSource();
            _viewModel.IsAutocropChecked = true;
            _viewModel.SelectedSize = new ListItem(size);
            _transcodeCalculator.CalculateCropParameters(Arg.Any<Dimensions>(),
                                                         Arg.Any<CropParameters>(),
                                                         Arg.Any<double>(),
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
            _tricycleConfig.Audio.PassthruMatchingTracks = true;
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Surround5dot1
                        }
                    }
                }
            };
            _audioStream.FormatName = "ac-3";
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Surround5dot1
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
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
            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono
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

            _tricycleConfig.Audio.Codecs = new AudioCodec[]
            {
                new AudioCodec()
                {
                    Format = AudioFormat.Ac3,
                    Presets = new AudioPreset[]
                    {
                        new AudioPreset()
                        {
                            Mixdown = AudioMixdown.Mono,
                            Quality = quality
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
        public void DisplaysCorrectButtonImageInitially()
        {
            SelectSource();

            Assert.AreEqual("Images/play.png", _viewModel.ToggleStartImage);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenRunning()
        {
            SelectSource();
            Start();

            Assert.AreEqual("Images/stop.png", _viewModel.ToggleStartImage);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenStopped()
        {
            SelectSource();
            Start();
            Stop();

            Assert.AreEqual("Images/play.png", _viewModel.ToggleStartImage);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenJobCompletes()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.AreEqual("Images/play.png", _viewModel.ToggleStartImage);
        }

        [TestMethod]
        public void DisplaysCorrectButtonImageWhenJobFails()
        {
            SelectSource();
            Start();
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.AreEqual("Images/play.png", _viewModel.ToggleStartImage);
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
        public void DisplaysAlerWhenJobFailsToStart()
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
        public void DisplaysAlerWhenJobFailsToStop()
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
        public void DisplaysProgressText()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.34567,
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);

            Assert.AreEqual("34.57%", _viewModel.ProgressText);
        }

        [TestMethod]
        public void DisplaysRateText()
        {
            var status = new TranscodeStatus()
            {
                Speed = 0.225
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);

            Assert.AreEqual("0.225x", _viewModel.RateText);
        }

        [TestMethod]
        public void ResetsProgressWhenJobCompletes()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225,
                FramesPerSecond = 5.39
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            _mediaTranscoder.Completed += Raise.Event<Action>();

            Assert.IsFalse(_viewModel.IsProgressVisible);
            Assert.AreEqual(0, _viewModel.Progress);
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.ProgressText));
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.RateText));
        }

        [TestMethod]
        public void ResetsProgressWhenJobFails()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225,
                FramesPerSecond = 5.39
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            Assert.IsFalse(_viewModel.IsProgressVisible);
            Assert.AreEqual(0, _viewModel.Progress);
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.ProgressText));
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.RateText));
        }

        [TestMethod]
        public void ResetsProgressWhenJobIsStopped()
        {
            var status = new TranscodeStatus()
            {
                Percent = 0.3,
                Speed = 0.225,
                FramesPerSecond = 5.39
            };

            SelectSource();
            Start();
            _mediaTranscoder.StatusChanged += Raise.Event<Action<TranscodeStatus>>(status);
            Stop();

            Assert.IsFalse(_viewModel.IsProgressVisible);
            Assert.AreEqual(0, _viewModel.Progress);
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.ProgressText));
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.RateText));
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
            SelectSource();
            _fileService.Exists(_viewModel.DestinationName).Returns(true);
            Start();
            Stop();

            _fileService.Received().Delete(_viewModel.DestinationName);
        }

        [TestMethod]
        public void DeletesDestinationWhenJobFails()
        {
            SelectSource();
            _fileService.Exists(_viewModel.DestinationName).Returns(true);
            Start();

            var destinationName = _viewModel.DestinationName;

            _mediaTranscoder.Failed += Raise.Event<Action<string>>(string.Empty);

            _fileService.Received().Delete(destinationName);
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
            Assert.AreEqual("Good news! Your shiny new video is ready.", actualMessage);
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

        #endregion

        #region Helper Methods

        TricycleConfig CreateDefaultTricycleConfig()
        {
            return new TricycleConfig()
            {
                Video = new VideoConfig()
                {
                    Codecs = new VideoCodec[]
                    {
                        new VideoCodec()
                        {
                            Format = VideoFormat.Avc
                        },
                        new VideoCodec()
                        {
                            Format = VideoFormat.Hevc
                        }
                    }
                },
                Audio = new AudioConfig()
                {
                    Codecs = new AudioCodec[]
                    {
                        new AudioCodec()
                        {
                            Format = AudioFormat.Aac,
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

        #endregion
    }
}
