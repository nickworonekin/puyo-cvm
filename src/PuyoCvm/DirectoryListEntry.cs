using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal abstract class DirectoryListEntry
    {
        /// <summary>
        /// Gets the name of the entry.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the full name of the entry.
        /// </summary>
        public abstract string FullName { get; }

        /// <summary>
        /// Gets the parent directory of the entry.
        /// </summary>
        public abstract DirectoryListDirectoryEntry? Parent { get; }
    }
}
