using System;
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

        public void BeginInvokeOnMainThread(Action action)
        {
            action?.Invoke();
        }

        private MockDevice()
        {

        }
    }
}
