using System;
using Xamarin.Forms;

namespace Tricycle.UI
{
    /// <summary>
    /// Wraps the <see cref="Device"/> class to implement <see cref="IDevice"/> interface.
    /// </summary>
    public class DeviceWrapper : IDevice
    {
        static DeviceWrapper _instance;

        public static DeviceWrapper Self
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeviceWrapper();
                }

                return _instance;
            }
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }

        private DeviceWrapper()
        {

        }
    }
}
