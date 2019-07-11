using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.IO;
using Tricycle.IO.Models;
using Tricycle.Media;
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
    }
}
