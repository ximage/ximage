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
using XImage.Utilities;

namespace XImage
{
	public class XImager
	{
		// Width, Height, Crop, Filter, Mask, Text, Output
		public static readonly string[] XIMAGE_PARAMETERS = { "help", "w", "h", "c", "f", "m", "t", "o" };

		private static Stopwatch _stopwatch = Stopwatch.StartNew();

		public static void ProcessImage(XImageRequest request, XImageResponse response)
		{
			var timestamp = _stopwatch.ElapsedTicks;

			response.Properties["X-Image-Original-Width"] = response.InputImage.Width.ToString();
			response.Properties["X-Image-Original-Height"] = response.InputImage.Height.ToString();
			response.Properties["X-Image-Original-Format"] = "image/" + new ImageFormatConverter().ConvertToString(response.InputImage.RawFormat).ToLower();

			// ---------- Goes in ICrop ----------
			var targetImageSize = GetTargetImageSize(request, response.InputImage.Size);
			var targetIsWiderThanOutput = GetIsTargetWiderThanOutput(request, targetImageSize);
			var outputDimensions = GetOutputDimensions(request, targetImageSize, targetIsWiderThanOutput);
			var origin = GetImageOrigin(request, targetImageSize, targetIsWiderThanOutput, outputDimensions);
			response.OutputSize = outputDimensions;
			// -----------------------------------

			response.Properties["X-Image-Width"] = outputDimensions.Width.ToString();
			response.Properties["X-Image-Height"] = outputDimensions.Height.ToString();

			// ---------- Goes in ICrop ----------
			if (request.CropAsColor != null)
				response.OutputGraphics.Clear(request.CropAsColor.Value);
			// -----------------------------------

			//if (_encoder.MimeType == "image/png")
			//	outputGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

			response.OutputGraphics.DrawImage(response.InputImage, new Rectangle(origin, targetImageSize));

			var bitmapData = response.OutputImage.LockBits(new Rectangle(Point.Empty, response.OutputImage.Size), ImageLockMode.ReadWrite, response.OutputImage.PixelFormat);
			var bytesPerPixel = Bitmap.GetPixelFormatSize(response.OutputImage.PixelFormat) / 8;
			var byteCount = bitmapData.Stride * response.OutputImage.Height;
			var pixelCount = byteCount / bytesPerPixel;
			var data = new byte[byteCount];
			Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);

			// ---------- Goes in IFilter ----------
			for (int i = 0; i < request.Filters.Count; i++)
				request.Filters[i].ProcessImage(data);
			// -----------------------------------


			// ---------- Goes in IProperties ----------
			{
				int r = 0, g = 0, b = 0;
				int rSum = 0, gSum = 0, bSum = 0;
				int rBucket = 0, gBucket = 0, bBucket = 0;
				var histogram = new Dictionary<Color, int>();
				int histogramSize = 32;
				for (int i = 0; i < byteCount; i += 4)
				{
					r = data[i + 2];
					g = data[i + 1];
					b = data[i];

					// Sum up channels for use on averages.
					rSum += r;
					gSum += g;
					bSum += b;

					// Place colors in buckets for use on pallete.
					rBucket = (r / histogramSize);
					gBucket = (g / histogramSize);
					bBucket = (b / histogramSize);

					// Ignore greys.
					if (rBucket != gBucket || gBucket != bBucket || bBucket != rBucket)
					{
						var bucket = Color.FromArgb(rBucket, gBucket, bBucket);
						if (!histogram.ContainsKey(bucket))
							histogram[bucket] = 1;
						else
							histogram[bucket]++;
					}
				}
				var rAvg = rSum / pixelCount;
				var gAvg = gSum / pixelCount;
				var bAvg = bSum / pixelCount;
				var averageColor = Color.FromArgb(rAvg, gAvg, bAvg);
				response.Properties["X-Image-Color-Average"] = averageColor.ToHex();

				var palette = histogram
					.OrderByDescending(p => p.Value)
					.Take(8)
					.Select(p => p.Key)
					.ToList();
				if (palette.Count > 0)
				{
					for (int i = 0; i < palette.Count; i++)
						palette[i] = Color.FromArgb(palette[i].R * histogramSize, palette[i].G * histogramSize, palette[i].B * histogramSize);
					response.Properties["X-Image-Color-Palette"] = string.Join(",", palette.Select(c => c.ToHex()));

					response.Properties["X-Image-Color-Dominant"] = palette
						.First()
						.ToHex();

					response.Properties["X-Image-Color-Accent"] = palette
						.OrderByDescending(p => Math.Max(p.R, Math.Max(p.G, p.B)))
						.First()
						.ToHex();

					response.Properties["X-Image-Color-Base"] = palette
						.OrderBy(p => Math.Max(p.R, Math.Max(p.G, p.B)))
						.First()
						.ToHex();
				}
			}
			// -----------------------------------

			// Only copy bytes back if we made "edits."
			if (request.Filters.Count > 0)
				Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);

			response.OutputImage.UnlockBits(bitmapData);

			request.Output.ProcessImage(response.OutputImage, response.OutputStream);

			response.Properties.Add("X-Image-Processing-Time", string.Format("{0:N2}ms", 1000D * (double)(_stopwatch.ElapsedTicks - timestamp) / (double)Stopwatch.Frequency));
		}

		static Size GetTargetImageSize(XImageRequest request, Size original)
		{
			if (request.Width == null && request.Height == null)
				return original;

			if (request.Crop != null && (request.Width == null || request.Height == null))
				throw new ArgumentException("Cannot specify a fit without also specifying both width and height.");

			Size scaled = original;

			// If no fit is specified, default to clip.
			var fit = request.Crop ?? Crops.NONE;

			// If upscaling is not allowed (the default), cap those values.
			var parametersWidth = request.Width;
			if (parametersWidth != null && !request.AllowUpscaling)
				parametersWidth = Math.Min(original.Width, parametersWidth.Value);
			var parametersHeight = request.Height;
			if (parametersHeight != null && !request.AllowUpscaling)
				parametersHeight = Math.Min(original.Height, parametersHeight.Value);

			// In the event that just one dimension was specified, i.e. just w or just h,
			// then extrapolate the missing dimension.  This should only occur when fit is null.
			int w = parametersWidth ?? Convert.ToInt32(original.Width * parametersHeight.Value / (float)original.Height);
			int h = parametersHeight ?? Convert.ToInt32(original.Height * parametersWidth.Value / (float)original.Width);

			switch (fit)
			{
				case Crops.NONE:
				case Crops.COLOR:
					if ((float)w / (float)h < (float)original.Width / (float)original.Height)
					{
						scaled.Width = w;
						scaled.Height = Convert.ToInt32(original.Height * w / (float)original.Width);
					}
					else
					{
						scaled.Height = h;
						scaled.Width = Convert.ToInt32(original.Width * h / (float)original.Height);
					}
					break;
				case Crops.FILL:
					if ((float)w / (float)h > (float)original.Width / (float)original.Height)
					{
						scaled.Width = w;
						scaled.Height = Convert.ToInt32(original.Height * w / (float)original.Width);
					}
					else
					{
						scaled.Height = h;
						scaled.Width = Convert.ToInt32(original.Width * h / (float)original.Height);
					}
					break;
				case Crops.STRETCH:
					scaled.Width = w;
					scaled.Height = h;
					break;
			}

			return scaled;
		}

		static bool GetIsTargetWiderThanOutput(XImageRequest request, Size targetImageSize)
		{
			if (request.Crop == Crops.FILL || request.Crop == Crops.COLOR)
				return (float)request.Width.Value / (float)request.Height.Value < (float)targetImageSize.Width / (float)targetImageSize.Height;
			else
				return false;
		}

		static Size GetOutputDimensions(XImageRequest request, Size targetImageSize, bool targetIsWiderThanOutput)
		{
			Size outputDimensions = targetImageSize;

			if (request.Crop == Crops.FILL || request.Crop == Crops.COLOR)
			{
				outputDimensions.Width = request.Width.Value;
				outputDimensions.Height = request.Height.Value;

				if (!request.AllowUpscaling && request.Crop == Crops.FILL)
				{
					float scale = targetIsWiderThanOutput ? (float)targetImageSize.Height / (float)outputDimensions.Height : (float)targetImageSize.Width / (float)outputDimensions.Width;
					outputDimensions.Height = Convert.ToInt32(request.Height * scale);
					outputDimensions.Width = Convert.ToInt32(request.Width * scale);
				}
			}

			return outputDimensions;
		}

		static Point GetImageOrigin(XImageRequest request, Size targetImageSize, bool targetIsWiderThanOutput, Size outputDimensions)
		{
			Point origin = Point.Empty;

			if (request.Crop == Crops.FILL)
			{
				if (targetIsWiderThanOutput)
					origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
				else
					origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;
			}
			else if (request.Crop == Crops.COLOR)
			{
				origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
				origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;
			}

			return origin;
		}
	}
}