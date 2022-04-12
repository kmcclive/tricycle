using System;
using System.IO;
using System.IO.Abstractions;
using Tricycle.Utilities;
using FileIO = Windows.Storage.FileIO;
using StorageFile = Windows.Storage.StorageFile;
using StorageFolder = Windows.Storage.StorageFolder;

namespace Tricycle.IO.UWP
{
    public class File : FileWrapper
    {
        public File(IFileSystem fileSystem)
            : base(fileSystem)
        {

        }

        public override void WriteAllText(string path, string contents)
        {
            var folder = GetFolder(path);
            StorageFile file;
            string fileName;

            if (!TryGetFile(folder, path, out file, out fileName))
            {
                file = folder.CreateFileAsync(fileName).AsTask().RunSync();
            }

            FileIO.WriteTextAsync(file, contents).AsTask().RunSync();
        }

        StorageFolder GetFolder(string path)
        {
            string dir = Path.GetDirectoryName(path);

            return StorageFolder.GetFolderFromPathAsync(dir).AsTask().RunSync();
        }

        bool TryGetFile(StorageFolder folder, string path, out StorageFile file, out string fileName)
        {
            fileName = Path.GetFileName(path);
            file = folder.TryGetItemAsync(fileName).AsTask().RunSync() as StorageFile;

            return file != null;
        }
    }
}
