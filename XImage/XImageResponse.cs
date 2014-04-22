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
		public Bitmap InputImage { get; set; }

		public Rectangle CropBox { get; set; }

		public Rectangle ContentArea { get; set; }

		public Size CanvasSize { get; set; }

		private Bitmap _outputImage = null;
		public Bitmap OutputImage
		{
			get
			{
				if (_outputImage == null)
					_outputImage = new Bitmap(CanvasSize.Width, CanvasSize.Height, PixelFormat.Format32bppArgb);
				return _outputImage;
			}
			set
			{
				_outputImage = value;
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
			set
			{
				_outputGraphics = value;
			}
		}

		public ImageAttributes ImageAttributes { get; private set; }

		public Stream OutputStream { get; private set; }

		public NameValueCollection Properties { get; private set; }

		public XImageDiagnostics Diagnostics { get; private set; }

		public XImageResponse(HttpContext httpContext)
		{
			InputImage = Bitmap.FromStream(httpContext.Response.Filter) as Bitmap;
			CropBox = new Rectangle(Point.Empty, InputImage.Size);
			CanvasSize = InputImage.Size;
			ContentArea = new Rectangle(Point.Empty, CanvasSize);
			ImageAttributes = new ImageAttributes();
			OutputStream = httpContext.Response.OutputStream;
			Properties = httpContext.Response.Headers;
			Diagnostics = new XImageDiagnostics(Properties);
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