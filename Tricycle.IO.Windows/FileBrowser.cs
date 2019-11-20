using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Tricycle.IO.Models;

namespace Tricycle.IO.Windows
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
            var result = new FileBrowserResult();
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = defaultDirectory
            };

            if (extensions?.Any() == true)
            {
                var filterBuilder = new StringBuilder();

                for (int i = 0; i < extensions.Count; i++)
                {
                    if (i > 0)
                    {
                        filterBuilder.Append("|");
                    }

                    string ext = extensions[i].Replace(".", string.Empty);

                    filterBuilder.Append($"{ext.ToUpper()} Files (*.{ext})|*.{ext}");
                }

                dialog.Filter = filterBuilder.ToString();
            }

            if (dialog.ShowDialog() == true)
            {
                result.Confirmed = true;
                result.FileName = dialog.FileName;
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
            var result = new FileBrowserResult();
            var dialog = new SaveFileDialog()
            {
                InitialDirectory = defaultDirectory,
                FileName = defaultFileName
            };

            if (dialog.ShowDialog() == true)
            {
                result.Confirmed = true;
                result.FileName = dialog.FileName;
            }

            return Task.FromResult(result);
        }
    }
}
