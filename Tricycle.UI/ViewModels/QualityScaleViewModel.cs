using System;
namespace Tricycle.UI.ViewModels
{
    public class QualityScaleViewModel : ViewModelBase
    {
        decimal? _min;
        decimal? _max;
        int? _stepCount;

        public decimal? Min
        {
            get => _min;
            set => SetProperty(ref _min, value);
        }

        public decimal? Max
        {
            get => _max;
            set => SetProperty(ref _max, value);
        }

        public int? StepCount
        {
            get => _stepCount;
            set => SetProperty(ref _stepCount, value);
        }
    }
}
