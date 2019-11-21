using System;
using Tricycle.UI.Models;
using Xamarin.Forms;

namespace Tricycle.UI
{
    public class AppManager : IAppManager
    {
        public event Action Ready;
        public event Action Busy;
        public event Action<string> FileOpened;
        public event Action<CancellationArgs> Quitting;
        public event Action<ContentPage> ModalOpened;

        public void RaiseReady() => Ready?.Invoke();
        public void RaiseBusy() => Busy?.Invoke();
        public void RaiseFileOpened(string fileName) => FileOpened?.Invoke(fileName);
        public void RaiseQuitting(CancellationArgs args) => Quitting?.Invoke(args);
        public void RaiseModalOpened(ContentPage page) => ModalOpened?.Invoke(page);
    }
}
