using System;
using System.Threading;

namespace Tricycle.UI
{
    public class MockDevice : IDevice
    {
        static MockDevice _instance;

        public static MockDevice Self
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MockDevice();
                }

                return _instance;
            }
        }

        public string RuntimePlatform => "macOS";

        private MockDevice()
        {

        }

        public void BeginInvokeOnMainThread(Action action)
        {
            action?.Invoke();
        }

        public void StartTimer(TimeSpan interval, Func<bool> callback)
        {
            do
            {
                Thread.Sleep(interval);
            } while (callback());
        }
    }
}
