using System;
using System.IO;
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
        MainViewModel _viewModel;
        IFileBrowser _fileBrowser;
        IMediaInspector _mediaInspector;
        ICropDetector _cropDetector;
        TricycleConfig _tricycleConfig;
        string _defaultDestinationDirectory;

        [TestInitialize]
        public void Setup()
        {
            _fileBrowser = Substitute.For<IFileBrowser>();
            _mediaInspector = Substitute.For<IMediaInspector>();
            _cropDetector = Substitute.For<ICropDetector>();
            _tricycleConfig = new TricycleConfig();
            _defaultDestinationDirectory = Path.Combine("Users", "fred", "Movies");
            _viewModel = new MainViewModel(_fileBrowser,
                                           _mediaInspector,
                                           _cropDetector,
                                           _tricycleConfig,
                                           _defaultDestinationDirectory);
        }

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
        public void SourceSelectionOpensFileBrowser()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult());
            _viewModel.SourceSelectCommand.Execute(null);
            _fileBrowser.Received().BrowseToOpen();
        }

        [TestMethod]
        public void CancellingFileBrowserDoesNotReadSource()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = false
            });
            _viewModel.SourceSelectCommand.Execute(null);
            _mediaInspector.DidNotReceive().Inspect(Arg.Any<string>());
        }

        [TestMethod]
        public void ConfirmingFileBrowserReadsCorrectSource()
        {
            string fileName = "test.mkv";

            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = fileName
            });
            _viewModel.SourceSelectCommand.Execute(null);
            _mediaInspector.Received().Inspect(fileName);
        }

        [TestMethod]
        public void DisplaysAlertWhenSourceInfoIsNull()
        {
            string actualTitle = null;
            string actualMessage = null;

            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(default(MediaInfo));
            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("Invalid Source", actualTitle);
            Assert.AreEqual("The selected file could not be opened.", actualMessage);
        }

        [TestMethod]
        public void DisplaysAlertWhenSourceHasNoVideo()
        {
            string actualTitle = null;
            string actualMessage = null;

            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new AudioStreamInfo()
                }
            });
            _viewModel.Alert += (title, message) =>
            {
                actualTitle = title;
                actualMessage = message;
            };
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("Invalid Source", actualTitle);
            Assert.AreEqual("The selected file could not be opened.", actualMessage);
        }

        [TestMethod]
        public void ShowsSourceInfoWhenSourceIsValid()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.IsTrue(_viewModel.IsSourceInfoVisible);
        }

        [TestMethod]
        public void DisplaysCorrectSourceName()
        {
            var fileName = "test.mkv";

            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = fileName
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                FileName = fileName,
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual(fileName, _viewModel.SourceName);
        }

        [TestMethod]
        public void DisplaysCorrectSourceDuration()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Duration = new TimeSpan(1, 42, 17),
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("01:42:17", _viewModel.SourceDuration);
        }

        [TestMethod]
        public void Displays4KSourceSizeFor3840Width()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(3840, 1646)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("4K", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays4KSourceSizeFor2160Height()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(2880, 2160)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("4K", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays1080pSourceSizeFor1920Width()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(1920, 822)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("1080p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays1080pSourceSizeFor1080Height()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(1440, 1080)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("1080p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays720pSourceSizeFor1280Width()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(1280, 548)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("720p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays720pSourceSizeFor720Height()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(960, 720)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("720p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays480pSourceSizeFor853Width()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(853, 366)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("480p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void Displays480pSourceSizeFor480Height()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(640, 480)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("480p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void DisplaysCustomSourceSize()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(568, 320)
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.AreEqual("320p", _viewModel.SourceSize);
        }

        [TestMethod]
        public void ShowsHdrLabelForHdr()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        DynamicRange = DynamicRange.High
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.IsTrue(_viewModel.IsSourceHdr);
        }

        [TestMethod]
        public void HidesHdrLabelForSdr()
        {
            _fileBrowser.BrowseToOpen().Returns(new FileBrowserResult()
            {
                Confirmed = true,
                FileName = "test.mkv"
            });
            _mediaInspector.Inspect(Arg.Any<string>()).Returns(new MediaInfo()
            {
                Streams = new StreamInfo[]
                {
                    new VideoStreamInfo()
                    {
                        DynamicRange = DynamicRange.Standard
                    }
                }
            });
            _viewModel.SourceSelectCommand.Execute(null);

            Assert.IsFalse(_viewModel.IsSourceHdr);
        }
    }
}
