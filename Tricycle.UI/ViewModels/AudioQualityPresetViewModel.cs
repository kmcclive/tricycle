using System;
using System.Collections.Generic;
using System.Windows.Input;
using Tricycle.UI.Models;
using Xamarin.Forms;

namespace Tricycle.UI.ViewModels
{
    public class AudioQualityPresetViewModel : ViewModelBase
    {
        IList<ListItem> _formatOptions;
        ListItem _selectedFormat;
        IList<ListItem> _mixdownOptions;
        ListItem _selectedMixdown;
        decimal? _quality;
        bool _canRemove;

        public AudioQualityPresetViewModel()
        {
            RemoveCommand = new Command(
                () => Removed?.Invoke(),
                () =>
                {
                    if (!_canRemove)
                    {
                        _canRemove = SelectedFormat != null || SelectedMixdown != null || Quality.HasValue;
                    }

                    return _canRemove;
                });
        }

        public IList<ListItem> FormatOptions
        {
            get => _formatOptions;
            set => SetProperty(ref _formatOptions, value);
        }

        public ListItem SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                if (value != _selectedFormat)
                {
                    SetProperty(ref _selectedFormat, value);
                    ((Command)RemoveCommand).ChangeCanExecute();
                    Modified?.Invoke();
                }
            }
        }

        public IList<ListItem> MixdownOptions
        {
            get => _mixdownOptions;
            set => SetProperty(ref _mixdownOptions, value);
        }

        public ListItem SelectedMixdown
        {
            get => _selectedMixdown;
            set
            {
                if (value != _selectedMixdown)
                {
                    SetProperty(ref _selectedMixdown, value);
                    ((Command)RemoveCommand).ChangeCanExecute();
                    Modified?.Invoke();
                }
            }
        }

        public decimal? Quality
        {
            get => _quality;
            set
            {
                if (value != _quality)
                {
                    SetProperty(ref _quality, value);
                    ((Command)RemoveCommand).ChangeCanExecute();
                    Modified?.Invoke();
                }
            }
        }

        public ICommand RemoveCommand { get; }

        public event Action Modified;
        public event Action Removed;
    }
}
