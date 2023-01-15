using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal static class DirectoryListConstants
    {
        public static ReadOnlySpan<byte> DirLstMagicCode => "#DirLst#\0\0\0\0"u8;
    }
}
