using DiscUtils;
using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal static class IClusterBasedFileSystemExtensions
    {
        /// <summary>
        /// Gets the cluster and length of the directory table for a directory.
        /// </summary>
        /// <remarks>The length of the directory table will be a multiple of <see cref="IClusterBasedFileSystem.ClusterSize"/>.</remarks>
        /// <param name="fileSystem"></param>
        /// <param name="path">The directory path.</param>
        /// <returns>The cluster and length of the directory table.</returns>
        public static (long cluster, long length) GetDirectoryClusterAndLength(this IClusterBasedFileSystem fileSystem, string path)
        {
            Range<long, long>[] clusters = fileSystem.PathToClusters(path);

            long offset = clusters[0].Offset;
            long length = clusters[0].Count * fileSystem.ClusterSize;

            return (offset, length);
        }

        /// <summary>
        /// Gets the cluster and length of a file.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="path">The file path.</param>
        /// <returns>The cluster and length.</returns>
        public static (long cluster, long length) GetFileClusterAndLength(this IClusterBasedFileSystem fileSystem, string path)
        {
            Range<long, long>[] clusters = fileSystem.PathToClusters(path);

            long offset = clusters[0].Offset;
            long length = fileSystem.GetFileLength(path);

            return (offset, length);
        }
    }
}
