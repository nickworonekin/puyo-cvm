using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal static class CvmConstants
    {
        public static ReadOnlySpan<byte> MagicCode => "CVMH"u8;

        public static ReadOnlySpan<byte> RofsMagicCode => "ROFS"u8;

        public static ReadOnlySpan<byte> ZoneMagicCode => "ZONE"u8;

        public static ReadOnlySpan<byte> Application => "ROFSBLD Ver.1.52 2003-06-09"u8;
    }
}
