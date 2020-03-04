using System.Threading.Tasks;
using Tricycle.IO.Models;

namespace Tricycle.IO
{
    public interface IFolderBrowser
    {
        Task<FolderBrowserResult> Browse();
        Task<FolderBrowserResult> Browse(string defaultDirectory);
    }
}
