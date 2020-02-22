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
            var window = new InputWindow()
            {
                Owner = _mainWindow,
                Title = "Save Template",
                Message = "Please enter a name for the template:",
                Value = defaultValue,
                IsValueRequired = true
            };

            return window.ShowDialog() == true ? window.Value : null;
        }

        public override bool Confirm(string title, string message)
        {
            return MessageWindow.Show(_mainWindow, title, message, Severity.Warning, MessageWindowButtons.OkCancel) == true;
        }
    }
}
