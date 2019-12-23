using System;

namespace Tricycle.UI
{
    public interface IDevice
    {
        void BeginInvokeOnMainThread(Action action);
        void StartTimer(TimeSpan interval, Func<bool> callback);
    }
}
