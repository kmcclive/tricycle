using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricycle.IO.Models;
using Windows.Storage.Pickers;

namespace Tricycle.IO.UWP
{
    public class FolderBrowser : IFolderBrowser
    {
        public Task<FolderBrowserResult> Browse()
        {
            return Browse(null);
        }

        public async Task<FolderBrowserResult> Browse(string defaultDirectory)
        {
            var folderPicker = new FolderPicker();
            var startlocation = StorageUtility.GetPickerLocationId(defaultDirectory);

            if (startlocation.HasValue)
            {
                folderPicker.SuggestedStartLocation = startlocation.Value;
            }

            var folder = await folderPicker.PickSingleFolderAsync();
            var result = new FolderBrowserResult();

            if (folder != null)
            {
                result.Confirmed = true;
                result.FolderName = folder.Path;
            }

            return result;
        }
    }
}
