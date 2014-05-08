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
		public static readonly string[] XIMAGE_PARAMETERS = { "help", "w", "width", "h", "height", "f", "filter", "filters", "o", "output", "debug" };

		public static void ProcessImage(XImageRequest request, XImageResponse response)
		{
			using (response.Profiler.Measure("X-Image-Time-Total"))
			{
				// --- FILTERS ---
				using (response.Profiler.Measure("X-Image-Time-Filters"))
				{
					new Fit().PreProcess(request, response); // Default crop to fit to ensure canvas size is set.
					response.Profiler.Mark("Filter.PreProcess: Fit (default)");

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
					foreach (var output in request.Outputs)
					{
						output.PostProcess(request, response);
						response.Profiler.Mark("Image encoded: " + output.GetType().Name);
					}
				}
			}
		}

		public static void Rasterize(XImageRequest request, XImageResponse response)
		{
			var canvasSize = response.CanvasSize;
			var contentArea = response.ContentArea;
			var cropBox = response.CropBox;

			// Set the OutputImage and OutputGraphics objects which can be used in the PostProcess methods.
			response.OutputImage = new Bitmap(canvasSize.Width, canvasSize.Height, PixelFormat.Format32bppArgb);
			response.OutputGraphics = Graphics.FromImage(response.OutputImage);
			response.OutputGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic; // TODO: Make this a setting.
			response.OutputGraphics.SmoothingMode = SmoothingMode.HighQuality;
			response.OutputGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			//response.OutputGraphics.CompositingQuality = CompositingQuality.HighQuality;

			// Apply any vector transformations.
			response.OutputGraphics.Transform = response.VectorTransform;

			// Set the background.
			var clearColor = request.Outputs.Exists(o => o.SupportsTransparency) ? Color.Transparent : Color.White;
			response.OutputGraphics.Clear(clearColor);

			// Draw the image in the proper position with the proper image attributes (color transformations).
			response.OutputGraphics.DrawImage(
				image: response.InputImage,
				destRect: new Rectangle(contentArea.X, contentArea.Y, contentArea.Width, contentArea.Height),
				srcX: cropBox.X,
				srcY: cropBox.Y,
				srcWidth: cropBox.Width,
				srcHeight: cropBox.Height,
				srcUnit: GraphicsUnit.Pixel,
				imageAttr: response.ImageAttributes);
		}
	}
}