using DiscUtils;
using DiscUtils.Iso9660;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class DirectoryListDirectoryEntry : DirectoryListEntry
    {
        public override string Name { get; }

        public override string FullName => Parent is null
            ? string.Empty
            : Parent?.FullName + Name + '\\';

        public override DirectoryListDirectoryEntry? Parent { get; }

        /// <summary>
        /// Gets the entries in the directory.
        /// </summary>
        public List<DirectoryListEntry> Entries { get; } = new();

        public DirectoryListDirectoryEntry(string name, DirectoryListDirectoryEntry? parent)
        {
            Name = name;
            Parent = parent;
        }

        public void Read(BinaryReader reader, DirectoryListInfo directoryListInfo)
        {
            List<DirectoryListDirectoryEntry> directories = new();

            // Get the number of entries in the directory.
            // The number of entries includes the "." and ".." directory entries.
            int entryCount = reader.ReadInt32(directoryListInfo.Endianness);
            int entryCountDuplicate = reader.ReadInt32(directoryListInfo.Endianness);

            // Verify the two entry count values match.
            if (entryCount != entryCountDuplicate)
            {
                throw new DirectoryListInvalidException("The entry counts for this directory listing do not match.");
            }

            reader.BaseStream.Position += 4; // Sector of directory entry in the CVM. Not needed for reading.

            // Read the directory entry magic code & verify it's equal to "#DirLst#".
            ReadOnlySpan<byte> magicCode = reader.ReadBytes(DirectoryListConstants.DirLstMagicCode.Length);
            if (!magicCode.SequenceEqual(DirectoryListConstants.DirLstMagicCode))
            {
                Console.WriteLine(reader.BaseStream.Position);
                throw new DirectoryListInvalidException("The magic code for this directory listing is invalid.");
            }

            for (int i = 0; i < entryCount; i++)
            {
                reader.BaseStream.Position += 12; // 4 bytes - Length of the directory entry's sector in CVM. Not needed for reading.
                                                  // 4 bytes - Appears to be padding (always null).
                                                  // 4 bytes - Sector of directory/file entry in the CVM. Not needed for reading.

                DirectoryListEntryAttributes attributes = (DirectoryListEntryAttributes)reader.ReadByte();
                reader.BaseStream.Position++; // Unknown. Not needed for reading.

                string name = reader.ReadString(34);

                // This entry represents a directory.
                if (attributes.HasFlag(DirectoryListEntryAttributes.Directory))
                {
                    // Reference to the current directory.
                    if (name == ".")
                    {
                        Entries.Add(this);
                    }

                    // Reference to the parent directory.
                    else if (name == "..")
                    {
                        Entries.Add(Parent ?? this);
                    }

                    // A sub-directory.
                    else
                    {
                        DirectoryListDirectoryEntry directoryEntry = new(name, this);
                        Entries.Add(directoryEntry);

                        directories.Add(directoryEntry);
                    }
                }

                // This entry represents a file.
                else
                {
                    DirectoryListFileEntry fileEntry = new(name, this);
                    Entries.Add(fileEntry);
                }
            }

            // If there's any padding, skip over it.
            if (directoryListInfo.Padding != 0)
            {
                reader.BaseStream.Position += directoryListInfo.Padding;
            }

            // Read the entries in the sub-directories listed in this directory.
            foreach (DirectoryListDirectoryEntry directory in directories)
            {
                directory.Read(reader, directoryListInfo);
            }
        }

        public void Write(BinaryWriter writer, DirectoryListInfo directoryListInfo, IClusterBasedFileSystem fileSystem)
        {
            writer.BaseStream.Position += 8; // Skip the number of entries in the directory. No need to change.

            // Get and write the cluster for this directory.
            (uint cluster, _) = ((uint, long))fileSystem.GetDirectoryClusterAndLength(FullName);

            writer.WriteUInt32(cluster, directoryListInfo.Endianness);

            writer.BaseStream.Position += DirectoryListConstants.DirLstMagicCode.Length; // Skip the directory list entry magic code. No need to change.

            foreach (DirectoryListEntry entry in Entries)
            {
                // Get the cluster and length for this entry.
                uint entryCluster;
                uint entryLength;

                if (entry is DirectoryListDirectoryEntry)
                {
                    (entryCluster, entryLength) = ((uint, uint))fileSystem.GetDirectoryClusterAndLength(entry.FullName);
                }
                else
                {
                    (entryCluster, entryLength) = ((uint, uint))fileSystem.GetFileClusterAndLength(entry.FullName);
                }

                writer.WriteUInt32(entryLength, directoryListInfo.Endianness);
                writer.BaseStream.Position += 4; // Padding. No need to change.
                writer.WriteUInt32(entryCluster, directoryListInfo.Endianness);

                writer.BaseStream.Position += 36; // 1 byte - Entry attributes. No need to change.
                                                  // 1 byte - Unknown. No need to change.
                                                  // 34 bytes - Entry name. No need to change.
            }

            // If there's any padding, skip over it.
            if (directoryListInfo.Padding != 0)
            {
                writer.BaseStream.Position += directoryListInfo.Padding;
            }

            // Write the entries in the sub-directories listed in this directory.
            IEnumerable<DirectoryListDirectoryEntry> directories = Entries
                .OfType<DirectoryListDirectoryEntry>()
                .Where(x => x.Parent == this);
            foreach (DirectoryListDirectoryEntry directory in directories)
            {
                directory.Write(writer, directoryListInfo, fileSystem);
            }
        }

        /// <summary>
        /// Returns an enumerable collection of all the entries in this directory and any sub-directories.
        /// </summary>
        /// <remarks>The enumerable collection does not include the "." and ".." directories.</remarks>
        /// <returns></returns>
        public IEnumerable<DirectoryListEntry> EnumerateAllEntries()
        {
            IEnumerable<DirectoryListEntry> entries = Entries.Where(x => x.Parent == this);
            IEnumerable<DirectoryListDirectoryEntry> directories = entries.OfType<DirectoryListDirectoryEntry>();

            foreach (DirectoryListEntry entry in entries)
            {
                yield return entry;
            }

            foreach (DirectoryListDirectoryEntry directory in directories)
            {
                foreach (DirectoryListEntry directoryEntry in directory.EnumerateAllEntries())
                {
                    yield return directoryEntry;
                }
            }
        }
    }
}
