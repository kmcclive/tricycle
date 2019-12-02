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
        string _quality;
        bool _isRemoveEnabled;

        public AudioQualityPresetViewModel()
        {
            RemoveCommand = new Command(() => Removed?.Invoke(), () => IsRemoveEnabled);
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
                    Modified?.Invoke();
                }
            }
        }

        public string Quality
        {
            get => _quality;
            set
            {
                if (value != _quality)
                {
                    SetProperty(ref _quality, value);
                    Modified?.Invoke();
                }
            }
        }

        public bool IsRemoveEnabled
        {
            get => _isRemoveEnabled;
            set
            {
                SetProperty(ref _isRemoveEnabled, value);
                ((Command)RemoveCommand).ChangeCanExecute();
            }
        }

        public ICommand RemoveCommand { get; }

        public event Action Modified;
        public event Action Removed;

        public void ClearHandlers()
        {
            if (Modified != null)
            {
                foreach (Action handler in Modified.GetInvocationList())
                {
                    Modified -= handler;
                }
            }

            if (Removed != null)
            {
                foreach (Action handler in Removed.GetInvocationList())
                {
                    Removed -= handler;
                }
            }
        }
    }
}
