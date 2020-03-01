using System.Collections.Generic;
using System.Threading.Tasks;
using Tricycle.IO.Models;

namespace Tricycle.IO
{
    public interface IFolderBrowser
    {
        Task<FolderBrowserResult> BrowseToOpen();
        Task<FolderBrowserResult> BrowseToOpen(string defaultDirectory);
        Task<FolderBrowserResult> BrowseToSave();
        Task<FolderBrowserResult> BrowseToSave(string defaultDirectory);
    }
}
