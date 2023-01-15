using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class DirectoryListInfo
    {
        public DirectoryListInfo(string name, long position, Endianness endianness, long padding)
        {
            Name = name;
            Position = position;
            Endianness = endianness;
            Padding = padding;
        }

        /// <summary>
        /// Gets the name of the game the directory list belongs to.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the position of the directory list in the executable.
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Gets the endianness of the values in the directory list.
        /// </summary>
        public Endianness Endianness { get; }

        /// <summary>
        /// Gets the number of bytes of padding at the end of a directory's list.
        /// </summary>
        public long Padding { get; }
    }
}
