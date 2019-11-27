using System;
using System.Collections.Generic;
using Tricycle.UI.Models;

namespace Tricycle.UI.ViewModels
{
    public class AudioQualityPresetViewModel : ViewModelBase
    {
        IList<ListItem> _formatOptions;
        ListItem _selectedFormat;
        IList<ListItem> _mixdownOptions;
        ListItem _selectedMixdown;
        decimal _quality;

        public IList<ListItem> FormatOptions
        {
            get => _formatOptions;
            set => SetProperty(ref _formatOptions, value);
        }

        public ListItem SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }

        public IList<ListItem> MixdownOptions
        {
            get => _mixdownOptions;
            set => SetProperty(ref _mixdownOptions, value);
        }

        public ListItem SelectedMixdown
        {
            get => _selectedMixdown;
            set => SetProperty(ref _selectedMixdown, value);
        }

        public decimal Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }
    }
}
