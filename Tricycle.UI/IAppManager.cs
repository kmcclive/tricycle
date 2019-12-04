using System;
using Tricycle.UI.Models;
using Xamarin.Forms;

namespace Tricycle.UI
{
    public interface IAppManager
    {
        bool IsBusy { get; }
        bool IsQuitConfirmed { get; }
        bool IsModalOpen { get; }

        event Action Ready;
        event Action Busy;
        event Action<string> FileOpened;
        event Action Quitting;
        event Action QuitConfirmed;
        event Action<Page> ModalOpened;
        event Action ModalClosed;

        void RaiseReady();
        void RaiseBusy();
        void RaiseFileOpened(string fileName);
        void RaiseQuitting();
        void RaiseQuitConfirmed();
        void RaiseModalOpened(Page page);
        void RaiseModalClosed();
    }
}
