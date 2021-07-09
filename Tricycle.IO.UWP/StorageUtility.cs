using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Tricycle.IO.UWP
{
    public static class StorageUtility
    {
        public static PickerLocationId? GetPickerLocationId(string directory)
        {
            PickerLocationId? result = null;

            if (string.IsNullOrWhiteSpace(directory))
            {
                return result;
            }

            string fullPath = null;

            try
            {
                fullPath = Path.GetFullPath(directory);
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }
            catch (SecurityException) { }

            if (fullPath == KnownFolders.DocumentsLibrary.Path)
            {
                result = PickerLocationId.DocumentsLibrary;
            }
            else if (fullPath == KnownFolders.HomeGroup.Path)
            {
                result = PickerLocationId.HomeGroup;
            }
            else if (fullPath == KnownFolders.MusicLibrary.Path)
            {
                result = PickerLocationId.MusicLibrary;
            }
            else if (fullPath == KnownFolders.Objects3D.Path)
            {
                result = PickerLocationId.Objects3D;
            }
            else if (fullPath == KnownFolders.PicturesLibrary.Path)
            {
                result = PickerLocationId.PicturesLibrary;
            }
            else if (fullPath == KnownFolders.VideosLibrary.Path)
            {
                result = PickerLocationId.VideosLibrary;
            }

            return result;
        }
    }
}
