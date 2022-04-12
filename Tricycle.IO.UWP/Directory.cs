using System;
using System.IO;
using System.IO.Abstractions;
using Tricycle.Utilities;
using StorageFolder = Windows.Storage.StorageFolder;

namespace Tricycle.IO.UWP
{
    public class Directory : DirectoryWrapper
    {
        public Directory(IFileSystem fileSystem)
            : base(fileSystem)
        {

        }

        public override bool Exists(string path)
        {
            try
            {
                GetFolder(path);
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        public override IDirectoryInfo CreateDirectory(string path)
        {
            string parent = Path.GetFullPath(Path.Combine(path, ".."));
            var folder = GetFolder(parent);
            string name = Path.GetFileName(path);

            folder.CreateFolderAsync(name).AsTask().RunSync();

            return new DirectoryInfoWrapper(FileSystem, new DirectoryInfo(path));
        }

        StorageFolder GetFolder(string path)
        {
            return StorageFolder.GetFolderFromPathAsync(path).AsTask().RunSync();
        }
    }
}
