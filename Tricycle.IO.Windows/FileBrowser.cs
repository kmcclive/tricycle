using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tricycle.IO.Models;

namespace Tricycle.IO.Windows
{
    public class FileBrowser : IFileBrowser
    {
        public Task<FileBrowserResult> BrowseToOpen()
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToOpen(string defaultDirectory)
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToOpen(string defaultDirectory, IList<string> extensions)
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToSave()
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory)
        {
            throw new NotImplementedException();
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory, string defaultFileName)
        {
            throw new NotImplementedException();
        }
    }
}
