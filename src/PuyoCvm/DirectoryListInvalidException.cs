using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    /// <summary>
    /// The exception that is thrown when a directory listing in the game's executable is invalid.
    /// </summary>
    internal class DirectoryListInvalidException : Exception
    {
        public DirectoryListInvalidException()
        {
        }

        public DirectoryListInvalidException(string? message) : base(message)
        {
        }

        public DirectoryListInvalidException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DirectoryListInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
