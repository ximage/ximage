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

namespace XImage
{
	public class XImager
	{
		static readonly int DEFAULT_QUALITY = ConfigurationManager.AppSettings["XImage.DefaultQuality"].AsNullableInt() ?? 75;

		static readonly ImageCodecInfo _jpgEncoder = null;
		static readonly ImageCodecInfo _gifEncoder = null;
		static readonly ImageCodecInfo _pngEncoder = null;
		static readonly Dictionary<ImageFormat, ImageCodecInfo> _formatToCodec = null;

		XImageParameters _parameters = null;
		EncoderParameters _encoderParameters = null;
		ImageCodecInfo _encoder = null;
		int saveAttempts = 0;

		static XImager()
		{
			var codecs = ImageCodecInfo.GetImageEncoders();
			_jpgEncoder = codecs.First(c => c.MimeType == "image/jpeg");
			_gifEncoder = codecs.First(c => c.MimeType == "image/gif");
			_pngEncoder = codecs.First(c => c.MimeType == "image/png");
			_formatToCodec = new Dictionary<ImageFormat, ImageCodecInfo> 
			{
				{ ImageFormat.Jpeg, _jpgEncoder },
				{ ImageFormat.Gif, _gifEncoder },
				{ ImageFormat.Png, _pngEncoder },
			};
		}

		public XImager(XImageParameters parameters)
		{
			_parameters = parameters;

			_encoderParameters = new EncoderParameters(2);
			_encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
			_encoderParameters.Param[1] = new EncoderParameter(Encoder.Quality, (long)(_parameters.Quality ?? DEFAULT_QUALITY));

			_encoder = _formatToCodec[parameters.OutputFormat ?? parameters.SourceFormat ?? ImageFormat.Jpeg];
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

				using (var canvas = new Bitmap(outputDimensions.Width, outputDimensions.Height, PixelFormat.Format32bppArgb))
				{
					using (var graphics = Graphics.FromImage(canvas))
					{
						ProcessImage(sourceImage, canvas, graphics, origin, targetImageSize, properties);
						SaveImage(canvas, outputStream);
					}
				}
			}

			return properties;
		}

		void ProcessImage(Bitmap sourceImage, Bitmap targetImage, Graphics graphics, Point origin, Size targetImageSize, Dictionary<string, string> properties)
		{
			if (_parameters.CropAsColor != null)
				graphics.Clear(_parameters.CropAsColor.Value);

			if (_encoder.MimeType == "image/png")
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

			graphics.DrawImage(sourceImage, new Rectangle(origin, targetImageSize));

			var bitmapData = targetImage.LockBits(new Rectangle(Point.Empty, targetImage.Size), ImageLockMode.ReadWrite, targetImage.PixelFormat);
			var bytesPerPixel = Bitmap.GetPixelFormatSize(targetImage.PixelFormat) / 8;
			var byteCount = bitmapData.Stride * targetImage.Height;
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

			// Only do this if we made "edits."
			// Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
			targetImage.UnlockBits(bitmapData);
		}

		Size GetTargetImageSize(Size original)
		{
			if (_parameters.Width == null && _parameters.Height == null)
				return original;

			if (_parameters.Crop != null && (_parameters.Width == null || _parameters.Height == null))
				throw new ArgumentException("Cannot specify a fit without also specifying both width and height.");

			Size scaled = original;

			// If no fit is specified, default to clip.
			var fit = _parameters.Crop ?? Crops.NONE;

			// If upscaling is not allowed (the default), cap those values.
			var parametersWidth = _parameters.Width;
			if (parametersWidth != null && !_parameters.AllowUpscaling)
				parametersWidth = Math.Min(original.Width, parametersWidth.Value);
			var parametersHeight = _parameters.Height;
			if (parametersHeight != null && !_parameters.AllowUpscaling)
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
			if (_parameters.Crop == Crops.FILL || _parameters.Crop == Crops.COLOR)
				return (float)_parameters.Width.Value / (float)_parameters.Height.Value < (float)targetImageSize.Width / (float)targetImageSize.Height;
			else
				return false;
		}

		Size GetOutputDimensions(Size targetImageSize, bool targetIsWiderThanOutput)
		{
			Size outputDimensions = targetImageSize;

			if (_parameters.Crop == Crops.FILL || _parameters.Crop == Crops.COLOR)
			{
				outputDimensions.Width = _parameters.Width.Value;
				outputDimensions.Height = _parameters.Height.Value;

				if (!_parameters.AllowUpscaling && _parameters.Crop == Crops.FILL)
				{
					float scale = targetIsWiderThanOutput ? (float)targetImageSize.Height / (float)outputDimensions.Height : (float)targetImageSize.Width / (float)outputDimensions.Width;
					outputDimensions.Height = Convert.ToInt32(_parameters.Height * scale);
					outputDimensions.Width = Convert.ToInt32(_parameters.Width * scale);
				}
			}

			return outputDimensions;
		}

		Point GetImageOrigin(Size targetImageSize, bool targetIsWiderThanOutput, Size outputDimensions)
		{
			Point origin = Point.Empty;

			if (_parameters.Crop == Crops.FILL)
			{
				if (targetIsWiderThanOutput)
					origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
				else
					origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;
			}
			else if (_parameters.Crop == Crops.COLOR)
			{
				origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
				origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;
			}

			return origin;
		}

		void SaveImage(Bitmap canvas, Stream outputStream)
		{
			if (_parameters.QualityAsKb && _encoder.MimeType == "image/jpeg")
				BinarySearchImageQuality(canvas, outputStream, 1, 100);
			else
				canvas.Save(outputStream, _encoder, _encoderParameters);
		}


		void BinarySearchImageQuality(Bitmap canvas, Stream outputStream, long lowerRange, long upperRange)
		{
			long targetSize = _parameters.Quality.Value * 1024;
			long testQuality = (upperRange - lowerRange) / 2L + lowerRange;
			using (var mem = new MemoryStream())
			{
				_encoderParameters.Param[1] = new EncoderParameter(Encoder.Quality, testQuality);
				canvas.Save(mem, _encoder, _encoderParameters);

				// If the sizes are within 10%, we're good to go.
				var closeness = (float)Math.Abs(mem.Length - targetSize) / (float)targetSize;
				if (closeness < .1F || ++saveAttempts >= 5)
				{
					mem.Position = 0;
					mem.CopyTo(outputStream);
				}
				else if (targetSize < mem.Length)
				{
					BinarySearchImageQuality(canvas, outputStream, lowerRange, testQuality);
				}
				else
				{
					BinarySearchImageQuality(canvas, outputStream, testQuality, upperRange);
				}
			}
		}
	}
}