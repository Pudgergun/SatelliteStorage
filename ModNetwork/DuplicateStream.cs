using System;
using System.IO;

namespace SatelliteStorage.ModNetwork
{
    public class DuplicateStream : Stream
    {
        private readonly Stream _fromStream;
        private readonly Stream _toStream;

        public DuplicateStream(Stream underlyingStream, Stream copyStream)
        {
            _fromStream = underlyingStream;
            _toStream = copyStream;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }

        public override void Flush()
        {
            _fromStream.Flush();
        }

        public override long Length { get { return _fromStream.Length; } }
        public override long Position { get { return _fromStream.Position; } set { } }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _fromStream.Read(buffer, offset, count);

            _toStream.Write(buffer, offset, bytesRead);

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
    }
}
