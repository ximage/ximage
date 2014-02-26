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
	
		public string MethodName { get { return "jpg"; } }

		public string MethodDescription
		{
			get { return string.Format("Uses a jpg encoder.  Optionally supply quality as a percentage, an integer between 1-100 (default {0}).", DEFAULT_QUALITY); }
		}

		public string[] ExampleQueryStrings { get { return new string[] { "jpg", "jpg(10)" }; } }
		
		public string ContentType { get { return "image/jpg"; } }

		public void ProcessImage(Bitmap outputImage, Stream outputStream, params string[] args)
		{
			long quality;
			bool asKb;
			ParseArgs(out quality, out asKb, args);

			var encoderParameters = new EncoderParameters(2);
			encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
			encoderParameters.Param[1] = new EncoderParameter(Encoder.Quality, quality);
			int saveAttempts = 5;

			if (asKb)
				BinarySearchImageQuality(outputImage, outputStream, quality, encoderParameters, ref saveAttempts, 1, 100);
			else
				outputImage.Save(outputStream, _encoder, encoderParameters);
		}

		void BinarySearchImageQuality(Bitmap canvas, Stream outputStream, long quality, EncoderParameters encoderParams, ref int saveAttempts, long lowerRange, long upperRange)
		{
			long targetSize = quality * 1024; // kilobytes to bytes
			long testQuality = (upperRange - lowerRange) / 2L + lowerRange;
			using (var mem = new MemoryStream())
			{
				encoderParams.Param[1] = new EncoderParameter(Encoder.Quality, testQuality);
				canvas.Save(mem, _encoder, encoderParams);

				// If the sizes are within 10%, we're good to go.
				var closeness = (float)Math.Abs(mem.Length - targetSize) / (float)targetSize;
				if (closeness < .1F || --saveAttempts <= 0)
				{
					mem.Position = 0;
					mem.CopyTo(outputStream);
				}
				else if (targetSize < mem.Length)
				{
					BinarySearchImageQuality(canvas, outputStream, quality, encoderParams, ref saveAttempts, lowerRange, testQuality);
				}
				else
				{
					BinarySearchImageQuality(canvas, outputStream, quality, encoderParams, ref saveAttempts, testQuality, upperRange);
				}
			}
		}

		void ParseArgs(out long quality, out bool asKb, params string[] args)
		{
			quality = DEFAULT_QUALITY;
			asKb = false;
			if (args == null || args.Length == 0)
				return;

			var q = args[0];
			asKb = q.EndsWith("kb");
			if (asKb)
				q = q.Substring(0, q.Length - 2);
			quality = q.AsNullableInt() ?? DEFAULT_QUALITY;

			if (!q.AsNullableInt().HasValue)
				throw new ArgumentException("Quality must be an integer between 1 and 100 or a size in kb, e.g. 150kb.");
			if (quality <= 0)
				throw new ArgumentException("Quality must be an integer between 1 and 100.");
			if (!asKb && quality > 100)
				throw new ArgumentException("Quality must be an integer between 1 and 100 unless you are specifing a content size, e.g. 150kb.");
		}
	}
}