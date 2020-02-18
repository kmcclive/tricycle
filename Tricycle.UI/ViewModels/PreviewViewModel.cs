using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tricycle.Media;
using Tricycle.Models.Jobs;
using Xamarin.Forms;

namespace Tricycle.UI.ViewModels
{
    public class PreviewViewModel : ViewModelBase
    {
        #region Fields

        readonly IPreviewImageGenerator _imageGenerator;
        readonly IFileSystem _fileSystem;
        readonly IAppManager _appManager;
        readonly IDevice _device;

        string _currentImageSource;
        bool _isLoading;
        string _status;

        IList<string> _imageFileNames;
        int _currentIndex;

        #endregion

        #region Constructors

        public PreviewViewModel(IPreviewImageGenerator imageGenerator,
                                IFileSystem fileSystem,
                                IAppManager appManager,
                                IDevice device)
        {
            _imageGenerator = imageGenerator;
            _fileSystem = fileSystem;
            _appManager = appManager;
            _device = device;

            _appManager.Quitting += OnAppQuitting;

            BackCommand = new Command(() => _appManager.RaiseModalClosed());
            PreviousCommand = new Command(Previous, () => _currentIndex > 0);
            NextCommand = new Command(Next, () => _currentIndex < (_imageFileNames?.Count ?? 0) - 1);
        }

        #endregion

        #region Properties

        public bool IsPageVisible { get; set; }

        public string CurrentImageSource
        {
            get => _currentImageSource;
            set => SetProperty(ref _currentImageSource, value);
        }

        public bool IsImageVisible => !_isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                RaisePropertyChanged(nameof(IsImageVisible));
                RaisePropertyChanged(nameof(IsSpinnerVisible));

                if (_isLoading)
                {
                    _appManager.RaiseBusy();
                } else
                {
                    _appManager.RaiseReady();
                }
            }
        }

        public bool IsSpinnerVisible => _isLoading;

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ICommand PreviousCommand { get; }

        public ICommand NextCommand { get; }

        public ICommand BackCommand { get; }

        #endregion

        #region Methods

        #region Public

        public async Task Load(TranscodeJob job)
        {
            _imageFileNames = null;
            _currentIndex = 0;

            _device.BeginInvokeOnMainThread(() =>
            {
                IsLoading = true;
                Status = "Generating preview...";
                RefreshButtons();

                CurrentImageSource = null;
            });
            
            if (job != null)
            {
                try
                {
                    _imageFileNames = await _imageGenerator.Generate(job);
                }
                catch (ArgumentException) { }
                catch (NotSupportedException) { }
            }

            _device.BeginInvokeOnMainThread(() =>
            {
                CurrentImageSource = _imageFileNames?.FirstOrDefault();

                RefreshButtons();
                IsLoading = false;
                Status = "Preview";

                if (_imageFileNames?.Any() != true)
                {
                    _appManager.Alert("Preview Error", @"Oops! Your preview didn't show up for some reason. ¯\_(ツ)_/¯");
                }
            });
        }

        public void Close()
        {
            DeleteImages();
        }

        #endregion

        #region Command Actions

        void Previous()
        {
            _currentIndex--;
            CurrentImageSource = _imageFileNames[_currentIndex];

            RefreshButtons();
        }

        void Next()
        {
            _currentIndex++;
            CurrentImageSource = _imageFileNames[_currentIndex];

            RefreshButtons();
        }

        #endregion

        #region Helpers

        void RefreshButtons()
        {
            ((Command)PreviousCommand).ChangeCanExecute();
            ((Command)NextCommand).ChangeCanExecute();
        }

        void DeleteImages()
        {
            if (_imageFileNames == null)
            {
                return;
            }

            foreach (var fileName in _imageFileNames)
            {
                try
                {
                    if (_fileSystem.File.Exists(fileName))
                    {
                        _fileSystem.File.Delete(fileName);
                    }
                }
                catch (ArgumentException) { }
                catch (NotSupportedException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        #endregion

        #region Event Handlers

        void OnAppQuitting()
        {
            if (IsPageVisible)
            {
                DeleteImages();

                // This raises the event outside of the current closing call stack
                _device.StartTimer(TimeSpan.FromTicks(1), () =>
                {
                    _appManager.RaiseQuitConfirmed();
                    return false;
                });
            }
        }

        #endregion

        #endregion
    }
}
