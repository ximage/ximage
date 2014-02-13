using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage
{
	public class XImageFilterStream : Stream
	{
		MemoryStream _sourceStream = new MemoryStream();
		Stream _outputStream;
		XImageParameters _parameters;

		public override bool CanRead
		{
			get { return _sourceStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _sourceStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _sourceStream.CanWrite; }
		}

		public override long Length
		{
			get { return _sourceStream.Length; }
		}

		public override long Position
		{
			get { return _sourceStream.Position; }
			set { _sourceStream.Position = value; }
		}

		public XImageFilterStream(Stream outputStream, XImageParameters parameters)
		{
			_outputStream = outputStream;
			_parameters = parameters;
		}

		public override void Close()
		{
			new XImager(_parameters).Generate(_sourceStream, _outputStream);
			_outputStream.Close();
			_sourceStream.Close();
		}

		public override void Flush()
		{
			_sourceStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _sourceStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _sourceStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_sourceStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_sourceStream.Write(buffer, offset, count);
		}
	}
}