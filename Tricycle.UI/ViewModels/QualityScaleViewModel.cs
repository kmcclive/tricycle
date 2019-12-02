using System;
using System.Runtime.CompilerServices;

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

        public event Action Modified;

        protected override void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            base.SetProperty(ref field, value, propertyName);

            Modified?.Invoke();
        }

        public void ClearHandlers()
        {
            if (Modified != null)
            {
                foreach (Action handler in Modified.GetInvocationList())
                {
                    Modified -= handler;
                }
            }
        }
    }
}
