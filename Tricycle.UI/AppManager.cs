using System;
using Tricycle.UI.Models;

namespace Tricycle.UI
{
    public class AppManager : IAppManager
    {
        public event Action Ready;
        public event Action Busy;
        public event Action<string> FileOpened;
        public event Action<CancellationArgs> Quitting;

        public void RaiseReady() => Ready?.Invoke();
        public void RaiseBusy() => Busy?.Invoke();
        public void RaiseFileOpened(string fileName) => FileOpened?.Invoke(fileName);
        public void RaiseQuitting(CancellationArgs args) => Quitting?.Invoke(args);
    }
}
