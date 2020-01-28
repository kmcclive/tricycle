using System;
using Xamarin.Forms;

namespace Tricycle.UI
{
    public enum Modal
    {
        Config,
        Preview
    }

    public interface IAppManager
    {
        bool IsBusy { get; }
        bool IsQuitConfirmed { get; }
        bool IsModalOpen { get; }
        bool IsValidSourceSelected { get; }

        event Action Ready;
        event Action Busy;
        event Action<string> FileOpened;
        event Action Quitting;
        event Action QuitConfirmed;
        event Action<Modal> ModalOpened;
        event Action ModalClosed;
        event Action<bool> SourceSelected;
        event Action<string> TemplateSaved;

        void RaiseReady();
        void RaiseBusy();
        void RaiseFileOpened(string fileName);
        void RaiseQuitting();
        void RaiseQuitConfirmed();
        void RaiseModalOpened(Modal modal);
        void RaiseModalClosed();
        void RaiseSourceSelected(bool isValid);
        void RaiseTemplateSaved(string name);
    }
}
