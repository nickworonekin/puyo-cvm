using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    /// <summary>
    /// The exception that is thrown when a directory listing is not found in the game's executable.
    /// </summary>
    internal class DirectoryListNotFoundException : Exception
    {
        public DirectoryListNotFoundException()
        {
        }

        public DirectoryListNotFoundException(string? message) : base(message)
        {
        }

        public DirectoryListNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DirectoryListNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
