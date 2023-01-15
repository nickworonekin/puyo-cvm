using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class DirectoryListReader
    {
        /// <summary>
        /// Gets the root directory.
        /// </summary>
        public DirectoryListDirectoryEntry Root { get; }

        public DirectoryListReader(Stream stream, DirectoryListInfo directoryListInfo)
        {
            using BinaryReader reader = new(stream, Encoding.UTF8, true);

            Root = new(string.Empty, null);
            Root.Read(reader, directoryListInfo);
        }
    }
}
