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
				CalculatePalette(request, response);
				response.Profiler.Mark("Calculate color palette");

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
					foreach (var meta in request.Metas)
					{
						meta.Calculate(request, response);
						response.Profiler.Mark("Meta: " + meta.GetType().Name);
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

		static void Rasterize(XImageRequest request, XImageResponse response)
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

		static void CalculatePalette(XImageRequest request, XImageResponse response)
		{
			try
			{
				using (var bitmapBits = response.InputImage.GetBitmapBits())
				{
					var data = bitmapBits.Data;
					var pixelCount = 0;

					int r = 0, g = 0, b = 0;
					int rSum = 0, gSum = 0, bSum = 0;
					int rBucket = 0, gBucket = 0, bBucket = 0;
					var histogram = new Dictionary<Color, int>();
					int histogramSize = 32;
					for (int i = 0; i < data.Length; i += 4)
					{
						r = data[i + 2];
						g = data[i + 1];
						b = data[i];

						// Place colors in buckets for use on pallete.
						rBucket = (r / histogramSize);
						gBucket = (g / histogramSize);
						bBucket = (b / histogramSize);

						// Ignore greys.
						if (rBucket != gBucket || gBucket != bBucket || bBucket != rBucket)
						{
							pixelCount++;

							// Sum up channels for use on averages.
							rSum += r;
							gSum += g;
							bSum += b;

							var bucket = Color.FromArgb(rBucket, gBucket, bBucket);
							if (!histogram.ContainsKey(bucket))
								histogram[bucket] = 1;
							else
								histogram[bucket]++;
						}
					}

					if (pixelCount == 0)
						return;

					var rAvg = rSum / pixelCount;
					var gAvg = gSum / pixelCount;
					var bAvg = bSum / pixelCount;
					var averageColor = Color.FromArgb(rAvg, gAvg, bAvg);
					response.Palette["Average"] = averageColor;

					var palette = histogram
						.OrderByDescending(p => p.Value)
						.Take(8)
						.Select(p => p.Key)
						.ToList();
					if (palette.Count > 0)
					{
						for (int i = 0; i < palette.Count; i++)
						{
							palette[i] = Color.FromArgb(palette[i].R * histogramSize, palette[i].G * histogramSize, palette[i].B * histogramSize);
							response.Palette["Palette" + i] = palette[i];
						}

						response.Palette["Dominant"] = palette
							.First();

						response.Palette["Accent"] = palette
							.OrderByDescending(p => Math.Max(p.R, Math.Max(p.G, p.B)))
							.First();

						response.Palette["Base"] = palette
							.OrderBy(p => Math.Max(p.R, Math.Max(p.G, p.B)))
							.First();
					}
				}
			}
			catch
			{
				// TODO: This only works on PixelFormat.Format32bppArgb.  Fix that.
			}
		}
	}
}