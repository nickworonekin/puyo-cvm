using DiscUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class DirectoryListWriter
    {
        private readonly IClusterBasedFileSystem _fileSystem;
        private readonly DirectoryListDirectoryEntry _rootDirectory;

        public DirectoryListWriter(IClusterBasedFileSystem fileSystem, DirectoryListDirectoryEntry rootDirectory)
        {
            _fileSystem = fileSystem;
            _rootDirectory = rootDirectory;
        }

        public void Write(Stream destination, DirectoryListInfo directoryListInfo)
        {
            using BinaryWriter writer = new(destination, Encoding.UTF8, true);

            _rootDirectory.Write(writer, directoryListInfo, _fileSystem);
        }
    }
}
