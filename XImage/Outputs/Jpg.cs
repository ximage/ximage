using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Outputs
{
	public class Jpg : IOutput
	{
		static readonly int DEFAULT_QUALITY = ConfigurationManager.AppSettings["XImage.DefaultQuality"].AsNullableInt() ?? 75;
		static readonly ImageCodecInfo _encoder = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

		long _quality;
		bool _asKb;

		public string ContentType { get { return "image/jpeg"; } }

		public bool SupportsTransparency { get { return false; } }

		public Jpg()
		{
			_quality = DEFAULT_QUALITY;
			_asKb = false;
		}

		public Jpg(int quality)
		{
			_quality = quality;
			_asKb = false;

			if (_quality <= 0 || _quality > 100)
				throw new ArgumentException("Quality must be an integer between 1 and 100.");
		}

		public Jpg(string quality) // e.g. 150kb
		{
			_asKb = quality.EndsWith("kb");
			if (_asKb)
				quality = quality.Replace("kb", "");
			_quality = quality.AsNullableInt() ?? DEFAULT_QUALITY;

			if (!quality.AsNullableInt().HasValue)
				throw new ArgumentException("Quality must be an integer between 1 and 100 or a size in kb, e.g. 150kb.");
			else if (_quality <= 0)
				throw new ArgumentException("Quality must be an integer between 1 and 100.");
			else if (!_asKb && _quality > 100)
				throw new ArgumentException("Quality must be an integer between 1 and 100 unless you are specifing a content size, e.g. 150kb.");
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			var encoderParameters = new EncoderParameters(2);
			encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
			encoderParameters.Param[1] = new EncoderParameter(Encoder.Quality, _quality);
			int saveAttempts = 5;

			if (_asKb)
				BinarySearchImageQuality(response.OutputImage, response.OutputStream, encoderParameters, ref saveAttempts, 1, 100);
			else
				response.OutputImage.Save(response.OutputStream, _encoder, encoderParameters);
		}

		void BinarySearchImageQuality(Bitmap inputImage, Stream outputStream, EncoderParameters encoderParams, ref int saveAttempts, long lowerRange, long upperRange)
		{
			long targetSize = _quality * 1024; // kilobytes to bytes
			long testQuality = (upperRange - lowerRange) / 2L + lowerRange;
			using (var mem = new MemoryStream())
			{
				encoderParams.Param[1] = new EncoderParameter(Encoder.Quality, testQuality);
				inputImage.Save(mem, _encoder, encoderParams);

				// If the sizes are within 10%, we're good to go.
				var closeness = (float)Math.Abs(mem.Length - targetSize) / (float)targetSize;
				if (closeness < .1F || --saveAttempts <= 0)
				{
					mem.Position = 0;
					mem.CopyTo(outputStream);
				}
				else if (targetSize < mem.Length)
				{
					BinarySearchImageQuality(inputImage, outputStream, encoderParams, ref saveAttempts, lowerRange, testQuality);
				}
				else
				{
					BinarySearchImageQuality(inputImage, outputStream, encoderParams, ref saveAttempts, testQuality, upperRange);
				}
			}
		}
	}
}