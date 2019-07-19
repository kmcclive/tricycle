using System;
using System.Collections.Generic;
using Tricycle.UI.Models;

namespace Tricycle.UI.ViewModels
{
    public class AudioOutputViewModel : ViewModelBase
    {
        bool _isEnabled = true;
        IList<ListItem> _trackOptions;
        ListItem _selectedTrack;
        IList<ListItem> _formatOptions;
        ListItem _selectedFormat;
        IList<ListItem> _mixdownOptions;
        ListItem _selectedMixdown;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public IList<ListItem> TrackOptions
        {
            get { return _trackOptions; }
            set { SetProperty(ref _trackOptions, value); }
        }

        public ListItem SelectedTrack
        {
            get { return _selectedTrack; }
            set
            {
                var oldItem = _selectedTrack;

                if (!object.Equals(oldItem, value))
                {
                    SetProperty(ref _selectedTrack, value);

                    TrackSelected?.Invoke(this, new ItemChangedEventArgs(oldItem, value));
                }
            }
        }

        public IList<ListItem> FormatOptions
        {
            get { return _formatOptions; }
            set { SetProperty(ref _formatOptions, value); }
        }

        public ListItem SelectedFormat
        {
            get { return _selectedFormat; }
            set
            {
                var oldItem = _selectedFormat;

                if (!object.Equals(oldItem, value))
                {
                    SetProperty(ref _selectedFormat, value);

                    FormatSelected?.Invoke(this, new ItemChangedEventArgs(oldItem, value));
                }
            }
        }

        public IList<ListItem> MixdownOptions
        {
            get { return _mixdownOptions; }
            set { SetProperty(ref _mixdownOptions, value); }
        }

        public ListItem SelectedMixdown
        {
            get { return _selectedMixdown; }
            set { SetProperty(ref _selectedMixdown, value); }
        }

        public event EventHandler<ItemChangedEventArgs> TrackSelected;
        public event EventHandler<ItemChangedEventArgs> FormatSelected;
    }
}
