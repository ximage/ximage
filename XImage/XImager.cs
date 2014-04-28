using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using XImage.Filters;
using XImage.Utilities;

namespace XImage
{
	public class XImager
	{
		public static readonly string[] XIMAGE_PARAMETERS = { "help", "w", "width", "h", "height", "f", "filter", "filters", "o", "output" };

		public static void ProcessImage(XImageRequest request, XImageResponse response)
		{
			using (response.Profiler.Measure("X-Image-Time-Total"))
			{
				SetCanvasDimensions(request, response);
				response.Profiler.Mark("Canvas dimensions calculated");

				// --- FILTERS ---
				using (response.Profiler.Measure("X-Image-Time-Filters"))
				{
					foreach (var filter in request.Filters)
					{
						filter.PreProcess(request, response);
						response.Profiler.Mark("Filter.PreProcess: " + filter.GetType().Name);
					}

					Rasterize(request, response);
					response.Profiler.Mark("Rasterize");

					foreach (var filter in request.Filters)
					{
						filter.PostProcess(request, response);
						response.Profiler.Mark("Filter.PostProcess: " + filter.GetType().Name);
					}
				}

				// --- METAS ---
				using (response.Profiler.Measure("X-Image-Time-Metas"))
				{
					using (var bitmapBits = response.OutputImage.GetBitmapBits())
					{
						foreach (var meta in request.Metas)
						{
							meta.Calculate(request, response, bitmapBits.Data);
							response.Profiler.Mark("Meta: " + meta.GetType().Name);
						}
					}
				}

				// --- OUTPUT ---
				using (response.Profiler.Measure("X-Image-Time-Output"))
				{
					request.Output.PostProcess(request, response);
					response.Profiler.Mark("Image encoded");
				}
			}
		}

		public static void Rasterize(XImageRequest request, XImageResponse response)
		{
			var graphics = response.OutputGraphics;
			var canvasSize = response.CanvasSize;
			var contentArea = response.ContentArea;
			var cropBox = response.CropBox;

			graphics.Clear(request.Output.SupportsTransparency ? Color.Transparent : Color.White);

			graphics.TranslateTransform(canvasSize.Width / 2, canvasSize.Height / 2, MatrixOrder.Append);
			graphics.DrawImage(
				image: response.InputImage,
				destRect: new Rectangle(contentArea.Width / -2, contentArea.Height / -2, contentArea.Width, contentArea.Height),
				srcX: cropBox.X,
				srcY: cropBox.Y,
				srcWidth: cropBox.Width,
				srcHeight: cropBox.Height,
				srcUnit: GraphicsUnit.Pixel,
				imageAttr: response.ImageAttributes);
		}

		static void SetCanvasDimensions(XImageRequest request, XImageResponse response)
		{
			// TODO: Simplify...

			if (request.Width == null && request.Height == null)
			{
				// Do nothing here, but prevents the other conditions from running.
			}
			else if (request.Width != null && request.Height != null)
			{
				var size = new Size(request.Width.Value, request.Height.Value);

				// Unless upscaling is allowed, don't let the canvas size be larger than the input image.
				if (!request.AllowUpscaling && size.Width > response.InputImage.Width)
					size = size.ScaleToWidth(response.InputImage.Width);

				// Unless upscaling is allowed, don't let the canvas size be larger than the input image.
				if (!request.AllowUpscaling && size.Height > response.InputImage.Height)
					size = size.ScaleToHeight(response.InputImage.Height);

				response.CanvasSize = size;
			}
			else if (request.Width != null) // Implies that height == null, so infer the height.
			{
				// Start by scaling the canvas porportionally until its width is w.
				var size = response.CanvasSize.ScaleToWidth(request.Width.Value);

				// Unless upscaling is allowed, don't let the canvas size be larger than the input image.
				if (!request.AllowUpscaling && size.Height > response.InputImage.Height)
					size = size.ScaleToHeight(response.InputImage.Height);

				// In some cases the infered height will end up larger than MAX_SIZE.  Bring it back down some.
				if (size.Height > XImageRequest.MAX_SIZE)
					size = response.CanvasSize.ScaleToHeight(XImageRequest.MAX_SIZE);

				response.CanvasSize = size;
			}
			else if (request.Height != null) // Implies that width == null, so infer the width.
			{
				// Start by scaling the canvas porportionally until its height is h.
				var size = response.CanvasSize.ScaleToHeight(request.Height.Value);

				// Unless upscaling is allowed, don't let the canvas size be larger than the input image.
				if (!request.AllowUpscaling && size.Width > response.InputImage.Width)
					size = size.ScaleToWidth(response.InputImage.Width);

				// In some cases the infered width will end up larger than MAX_SIZE.  Bring it back down some.
				if (size.Width > XImageRequest.MAX_SIZE)
					size = response.CanvasSize.ScaleToWidth(XImageRequest.MAX_SIZE);

				response.CanvasSize = size;
			}

			// By default, use the Fit crop.
			new Fit().PreProcess(request, response);

			// By default the content area is the full canvas.
			// TODO: 9-patch logic goes here.
			response.ContentArea = new Rectangle(Point.Empty, response.CanvasSize);
		}
	}
}