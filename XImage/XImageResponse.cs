using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage
{
	public class XImageResponse : IDisposable
	{
		public Stream InputStream { get; private set; }

		private Bitmap _inputImage = null;
		public Bitmap InputImage
		{
			get
			{
				if (_inputImage == null)
					_inputImage = Bitmap.FromStream(InputStream) as Bitmap;
				return _inputImage;
			}
		}

		public Rectangle InputBounds { get; set; }

		public Size OutputSize { get; set; }

		private Bitmap _outputImage = null;
		public Bitmap OutputImage
		{
			get
			{
				if (_outputImage == null)
					_outputImage = new Bitmap(OutputSize.Width, OutputSize.Height, PixelFormat.Format32bppArgb);
				return _outputImage;
			}
		}

		private Graphics _outputGraphics = null;
		public Graphics OutputGraphics
		{
			get
			{
				if (_outputGraphics == null)
					_outputGraphics = Graphics.FromImage(OutputImage);
				return _outputGraphics;
			}
		}

		public Stream OutputStream { get; private set; }

		public NameValueCollection Properties { get; private set; }

		public XImageResponse(HttpContext httpContext)
		{
			InputStream = httpContext.Response.Filter;
			OutputStream = httpContext.Response.OutputStream;
			Properties = httpContext.Response.Headers;
		}

		public void Dispose()
		{
			if (_inputImage != null)
				_inputImage.Dispose();
			if (_outputImage != null)
				_outputImage.Dispose();
			if (_outputGraphics != null)
				_outputGraphics.Dispose();

			// Don't dispose the streams.
		}
	}
}