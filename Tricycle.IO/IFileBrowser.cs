using System.Collections.Generic;
using System.Threading.Tasks;
using Tricycle.IO.Models;

namespace Tricycle.IO
{
    public interface IFileBrowser
    {
        Task<FileBrowserResult> BrowseToOpen();
        Task<FileBrowserResult> BrowseToOpen(string defaultDirectory);
        Task<FileBrowserResult> BrowseToOpen(string defaultDirectory, IList<string> extensions);
        Task<FileBrowserResult> BrowseToSave();
        Task<FileBrowserResult> BrowseToSave(string defaultDirectory);
        Task<FileBrowserResult> BrowseToSave(string defaultDirectory, IList<string> extensions);
    }
}
