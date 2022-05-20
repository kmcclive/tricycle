using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Tricycle.Media;
using Tricycle.Models.Jobs;
using Tricycle.UI.ViewModels;

namespace Tricycle.UI.Tests.ViewModels;

[TestClass]
public class PreviewViewModelTests
{
    PreviewViewModel _viewModel;
    IPreviewImageGenerator _imageGenerator;
    IFileSystem _fileSystem;
    IFile _fileService;
    IAppManager _appManager;

    [TestInitialize]
    public void Setup()
    {
        _imageGenerator = Substitute.For<IPreviewImageGenerator>();
        _fileSystem = Substitute.For<IFileSystem>();
        _fileService = Substitute.For<IFile>();
        _appManager = Substitute.For<IAppManager>();
        _viewModel = new PreviewViewModel(_imageGenerator, _fileSystem, _appManager, MockDevice.Self)
        {
            IsPageVisible = true
        };

        _fileSystem.File.Returns(_fileService);
    }

    [TestMethod]
    public async Task PassesJobToImageGenerator()
    {
        var job = new TranscodeJob();

        await _viewModel.Load(job);

        await _imageGenerator.Received().Generate(job);
    }

    [TestMethod]
    public async Task DoesNotCallImageGeneratorWhenJobIsNull()
    {
        await _viewModel.Load(null);

        await _imageGenerator.DidNotReceive().Generate(Arg.Any<TranscodeJob>());
    }

    [TestMethod]
    public async Task ShowsSpinnerWhenLoadingJob()
    {
        _imageGenerator.When(x => x.Generate(Arg.Any<TranscodeJob>()))
                       .Do(x => Assert.IsTrue(_viewModel.IsLoading));

        await _viewModel.Load(new TranscodeJob());

        if (!_imageGenerator.ReceivedCalls().Any())
        {
            Assert.Inconclusive("Assert was not called.");
        }
    }

    [TestMethod]
    public async Task HidesImageWhenLoadingJob()
    {
        _imageGenerator.When(x => x.Generate(Arg.Any<TranscodeJob>()))
                       .Do(x => Assert.IsFalse(_viewModel.IsImageVisible));

        await _viewModel.Load(new TranscodeJob());

        if (!_imageGenerator.ReceivedCalls().Any())
        {
            Assert.Inconclusive("Assert was not called.");
        }
    }

    [TestMethod]
    public async Task HidesSpinnerWhenLoadCompletes()
    {
        await _viewModel.Load(new TranscodeJob());

        Assert.IsFalse(_viewModel.IsSpinnerVisible);
    }

    [TestMethod]
    public async Task ShowsImageWhenLoadCompletes()
    {
        await _viewModel.Load(new TranscodeJob());

        Assert.IsTrue(_viewModel.IsImageVisible);
    }

    [TestMethod]
    public async Task SetsCurrentImageSourceWhenLoadSucceeds()
    {
        var fileNames = new string[] { "a", "b", "c" };

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);

        await _viewModel.Load(new TranscodeJob());

        Assert.AreEqual(fileNames[0], _viewModel.CurrentImageSource);
    }

    [TestMethod]
    public async Task ClearsCurrentImageSourceWhenLoadFails()
    {
        var fileNames = new string[] { "a", "b", "c" };

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);
        await _viewModel.Load(new TranscodeJob());

        if (_viewModel.CurrentImageSource == null)
        {
            Assert.Inconclusive("The image source was not set initially.");
        }

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[0]);

        await _viewModel.Load(new TranscodeJob());

        Assert.IsNull(_viewModel.CurrentImageSource);
    }

    [TestMethod]
    public async Task EnablesNextCommandWhenMultipleImagesAreGenerated()
    {
        bool enabled = false;

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[] { "a", "b" });
        _viewModel.NextCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.NextCommand.CanExecute(null);

        await _viewModel.Load(new TranscodeJob());

        Assert.IsTrue(enabled);
    }

    [TestMethod]
    public async Task DisablesNextCommandWhenASingleImageIsGenerated()
    {
        bool enabled = true;

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[] { "a" });
        _viewModel.NextCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.NextCommand.CanExecute(null);

        await _viewModel.Load(new TranscodeJob());

        Assert.IsFalse(enabled);
    }

    [TestMethod]
    public async Task DisablesNextCommandWhenLoading()
    {
        bool enabled = true;

        _viewModel.NextCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.NextCommand.CanExecute(null);
        _imageGenerator.When(x => x.Generate(Arg.Any<TranscodeJob>()))
                       .Do(x => Assert.IsFalse(enabled));

        await _viewModel.Load(new TranscodeJob());

        if (!_imageGenerator.ReceivedCalls().Any())
        {
            Assert.Inconclusive("Assert was not called.");
        }
    }

    [TestMethod]
    public async Task DisablesNextCommandWhenLastImageIsSelected()
    {
        bool enabled = false;

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[] { "a", "b" });
        _viewModel.NextCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.NextCommand.CanExecute(null);

        await _viewModel.Load(new TranscodeJob());

        if (!enabled)
        {
            Assert.Inconclusive("The next command was not enabled initially.");
        }

        _viewModel.NextCommand.Execute(null);

        Assert.IsFalse(enabled);
    }

    [TestMethod]
    public async Task DisablesPreviousCommandWhenLoading()
    {
        bool enabled = true;

        _viewModel.PreviousCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviousCommand.CanExecute(null);
        _imageGenerator.When(x => x.Generate(Arg.Any<TranscodeJob>()))
                       .Do(x => Assert.IsFalse(enabled));

        await _viewModel.Load(new TranscodeJob());

        if (!_imageGenerator.ReceivedCalls().Any())
        {
            Assert.Inconclusive("Assert was not called.");
        }
    }

    [TestMethod]
    public async Task DisablesPreviousCommandWhenLoadIsSuccessful()
    {
        bool enabled = true;

        _viewModel.PreviousCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviousCommand.CanExecute(null);
        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[] { "a", "b", "c" });

        await _viewModel.Load(new TranscodeJob());

        Assert.IsFalse(enabled);
    }

    [TestMethod]
    public async Task EnablesPreviousCommandWhenNextImageIsSelected()
    {
        bool enabled = false;

        _viewModel.PreviousCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviousCommand.CanExecute(null);
        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[] { "a", "b", "c" });

        await _viewModel.Load(new TranscodeJob());

        _viewModel.NextCommand.Execute(null);

        Assert.IsTrue(enabled);
    }

    [TestMethod]
    public async Task DisablesPreviousCommandWhenFirstImageIsSelected()
    {
        bool enabled = false;

        _viewModel.PreviousCommand.CanExecuteChanged += (s, e) => enabled = _viewModel.PreviousCommand.CanExecute(null);
        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[] { "a", "b", "c" });
        await _viewModel.Load(new TranscodeJob());

        _viewModel.NextCommand.Execute(null);

        if (!enabled)
        {
            Assert.Inconclusive("The previous command was not enabled initially.");
        }

        _viewModel.PreviousCommand.Execute(null);

        Assert.IsFalse(enabled);
    }

    [TestMethod]
    public async Task UpdatesCurrentImageSourceWhenNextImageIsSelected()
    {
        var fileNames = new string[] { "a", "b", "c" };

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);
        await _viewModel.Load(new TranscodeJob());

        _viewModel.NextCommand.Execute(null);

        Assert.AreEqual(fileNames[1], _viewModel.CurrentImageSource);
    }

    [TestMethod]
    public async Task UpdatesCurrentImageSourceWhenPreviousImageIsSelected()
    {
        var fileNames = new string[] { "a", "b", "c" };

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);
        await _viewModel.Load(new TranscodeJob());
        _viewModel.NextCommand.Execute(null);

        if (_viewModel.CurrentImageSource != fileNames[1])
        {
            Assert.Inconclusive("The image source was not updated by the next command.");
        }

        _viewModel.PreviousCommand.Execute(null);

        Assert.AreEqual(fileNames[0], _viewModel.CurrentImageSource);
    }

    [TestMethod]
    public async Task DisplaysAlertWhenLoadFails()
    {
        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(new string[0]);

        await _viewModel.Load(new TranscodeJob());

        _appManager.Received().Alert("Preview Error",
                                     @"Oops! Your preview didn't show up for some reason. ¯\_(ツ)_/¯",
                                     Severity.Error);
    }

    [TestMethod]
    public async Task DeletesFilesWhenClosing()
    {
        var fileNames = new string[] { "a", "b", "c" };

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);
        await _viewModel.Load(new TranscodeJob());
        _fileService.Exists(Arg.Any<string>()).Returns(true);

        _viewModel.Close();

        foreach (var fileName in fileNames)
        {
            _fileService.Received().Delete(fileName);
        }
    }

    [TestMethod]
    public async Task DoesNotDeleteFilesThatDontExist()
    {
        var fileNames = new string[] { "a", "b", "c" };
        var doesNotExist = fileNames[1];

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);
        await _viewModel.Load(new TranscodeJob());
        _fileService.Exists(Arg.Any<string>()).Returns(true);
        _fileService.Exists(doesNotExist).Returns(false);

        _viewModel.Close();

        foreach (var fileName in fileNames)
        {
            if (fileName == doesNotExist)
            {
                _fileService.DidNotReceive().Delete(fileName);
            }
            else
            {
                _fileService.Received().Delete(fileName);
            }
        }
    }

    [TestMethod]
    public void RaisesModalClosedEventWhenBackButtonIsPressed()
    {
        _viewModel.BackCommand.Execute(null);

        _appManager.Received().RaiseModalClosed();
    }

    [TestMethod]
    public async Task DeletesFilesWhenAppIsQuitting()
    {
        var fileNames = new string[] { "a", "b", "c" };

        _imageGenerator.Generate(Arg.Any<TranscodeJob>()).Returns(fileNames);
        await _viewModel.Load(new TranscodeJob());
        _fileService.Exists(Arg.Any<string>()).Returns(true);

        _appManager.Quitting += Raise.Event<Action>();

        foreach (var fileName in fileNames)
        {
            _fileService.Received().Delete(fileName);
        }
    }

    [TestMethod]
    public void RaisesQuitConfirmedEventWhenVisibleAndAppIsQuitting()
    {
        _appManager.Quitting += Raise.Event<Action>();

        _appManager.Received().RaiseQuitConfirmed();
    }

    [TestMethod]
    public void DoesNotRaiseQuitConfirmedEventWhenNotVisibleAndAppIsQuitting()
    {
        _viewModel.IsPageVisible = false;
        _appManager.Quitting += Raise.Event<Action>();

        _appManager.DidNotReceive().RaiseQuitConfirmed();
    }
}
