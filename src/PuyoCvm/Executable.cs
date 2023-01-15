using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal static class Executable
    {
        private static readonly List<DirectoryListInfo> directoryListInfos = new()
        {
            new("Puyo Puyo! 15th Anniversary (PlayStation 2)", 0x1E5C40, Endianness.Little, 8),
            new("Puyo Puyo! 15th Anniversary (PlayStation Portable)", 0xEE5EC, Endianness.Little, 0),
            new("Puyo Puyo! 15th Anniversary (Wii, v1.0)", 0x25E1D0, Endianness.Big, 0),
            new("Puyo Puyo! 15th Anniversary (Wii, v1.1)", 0x25E350, Endianness.Big, 0),
            new("Puyo Puyo 7 (PlayStation Portable)", 0xE6FF0, Endianness.Little, 0),
            new("Puyo Puyo 7 (Wii)", 0x2B2D20, Endianness.Big, 0),
        };

        public static DirectoryListInfo? GetDirectoryListInfo(Stream source)
        {
            long startPosition = source.Position;

            using BinaryReader reader = new(source, Encoding.UTF8, true);

            foreach (DirectoryListInfo directoryListInfo in directoryListInfos)
            {
                // Verify enough bytes can be read.
                if (startPosition + directoryListInfo.Position + 0x10 > source.Length)
                {
                    continue;
                }

                // Verify the entry count + duplicate entry count matches.
                int entryCount = reader.At(startPosition + directoryListInfo.Position, x => x.ReadInt32(directoryListInfo.Endianness));
                int entryCountDuplicate = reader.At(startPosition + directoryListInfo.Position + 0x4, x => x.ReadInt32(directoryListInfo.Endianness));

                if (entryCount != entryCountDuplicate)
                {
                    continue;
                }

                // Read the directory entry magic code & verify it's equal to "#DirLst#".
                ReadOnlySpan<byte> magicCode = reader.At(startPosition + directoryListInfo.Position + 0xC, x => x.ReadBytes(DirectoryListConstants.DirLstMagicCode.Length));
                if (!magicCode.SequenceEqual(DirectoryListConstants.DirLstMagicCode))
                {
                    continue;
                }

                return directoryListInfo;
            }

            return null;
        }
    }
}
