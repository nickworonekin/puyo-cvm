using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal class CvmReader
    {
        private readonly Stream _source;

        public CvmReader(Stream source)
        {
            _source = source;
        }

        public Stream OpenIso()
        {
            // We can do better than this determining the position the ISO starts at.
            // For now, since we're writing the CVM to begin with, and the header is always 0x1800 bytes long,
            // just assume that to be the case.
            return new SubStream(_source, 0x1800, _source.Length - 0x1800);
        }
    }
}
