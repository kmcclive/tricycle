using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Tricycle.IO.Models;

namespace Tricycle.IO.macOS
{
    public class FileBrowser : IFileBrowser
    {
        public Task<FileBrowserResult> BrowseToOpen()
        {
            return BrowseToOpen(null, null);
        }

        public Task<FileBrowserResult> BrowseToOpen(string defaultDirectory)
        {
            return BrowseToOpen(defaultDirectory, null);
        }

        public Task<FileBrowserResult> BrowseToOpen(string defaultDirectory, IList<string> extensions)
        {
            var openPanel = new NSOpenPanel()
            {
                CanChooseDirectories = false,
                CanChooseFiles = true,
                CanCreateDirectories = false
            };
            var result = new FileBrowserResult();

            if (openPanel.RunModal(defaultDirectory, null, extensions?.ToArray()) == 1)
            {
                result.Confirmed = true;
                result.FileName = openPanel.Filename;
            }

            return Task.FromResult(result);
        }

        public Task<FileBrowserResult> BrowseToSave()
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory)
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory, IList<string> extensions)
        {
            throw new NotImplementedException();
        }
    }
}
