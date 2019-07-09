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
            return BrowseToOpen(null);
        }

        public Task<FileBrowserResult> BrowseToOpen(string defaultDirectory)
        {
            return BrowseToOpen(defaultDirectory, null);
        }

        public Task<FileBrowserResult> BrowseToOpen(string defaultDirectory, IList<string> extensions)
        {
            var openPanel = NSOpenPanel.OpenPanel;
            var result = new FileBrowserResult();

            openPanel.CanChooseDirectories = false;
            openPanel.CanChooseFiles = true;
            openPanel.CanCreateDirectories = false;

            if (openPanel.RunModal(defaultDirectory, null, extensions?.ToArray()) == 1)
            {
                result.Confirmed = true;
                result.FileName = openPanel.Filename;
            }

            return Task.FromResult(result);
        }

        public Task<FileBrowserResult> BrowseToSave()
        {
            return BrowseToSave(null);
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory)
        {
            return BrowseToSave(defaultDirectory, null);
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory, string defaultFileName)
        {
            var savePanel = NSSavePanel.SavePanel;
            var result = new FileBrowserResult();

            savePanel.CanCreateDirectories = true;
            savePanel.CanSelectHiddenExtension = true;

            if (savePanel.RunModal(defaultDirectory, defaultFileName) == 1)
            {
                result.Confirmed = true;
                result.FileName = savePanel.Filename;
            }

            return Task.FromResult(result);
        }
    }
}
