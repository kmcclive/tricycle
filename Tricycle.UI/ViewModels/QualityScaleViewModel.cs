using System;
using System.Runtime.CompilerServices;

namespace Tricycle.UI.ViewModels
{
    public class QualityScaleViewModel : ViewModelBase
    {
        string _min;
        string _max;
        string _stepCount;

        public string Min
        {
            get => _min;
            set => SetProperty(ref _min, value);
        }

        public string Max
        {
            get => _max;
            set => SetProperty(ref _max, value);
        }

        public string StepCount
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
