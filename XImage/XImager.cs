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
		XImageRequest _request = null;

		public XImager(XImageRequest request)
		{
			_request = request;
		}

		public Dictionary<string, string> CopyTo(Stream inputStream, Stream outputStream)
		{
			var properties = new Dictionary<string, string>();

			using (var sourceImage = Bitmap.FromStream(inputStream) as Bitmap)
			{
				var targetImageSize = GetTargetImageSize(sourceImage.Size);
				var targetIsWiderThanOutput = GetIsTargetWiderThanOutput(targetImageSize);
				var outputDimensions = GetOutputDimensions(targetImageSize, targetIsWiderThanOutput);
				var origin = GetImageOrigin(targetImageSize, targetIsWiderThanOutput, outputDimensions);

				properties["X-Image-Original-Width"] = sourceImage.Width.ToString();
				properties["X-Image-Original-Height"] = sourceImage.Height.ToString();
				properties["X-Image-Original-Format"] = "image/" + new ImageFormatConverter().ConvertToString(sourceImage.RawFormat).ToLower();
				properties["X-Image-Width"] = outputDimensions.Width.ToString();
				properties["X-Image-Height"] = outputDimensions.Height.ToString();

				using (var outputImage = new Bitmap(outputDimensions.Width, outputDimensions.Height, PixelFormat.Format32bppArgb))
				{
					using (var graphics = Graphics.FromImage(outputImage))
					{
						ProcessImage(sourceImage, outputImage, graphics, outputStream, origin, targetImageSize, properties);
					}
				}
			}

			return properties;
		}

		void ProcessImage(Bitmap sourceImage, Bitmap outputImage, Graphics outputGraphics, Stream outputStream, Point origin, Size targetImageSize, Dictionary<string, string> properties)
		{
			var response = new XImageResponse(sourceImage, outputImage, outputGraphics, properties);

			if (_request.CropAsColor != null)
				outputGraphics.Clear(_request.CropAsColor.Value);

			//if (_encoder.MimeType == "image/png")
			//	outputGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

			outputGraphics.DrawImage(sourceImage, new Rectangle(origin, targetImageSize));

			var bitmapData = outputImage.LockBits(new Rectangle(Point.Empty, outputImage.Size), ImageLockMode.ReadWrite, outputImage.PixelFormat);
			var bytesPerPixel = Bitmap.GetPixelFormatSize(outputImage.PixelFormat) / 8;
			var byteCount = bitmapData.Stride * outputImage.Height;
			var pixelCount = byteCount / bytesPerPixel;
			var data = new byte[byteCount];
			Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);

			// Color Calculations
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
				properties["X-Image-Color-Average"] = averageColor.ToHex();

				var palette = histogram
					.OrderByDescending(p => p.Value)
					.Take(8)
					.Select(p => p.Key)
					.ToList();
				if (palette.Count > 0)
				{
					for (int i = 0; i < palette.Count; i++)
						palette[i] = Color.FromArgb(palette[i].R * histogramSize, palette[i].G * histogramSize, palette[i].B * histogramSize);
					properties["X-Image-Color-Palette"] = string.Join(",", palette.Select(c => c.ToHex()));

					properties["X-Image-Color-Dominant"] = palette
						.First()
						.ToHex();

					properties["X-Image-Color-Accent"] = palette
						.OrderByDescending(p => Math.Max(p.R, Math.Max(p.G, p.B)))
						.First()
						.ToHex();

					properties["X-Image-Color-Base"] = palette
						.OrderBy(p => Math.Max(p.R, Math.Max(p.G, p.B)))
						.First()
						.ToHex();
				}
			}

			for (int i = 0; i < _request.Filters.Count; i++)
				_request.Filters[i].ProcessImage(data, _request.FiltersArgs[i]);

			// Only copy bytes back if we made "edits."
			if (_request.Filters.Count > 0)
				Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);

			outputImage.UnlockBits(bitmapData);

			_request.Output.ProcessImage(outputImage, outputStream, _request.OutputArgs);
		}

		Size GetTargetImageSize(Size original)
		{
			if (_request.Width == null && _request.Height == null)
				return original;

			if (_request.Crop != null && (_request.Width == null || _request.Height == null))
				throw new ArgumentException("Cannot specify a fit without also specifying both width and height.");

			Size scaled = original;

			// If no fit is specified, default to clip.
			var fit = _request.Crop ?? Crops.NONE;

			// If upscaling is not allowed (the default), cap those values.
			var parametersWidth = _request.Width;
			if (parametersWidth != null && !_request.AllowUpscaling)
				parametersWidth = Math.Min(original.Width, parametersWidth.Value);
			var parametersHeight = _request.Height;
			if (parametersHeight != null && !_request.AllowUpscaling)
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

		bool GetIsTargetWiderThanOutput(Size targetImageSize)
		{
			if (_request.Crop == Crops.FILL || _request.Crop == Crops.COLOR)
				return (float)_request.Width.Value / (float)_request.Height.Value < (float)targetImageSize.Width / (float)targetImageSize.Height;
			else
				return false;
		}

		Size GetOutputDimensions(Size targetImageSize, bool targetIsWiderThanOutput)
		{
			Size outputDimensions = targetImageSize;

			if (_request.Crop == Crops.FILL || _request.Crop == Crops.COLOR)
			{
				outputDimensions.Width = _request.Width.Value;
				outputDimensions.Height = _request.Height.Value;

				if (!_request.AllowUpscaling && _request.Crop == Crops.FILL)
				{
					float scale = targetIsWiderThanOutput ? (float)targetImageSize.Height / (float)outputDimensions.Height : (float)targetImageSize.Width / (float)outputDimensions.Width;
					outputDimensions.Height = Convert.ToInt32(_request.Height * scale);
					outputDimensions.Width = Convert.ToInt32(_request.Width * scale);
				}
			}

			return outputDimensions;
		}

		Point GetImageOrigin(Size targetImageSize, bool targetIsWiderThanOutput, Size outputDimensions)
		{
			Point origin = Point.Empty;

			if (_request.Crop == Crops.FILL)
			{
				if (targetIsWiderThanOutput)
					origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
				else
					origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;
			}
			else if (_request.Crop == Crops.COLOR)
			{
				origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
				origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;
			}

			return origin;
		}
	}
}