using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class DirectoryListFileEntry : DirectoryListEntry
    {
        public override string Name { get; }

        public override string FullName => Parent.FullName + Name;

        public override DirectoryListDirectoryEntry Parent { get; }

        public DirectoryListFileEntry(string name, DirectoryListDirectoryEntry parent)
        {
            Name = name;
            Parent = parent;
        }
    }
}
