using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage
{
	public class XImageResponse : IDisposable
	{
		public Bitmap InputImage { get; private set; }

		public Rectangle CropBox { get; set; }

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
				{
					_outputGraphics = Graphics.FromImage(OutputImage);
					_outputGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic; // TODO: Make this a setting.
					_outputGraphics.SmoothingMode = SmoothingMode.HighQuality;
					//_outputGraphics.CompositingQuality = CompositingQuality.HighQuality;
					_outputGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				}
				return _outputGraphics;
			}
		}

		public Stream OutputStream { get; private set; }

		public NameValueCollection Properties { get; private set; }

		public XImageResponse(HttpContext httpContext)
		{
			InputImage = Bitmap.FromStream(httpContext.Response.Filter) as Bitmap;
			CropBox = new Rectangle(Point.Empty, InputImage.Size);
			OutputSize = InputImage.Size;
			OutputStream = httpContext.Response.OutputStream;
			Properties = httpContext.Response.Headers;
		}

		public void Dispose()
		{
			if (InputImage != null)
				InputImage.Dispose();
			if (_outputImage != null)
				_outputImage.Dispose();
			if (_outputGraphics != null)
				_outputGraphics.Dispose();

			// Don't dispose the stream.
		}
	}
}