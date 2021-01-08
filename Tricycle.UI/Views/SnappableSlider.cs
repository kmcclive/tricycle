using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public class SnappableSlider : Slider
    {
        static readonly TimeSpan DEBOUNCE_TIME = TimeSpan.FromMilliseconds(50);

        public static readonly BindableProperty StepCountProperty = BindableProperty.Create(
          nameof(StepCount),
          typeof(int),
          typeof(SnappableSlider));

        bool _timerRunning;
        DateTime _lastChange;

        public int StepCount
        {
            get { return (int)GetValue(StepCountProperty); }
            set { SetValue(StepCountProperty, value); }
        }

        public SnappableSlider()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.macOS:
                    HeightRequest = 24;
                    break;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(StepCount):
                    Snap();
                    break;
                case nameof(Value):
                    _lastChange = DateTime.Now;

                    if (!_timerRunning)
                    {
                        _timerRunning = true;

                        Device.StartTimer(DEBOUNCE_TIME, () =>
                        {
                            if (DateTime.Now < _lastChange + DEBOUNCE_TIME)
                            {
                                return true;
                            }

                            Snap();

                            _timerRunning = false;

                            return false;
                        });
                    }
                    break;
            }
        }

        void Snap()
        {
            double stepAmount = (Maximum - Minimum) / StepCount;

            Value = Math.Round(Value / stepAmount) * stepAmount + Minimum;
        }
    }
}
