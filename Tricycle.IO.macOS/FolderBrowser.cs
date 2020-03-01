using System;
using System.Threading.Tasks;
using AppKit;
using Tricycle.IO.Models;

namespace Tricycle.IO.macOS
{
    public class FolderBrowser : IFolderBrowser
    {
        public Task<FolderBrowserResult> BrowseToOpen()
        {
            return BrowseToOpen(null);
        }

        public Task<FolderBrowserResult> BrowseToOpen(string defaultDirectory)
        {
            var openPanel = NSOpenPanel.OpenPanel;
            var result = new FolderBrowserResult();

            openPanel.CanChooseDirectories = true;
            openPanel.CanChooseFiles = false;
            openPanel.CanCreateDirectories = false;

            if (openPanel.RunModal(defaultDirectory, null, null) == 1)
            {
                result.Confirmed = true;
                result.FolderName = openPanel.Directory;
            }

            return Task.FromResult(result);
        }

        public Task<FolderBrowserResult> BrowseToSave()
        {
            return BrowseToSave(null);
        }

        public Task<FolderBrowserResult> BrowseToSave(string defaultDirectory)
        {
            return BrowseToOpen(defaultDirectory);
        }
    }
}
