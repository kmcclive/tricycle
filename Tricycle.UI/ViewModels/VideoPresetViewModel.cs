using System;
namespace Tricycle.UI.ViewModels
{
    public class VideoPresetViewModel : ViewModelBase
    {
        string _name;
        int? _width;
        int? _height;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int? Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public int? Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
    }
}
