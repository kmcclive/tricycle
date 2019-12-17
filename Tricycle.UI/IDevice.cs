using System;

namespace Tricycle.UI
{
    public interface IDevice
    {
        string RuntimePlatform { get; }

        void BeginInvokeOnMainThread(Action action);
        void StartTimer(TimeSpan interval, Func<bool> callback);
    }
}
