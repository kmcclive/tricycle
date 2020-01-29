using System;

namespace Tricycle.UI
{
    public class AppManager : IAppManager
    {
        int _modalCount;

        public bool IsBusy { get; private set; }
        public bool IsQuitConfirmed { get; private set; }
        public bool IsModalOpen { get => _modalCount > 0; }
        public bool IsValidSourceSelected { get; private set; }

        public event Action Ready;
        public event Action Busy;
        public event Action<string> FileOpened;
        public event Action Quitting;
        public event Action QuitConfirmed;
        public event Action<Modal> ModalOpened;
        public event Action ModalClosed;
        public event Action<bool> SourceSelected;
        public event Action<string> TemplateSaved;
        public event Action<string> TemplateApplied;

        public void RaiseFileOpened(string fileName) => FileOpened?.Invoke(fileName);
        public void RaiseQuitting() => Quitting?.Invoke();
        public void RaiseTemplateSaved(string name) => TemplateSaved?.Invoke(name);
        public void RaiseTemplateApplied(string name) => TemplateApplied?.Invoke(name);

        public void RaiseReady()
        {
            IsBusy = false;
            Ready?.Invoke();
        }

        public void RaiseBusy()
        {
            IsBusy = true;
            Busy?.Invoke();
        }

        public void RaiseQuitConfirmed()
        {
            IsQuitConfirmed = true;
            QuitConfirmed?.Invoke();
        }

        public void RaiseModalOpened(Modal modal)
        {
            _modalCount++;
            ModalOpened?.Invoke(modal);
        }

        public void RaiseModalClosed()
        {
            _modalCount--;
            ModalClosed?.Invoke();
        }

        public void RaiseSourceSelected(bool isValid)
        {
            IsValidSourceSelected = isValid;
            SourceSelected?.Invoke(isValid);
        }
    }
}
