using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Tricycle.IO.Models;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Tricycle.IO.UWP
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

        public async Task<FileBrowserResult> BrowseToOpen(string defaultDirectory, IList<string> extensions)
        {
            var openPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = StorageUtility.GetPickerLocationId(defaultDirectory),
            };

            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    openPicker.FileTypeFilter.Add(extension);
                }
            }

            var file = await openPicker.PickSingleFileAsync();
            var result = new FileBrowserResult();
            
            if (file != null)
            {
                result.Confirmed = true;
                result.FileName = file.Path;
            }

            return result;
        }

        public Task<FileBrowserResult> BrowseToSave()
        {
            return BrowseToSave(null);
        }

        public Task<FileBrowserResult> BrowseToSave(string defaultDirectory)
        {
            return BrowseToSave(defaultDirectory, null);
        }

        public async Task<FileBrowserResult> BrowseToSave(string defaultDirectory, string defaultFileName)
        {
            var savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = StorageUtility.GetPickerLocationId(defaultDirectory),
                SuggestedFileName = defaultFileName,
            };

            var file = await savePicker.PickSaveFileAsync();
            var result = new FileBrowserResult();

            if (file != null)
            {
                result.Confirmed = true;
                result.FileName = file.Path;
            }

            return result;
        }
    }
}
