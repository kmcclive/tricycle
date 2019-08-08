using System;
using Tricycle.UI.Models;

namespace Tricycle.UI
{
    public interface IAppManager
    {
        event Action Ready;
        event Action Busy;
        event Action<string> FileOpened;
        event Action<CancellationArgs> Quitting;

        void RaiseReady();
        void RaiseBusy();
        void RaiseFileOpened(string fileName);
        void RaiseQuitting(CancellationArgs args);
    }
}
