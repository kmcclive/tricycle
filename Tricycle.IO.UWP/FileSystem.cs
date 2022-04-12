using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tricycle.IO.UWP
{
    public class FileSystem : IFileSystem
    {
        readonly IFileSystem _fileSystem;

        public IFile File { get; private set; }
        public IDirectory Directory { get; private set; }
        public IFileInfoFactory FileInfo => _fileSystem.FileInfo;
        public IFileStreamFactory FileStream => _fileSystem.FileStream;
        public IPath Path => _fileSystem.Path;
        public IDirectoryInfoFactory DirectoryInfo => _fileSystem.DirectoryInfo;
        public IDriveInfoFactory DriveInfo => _fileSystem.DriveInfo;
        public IFileSystemWatcherFactory FileSystemWatcher => _fileSystem.FileSystemWatcher;

        public FileSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            File = new File(this);
            Directory = new Directory(this);
        }
    }
}
