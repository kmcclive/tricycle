using System;
using AppKit;
using Tricycle.IO.Models;

namespace Tricycle.IO.macOS
{
    public class FileBrowser : IFileBrowser
    {
        public FileBrowserResult BrowseToOpen()
        {
            return BrowseToOpen(null, null);
        }

        public FileBrowserResult BrowseToOpen(string defaultDirectory)
        {
            return BrowseToOpen(defaultDirectory, null);
        }

        public FileBrowserResult BrowseToOpen(string defaultDirectory, string[] extensions)
        {
            var openPanel = new NSOpenPanel()
            {
                CanChooseDirectories = false,
                CanChooseFiles = true,
                CanCreateDirectories = false
            };
            var result = new FileBrowserResult();

            if (openPanel.RunModal(defaultDirectory, null, extensions) == 1)
            {
                result.Confirmed = true;
                result.FileName = openPanel.Filename;
            }

            return result;
        }

        public FileBrowserResult BrowseToSave()
        {
            throw new NotImplementedException();
        }

        public FileBrowserResult BrowseToSave(string defaultDirectory)
        {
            throw new NotImplementedException();
        }

        public FileBrowserResult BrowseToSave(string defaultDirectory, string[] extensions)
        {
            throw new NotImplementedException();
        }
    }
}
