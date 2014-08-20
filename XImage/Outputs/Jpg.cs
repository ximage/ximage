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
	[Documentation(Text = @"Uses a JPEG encoder.  It uses a default quality of 75 which can be specified in settings.
							Additionally, specify a custom quality with values 1-100.  To get real fancy, target 
							a specific file size by appending kb after the number.  It does a best-effort attempt 
							to get the image within 10% of the desired file size.  Not all sizes are feasible.")]
	public class Jpg : IOutput
	{
		static readonly int DEFAULT_QUALITY = ConfigurationManager.AppSettings["XImage.DefaultQuality"].AsNullableInt() ?? 75;
		static readonly ImageCodecInfo _encoder = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

		long _quality;
		bool _useTargetSize;

		public string ContentType { get { return "image/jpeg"; } }

		public bool SupportsTransparency { get { return false; } }

		[Example(QueryString = "?w=100&o=jpg")]
		public Jpg()
		{
			_quality = DEFAULT_QUALITY;
			_useTargetSize = false;
		}

		[Example(QueryString = "?w=100&o=jpg(20)")]
		public Jpg(decimal quality)
		{
			_quality = Convert.ToInt64(quality);
			_useTargetSize = false;

			if (_quality <= 0 || _quality > 100)
				throw new ArgumentException("Quality must be an integer between 1 and 100.");
		}

		[Example(QueryString = "?w=100&o=jpg(50kb)")]
		public Jpg(string targetSize) // e.g. 150kb
		{
			targetSize = targetSize.ToLower();
			_useTargetSize = true;

			long multiplier = 1;
			if (targetSize.EndsWith("mb"))
			{
				targetSize = targetSize.Replace("mb", "");
				multiplier = 1024 * 1024;
			}
			else if (targetSize.EndsWith("kb"))
			{
				targetSize = targetSize.Replace("kb", "");
				multiplier = 1024;
			}
			else if (targetSize.EndsWith("b"))
			{
				targetSize = targetSize.Replace("b", "");
				multiplier = 1;
			}
			else
			{
				throw new ArgumentException("When specifing a target size, please use the format 90b, 90kb or 90mb.");
			}

			if (!targetSize.AsNullableInt().HasValue)
				throw new ArgumentException("When specifing a target size, please use the format 90b, 90kb or 90mb.");

			_quality = targetSize.AsNullableInt().Value * multiplier;

			if (_quality <= 0)
				throw new ArgumentException("Quality must be an integer between 1 and 100.");
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

			if (_useTargetSize)
				BinarySearchImageQuality(response.OutputImage, response.OutputStream, encoderParameters, ref saveAttempts, 1, 100);
			else
				response.OutputImage.Save(response.OutputStream, _encoder, encoderParameters);
		}

		void BinarySearchImageQuality(Bitmap inputImage, Stream outputStream, EncoderParameters encoderParams, ref int saveAttempts, long lowerRange, long upperRange)
		{
			long targetSize = _quality;
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