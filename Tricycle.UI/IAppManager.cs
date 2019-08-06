using System;
using Tricycle.UI.Models;

namespace Tricycle.UI
{
    public interface IAppManager
    {
        event Action<string> FileOpened;
        event Action<CancellationArgs> Quitting;

        void RaiseFileOpened(string fileName);
        void RaiseQuitting(CancellationArgs args);
    }
}
