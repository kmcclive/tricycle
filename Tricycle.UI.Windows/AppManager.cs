using System.Windows;

namespace Tricycle.UI.Windows
{
    class AppManager : AppManagerBase
    {
        Window _mainWindow;

        public AppManager(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public override void Alert(string title, string message, Severity severity)
        {
            MessageWindow.Show(_mainWindow, title, message, severity, MessageWindowButtons.Ok);
        }

        public override string Ask(string title, string message, string defaultValue)
        {
            return InputWindow.Show(_mainWindow, title, message, defaultValue, true);
        }

        public override bool Confirm(string title, string message)
        {
            return MessageWindow.Show(_mainWindow, title, message, Severity.Warning, MessageWindowButtons.OkCancel) == true;
        }
    }
}
