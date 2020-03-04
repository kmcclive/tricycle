using System.Threading.Tasks;
using AppKit;
using Tricycle.IO.Models;

namespace Tricycle.IO.macOS
{
    public class FolderBrowser : IFolderBrowser
    {
        public Task<FolderBrowserResult> Browse()
        {
            return Browse(null);
        }

        public Task<FolderBrowserResult> Browse(string defaultDirectory)
        {
            var openPanel = NSOpenPanel.OpenPanel;
            var result = new FolderBrowserResult();

            openPanel.CanChooseDirectories = true;
            openPanel.CanChooseFiles = false;
            openPanel.CanCreateDirectories = true;

            if (openPanel.RunModal(defaultDirectory, null, null) == 1)
            {
                result.Confirmed = true;
                result.FolderName = openPanel.Directory;
            }

            return Task.FromResult(result);
        }
    }
}
