using System;
using System.Windows;
using Windows.UI.Popups;

namespace Tricycle.UI.UWP
{
    class AppManager : AppManagerBase
    {
        enum CommandId
        {
            OK,
            Cancel
        }

        public AppManager()
        {

        }

        public override void Alert(string title, string message, Severity severity)
        {
            var dialog = new MessageDialog(message, title);

            dialog.ShowAsync().AsTask().RunSynchronously();
        }

        public override string Ask(string title, string message, string defaultValue)
        {
            return null;
        }

        public override bool Confirm(string title, string message)
        {
            var dialog = new MessageDialog(message, title);

            dialog.Commands.Add(new UICommand() { Id = CommandId.OK, Label = "OK" });
            dialog.Commands.Add(new UICommand() { Id = CommandId.Cancel, Label = "Cancel" });

            var task = dialog.ShowAsync().AsTask();

            task.RunSynchronously();

            return Equals(task.Result.Id, CommandId.OK);
        }
    }
}
