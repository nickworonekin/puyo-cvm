using DiscUtils.Iso9660;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class CvmWriter
    {
        // This CVM writer is only guaranteed to produce a CVM compatible with
        // Puyo Puyo! 15th Anniversary (PS2, PSP, & Wii) and Puyo Puyo 7 (PSP & Wii).
        // This writer assumes values that don't change between the different source ROFS
        // are constant values, which may not be the case for CVMs in other games.

        private readonly DirectoryListDirectoryEntry? _rootDirectory;
        private readonly string _sourcePath;

        public CvmWriter(DirectoryListDirectoryEntry? rootDirectory, string sourcePath)
        {
            _rootDirectory = rootDirectory;
            _sourcePath = sourcePath;
        }

        public void Write(Stream destination)
        {
            long startPosition = destination.Position;

            using BinaryWriter writer = new(destination, Encoding.UTF8, true);

            // Start with writing the CVM header (0x1800 bytes long).
            writer.Write(CvmConstants.MagicCode); // Magic Code (always "CVMH")
            writer.WriteInt64BigEndian(0x800 - 12); // Block length without header & this value (block length minus 12)
            writer.Write(new byte[16]); // Padding (16 bytes)
            writer.WriteInt64BigEndian(0); // Total length (to be filled in later)

            // Write the date information.
            DateTimeOffset now = DateTimeOffset.Now;
            writer.WriteByte((byte)(now.Year - 1900)); // Year (minus 1900)
            writer.WriteByte((byte)now.Month); // Month
            writer.WriteByte((byte)now.Day); // Day
            writer.WriteByte((byte)now.Hour); // Hour
            writer.WriteByte((byte)now.Minute); // Minute
            writer.WriteByte((byte)now.Second); // Second
            writer.WriteByte((byte)Math.Floor(now.Offset.TotalMinutes / 15.0)); // UTC offset in 15 minute increments.
            writer.WriteByte(0); // Padding (1 byte)

            writer.WriteUInt64BigEndian(0x0101000000000000u); // Assumed constant

            // Write the ROFS section.
            writer.Write(CvmConstants.RofsMagicCode); // Always "ROFS"
            writer.Write(CvmConstants.Application); // Application
            writer.Write(new byte[64 - CvmConstants.Application.Length]); // Padding (Application takes up 64 bytes)

            writer.WriteUInt32BigEndian(0x011F0000u); // Assumed constant
            writer.WriteUInt32BigEndian(0x03000000u); // Assumed constant
            writer.WriteUInt32BigEndian(0x00000001u); // Assumed constant
            writer.WriteUInt64BigEndian(0x0000000000000003u); // Assumed constant
            writer.Write(new byte[116]); // Padding (116 bytes)
            writer.WriteUInt32BigEndian(0x00000001u); // Assumed constant
            writer.Align(2048, startPosition); // Align to the next sector.

            // Write the ZONE section.
            writer.Write(CvmConstants.ZoneMagicCode); // Always "ZONE"
            writer.WriteInt64BigEndian(0); // Total length minus 2060 bytes (to be filled in later)
            writer.WriteUInt32BigEndian(0x00000003u); // Assumed constant
            writer.WriteUInt32BigEndian(0x00000410u); // Assumed constant
            writer.WriteUInt64BigEndian(0x0000000000000800u); // Assumed constant
            writer.WriteUInt32BigEndian(0x00000800u); // Assumed constant
            writer.WriteUInt32BigEndian(0x00000002u); // Assumed constant
            writer.WriteUInt64BigEndian(0x0000000000000800u); // Assumed constant
            writer.WriteUInt32BigEndian(0x00000003u); // Assumed constant
            writer.WriteInt64BigEndian(0); // Length of the ISO (total length minus 6144 bytes) (to be filled in later)
            writer.Align(2048, startPosition); // Align to the next sector.

            writer.WriteUInt64BigEndian(0x4100000200000000u); // Assumed constant
            writer.WriteUInt64BigEndian(0x4101000200000000u); // Assumed constant
            writer.WriteUInt64BigEndian(0x4101010200000200u); // Assumed constant
            writer.WriteUInt32BigEndian(0x41AA0102u); // Assumed constant
            writer.WriteUInt32BigEndian(0u); // Unknown. May be based on file size, but works ok with a value of 0.
            writer.Align(2048, startPosition); // Align to the next sector.

            WriteIso(destination);

            // Fill in values previously left blank.
            writer.At(startPosition + 0x1C, x => x.WriteInt64BigEndian(x.BaseStream.Length - startPosition));
            writer.At(startPosition + 0x804, x => x.WriteInt64BigEndian(x.BaseStream.Length - startPosition - 2060));
            writer.At(startPosition + 0x830, x => x.WriteInt64BigEndian(x.BaseStream.Length - startPosition - 6144));
        }
        
        private void WriteIso(Stream destination)
        {
            CDBuilder writer = new()
            {
                UseJoliet = false,
                SystemIdentifier = "CRI ROFS",
                VolumeIdentifier = "SAMPLE_GAME_TITLE",
                VolumeSetIdentifier = "SAMPLE_GAME_TITLE",
                PublisherIdentifier = "PUBLISHER_NAME",
                DataPreparerIdentifier = "PUBLISHER_NAME",
            };

            // If the root directory is not null, only add the files listed in the root directory's file enumeration.
            // Otherwise, add all the files from the source path.
            if (_rootDirectory is not null)
            {
                foreach (DirectoryListEntry entry in _rootDirectory.EnumerateAllEntries())
                {
                    if (entry is DirectoryListDirectoryEntry)
                    {
                        writer.AddDirectory(entry.FullName);
                    }
                    else
                    {
                        writer.AddFile(entry.FullName + ";1", Path.Combine(_sourcePath, entry.FullName));
                    }
                }
            }
            else
            {
                EnumerationOptions options = new()
                {
                    RecurseSubdirectories = true,
                };

                DirectoryInfo rootDirectory = new(_sourcePath);
                foreach (FileSystemInfo entry in rootDirectory.EnumerateFileSystemInfos("*", options))
                {
                    if (entry.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        writer.AddDirectory(entry.FullName.Substring(rootDirectory.FullName.Length + 1));
                    }
                    else
                    {
                        writer.AddFile(string.Concat(entry.FullName.AsSpan(rootDirectory.FullName.Length + 1), ";1"), entry.FullName);
                    }
                }
            }

            writer.Build(destination);
        }
    }
}
