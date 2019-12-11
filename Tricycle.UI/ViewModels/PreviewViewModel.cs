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
        readonly IPreviewImageGenerator _imageGenerator;
        readonly IFileSystem _fileSystem;
        readonly IDevice _device;

        string _currentImageSource;
        bool _isSpinnerVisible;
        bool _isImageVisible;

        IList<string> _imageFileNames;
        int _currentIndex;

        public PreviewViewModel(IPreviewImageGenerator imageGenerator, IFileSystem fileSystem, IDevice device)
        {
            _imageGenerator = imageGenerator;
            _fileSystem = fileSystem;
            _device = device;

            CloseCommand = new Command(Close);
            PreviousCommand = new Command(Previous, () => _currentIndex > 0);
            NextCommand = new Command(Next, () => _currentIndex < (_imageFileNames?.Count ?? 0) - 1);
        }

        public string CurrentImageSource
        {
            get => _currentImageSource;
            set => SetProperty(ref _currentImageSource, value);
        }

        public bool IsSpinnerVisible
        {
            get => _isSpinnerVisible;
            set => SetProperty(ref _isSpinnerVisible, value);
        }

        public bool IsImageVisible
        {
            get => _isImageVisible;
            set => SetProperty(ref _isImageVisible, value);
        }

        public ICommand CloseCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }

        public event Action Closed;

        public async Task Load(TranscodeJob job)
        {
            _imageFileNames = null;
            _currentIndex = 0;

            _device.BeginInvokeOnMainThread(() =>
            {
                SetLoading(true);

                CurrentImageSource = null;

                RefreshButtons();
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
                SetLoading(false);
            });
        }

        void Close()
        {
            if (_imageFileNames != null)
            {
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

            Closed?.Invoke();
        }

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

        void RefreshButtons()
        {
            ((Command)PreviousCommand).ChangeCanExecute();
            ((Command)NextCommand).ChangeCanExecute();
        }

        void SetLoading(bool isLoading)
        {
            IsSpinnerVisible = isLoading;
            IsImageVisible = !isLoading;
        }
    }
}
