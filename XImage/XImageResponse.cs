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

		public Matrix VectorTransform { get; set; }

		public ImageAttributes ImageAttributes { get; set; }

		private Bitmap _outputImage = null;
		public Bitmap OutputImage
		{
			get
			{
				if (_outputImage == null)
					throw new ApplicationException("OutputImage is not accessible until the PostProcess stage of the pipeline.");
				return _outputImage;
			}
			internal set
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
					throw new ApplicationException("OutputGraphics is not accessible until the PostProcess stage of the pipeline.");
				return _outputGraphics;
			}
			internal set
			{
				_outputGraphics = value;
			}
		}

		public Stream OutputStream { get; private set; }

		public NameValueCollection Properties { get; private set; }

		public XImageProfiler Profiler { get; private set; }

		public XImageResponse(HttpContext httpContext, XImageProfiler profiler = null)
		{
			InputImage = Bitmap.FromStream(httpContext.Response.Filter) as Bitmap;
			CropBox = new Rectangle(Point.Empty, InputImage.Size);
			CanvasSize = InputImage.Size;
			ContentArea = new Rectangle(Point.Empty, CanvasSize);
			ImageAttributes = new ImageAttributes();
			VectorTransform = new Matrix();
			OutputStream = httpContext.Response.OutputStream;
			Properties = httpContext.Response.Headers;
			Profiler = profiler ?? new XImageProfiler(Properties);

			// If debugging, don't dump the image down the response stream.
			if (HttpUtility.ParseQueryString(httpContext.Request.Url.Query).ContainsKey("debug"))
				OutputStream = new MemoryStream();
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