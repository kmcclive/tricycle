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
        public event Action Quitting;
        public event Action QuitConfirmed;
        public event Action<Page> ModalOpened;

        public void RaiseReady() => Ready?.Invoke();
        public void RaiseBusy() => Busy?.Invoke();
        public void RaiseFileOpened(string fileName) => FileOpened?.Invoke(fileName);
        public void RaiseQuitting() => Quitting?.Invoke();
        public void RaiseQuitConfirmed() => QuitConfirmed?.Invoke();
        public void RaiseModalOpened(Page page) => ModalOpened?.Invoke(page);
    }
}
