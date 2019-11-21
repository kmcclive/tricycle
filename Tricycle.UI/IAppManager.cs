using System;
using Tricycle.UI.Models;
using Xamarin.Forms;

namespace Tricycle.UI
{
    public interface IAppManager
    {
        event Action Ready;
        event Action Busy;
        event Action<string> FileOpened;
        event Action<CancellationArgs> Quitting;
        event Action<Page> ModalOpened;

        void RaiseReady();
        void RaiseBusy();
        void RaiseFileOpened(string fileName);
        void RaiseQuitting(CancellationArgs args);
        void RaiseModalOpened(Page page);
    }
}
