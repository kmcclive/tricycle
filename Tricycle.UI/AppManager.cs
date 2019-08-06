using System;
using Tricycle.UI.Models;

namespace Tricycle.UI
{
    public class AppManager : IAppManager
    {
        public event Action<string> FileOpened;
        public event Action<CancellationArgs> Quitting;

        public void RaiseFileOpened(string fileName) => FileOpened?.Invoke(fileName);
        public void RaiseQuitting(CancellationArgs args) => Quitting?.Invoke(args);
    }
}
