using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage
{
	public class XImager
	{
		static readonly int DEFAULT_QUALITY = ConfigurationManager.AppSettings["XImage.DefaultQuality"].AsNullableInt() ?? 75;

		static readonly ImageCodecInfo _jpgEncoder = null;
		static readonly ImageCodecInfo _gifEncoder = null;
		static readonly ImageCodecInfo _pngEncoder = null;

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
		}

		public XImager(XImageParameters parameters)
		{
			_parameters = parameters;
			_encoderParameters = new EncoderParameters(2);
			_encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
			_encoderParameters.Param[1] = new EncoderParameter(Encoder.Quality, (long)(_parameters.Quality ?? DEFAULT_QUALITY));

			var format = parameters.OutputFormat ?? parameters.SourceFormat;
			if (format == ImageFormat.Jpeg)
				_encoder = _jpgEncoder;
			else if (format == ImageFormat.Gif)
				_encoder = _gifEncoder;
			else if (format == ImageFormat.Png)
				_encoder = _pngEncoder;
			else
				_encoder = _jpgEncoder;
		}

		public void Generate(Stream inputStream, Stream outputStream)
		{
			using (var sourceImage = Bitmap.FromStream(inputStream))
			{
				var targetImageSize = GetScaledDimensions(_parameters, sourceImage.Size);
				var outputDimensions = targetImageSize;
				var targetIsWiderThanOutput = false;

				if (_parameters.Crop == Crops.FILL || _parameters.Crop == Crops.COLOR)
				{
					outputDimensions.Width = _parameters.Width.Value;
					outputDimensions.Height = _parameters.Height.Value;
					targetIsWiderThanOutput = (float)outputDimensions.Width / (float)outputDimensions.Height < (float)targetImageSize.Width / (float)targetImageSize.Height;

					if (!_parameters.AllowUpscaling && _parameters.Crop == Crops.FILL)
					{
						float scale = targetIsWiderThanOutput ? (float)targetImageSize.Height / (float)outputDimensions.Height : (float)targetImageSize.Width / (float)outputDimensions.Width;
						outputDimensions.Height = Convert.ToInt32(_parameters.Height * scale);
						outputDimensions.Width = Convert.ToInt32(_parameters.Width * scale);
					}
				}

				using (var canvas = new Bitmap(outputDimensions.Width, outputDimensions.Height, PixelFormat.Format32bppArgb))
				{
					using (var graphics = Graphics.FromImage(canvas))
					{
						if (_parameters.CropAsColor != null)
							graphics.Clear(_parameters.CropAsColor.Value);

						if (_encoder.MimeType == "image/png")
							graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

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

						graphics.DrawImage(sourceImage, new Rectangle(origin, targetImageSize));

						if (_parameters.QualityAsKb && _encoder.MimeType == "image/jpeg")
							BinarySearchImageQuality(outputStream, canvas, 1, 100);
						else
							canvas.Save(outputStream, _encoder, _encoderParameters);
					}
				}
			}
		}

		void BinarySearchImageQuality(Stream outputStream, Bitmap canvas, long lowerRange, long upperRange)
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
					BinarySearchImageQuality(outputStream, canvas, lowerRange, testQuality);
				}
				else
				{
					BinarySearchImageQuality(outputStream, canvas, testQuality, upperRange);
				}
			}
		}

		Size GetScaledDimensions(XImageParameters parameters, Size original)
		{
			if (parameters.Width == null && parameters.Height == null)
				return original;

			if (parameters.Crop != null && (parameters.Width == null || parameters.Height == null))
				throw new ArgumentException("Cannot specify a fit without also specifying both width and height.");

			Size scaled = original;

			// If no fit is specified, default to clip.
			var fit = parameters.Crop ?? Crops.NONE;

			// If upscaling is not allowed (the default), cap those values.
			var parametersWidth = parameters.Width;
			if (parametersWidth != null && !parameters.AllowUpscaling)
				parametersWidth = Math.Min(original.Width, parametersWidth.Value);
			var parametersHeight = parameters.Height;
			if (parametersHeight != null && !parameters.AllowUpscaling)
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
	}
}