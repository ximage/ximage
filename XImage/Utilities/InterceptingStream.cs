using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage.Utilities
{
	public class InterceptingStream : Stream
	{
		MemoryStream _bufferedStream = new MemoryStream();
		Action<Stream> _onClose;

		public override bool CanRead
		{
			get { return _bufferedStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _bufferedStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _bufferedStream.CanWrite; }
		}

		public override long Length
		{
			get { return _bufferedStream.Length; }
		}

		public override long Position
		{
			get { return _bufferedStream.Position; }
			set { _bufferedStream.Position = value; }
		}

		public InterceptingStream(Action<Stream> onClose)
		{
			_onClose = onClose;
		}

		public override void Close()
		{
			_onClose(_bufferedStream);

			_bufferedStream.Close();
		}

		public override void Flush()
		{
			_bufferedStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _bufferedStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _bufferedStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_bufferedStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_bufferedStream.Write(buffer, offset, count);
		}
	}
}