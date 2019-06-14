using System;
using System.Collections.Generic;
using Tricycle.IO.Models;

namespace Tricycle.IO
{
    public interface IFileBrowser
    {
        FileBrowserResult BrowseToOpen();
        FileBrowserResult BrowseToOpen(string defaultDirectory);
        FileBrowserResult BrowseToOpen(string defaultDirectory, string[] extensions);
        FileBrowserResult BrowseToSave();
        FileBrowserResult BrowseToSave(string defaultDirectory);
        FileBrowserResult BrowseToSave(string defaultDirectory, string[] extensions);
    }
}
