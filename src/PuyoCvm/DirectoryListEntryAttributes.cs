using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    [Flags]
    internal enum DirectoryListEntryAttributes : byte
    {
        None = 0,
        Directory = 0x2,
    }
}
