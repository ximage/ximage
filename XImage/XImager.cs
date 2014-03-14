﻿using System;
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
		// Help, Width, Height, Crop, Filter, Mask, Text, Output
		public static readonly string[] XIMAGE_PARAMETERS = { "help", "w", "width", "h", "height", "c", "crop", "f", "filter", "filters", "m", "mask", "t", "text", "o", "output" };
		private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

		public static void ProcessImage(XImageRequest request, XImageResponse response)
		{
			var timestamp = _stopwatch.ElapsedTicks;

			// --- CROP ---
			request.Crop.SetSizeAndCrop(request, response);

			response.OutputGraphics.DrawImage(
				image: response.InputImage,
				destRect: new Rectangle(Point.Empty, response.OutputSize),
				srcRect: response.CropBox,
				srcUnit: GraphicsUnit.Pixel);

			bool writeAccess = request.Filters.Count > 0 || request.Mask != null;
			using (var bitmapBits = response.OutputImage.GetBitmapBits(writeAccess))
			{
				// --- FILTERS ---
				foreach (var filter in request.Filters)
					filter.ProcessImage(request, response, bitmapBits.Data);

				// --- MASK ---
				ApplyMask(request, response, bitmapBits);

				// --- METAS ---
				foreach (var meta in request.Metas)
					meta.Calculate(request, response, bitmapBits.Data);
			}

			// --- OUTPUT ---
			request.Output.FormatImage(request, response);

			response.Properties.Add("X-Image-Processing-Time", string.Format("{0:N2}ms", 1000D * (double)(_stopwatch.ElapsedTicks - timestamp) / (double)Stopwatch.Frequency));
		}

		private static void ApplyMask(XImageRequest request, XImageResponse response, BitmapBits bitmapBits)
		{
			if (request.Mask != null)
			{
				using (var maskBitmap = new Bitmap(response.OutputSize.Width, response.OutputSize.Height))
				{
					using (var maskGraphics = Graphics.FromImage(maskBitmap))
					{
						maskGraphics.SmoothingMode = SmoothingMode.HighQuality;

						request.Mask.DrawMask(request, response, maskGraphics);

						using (var maskData = maskBitmap.GetBitmapBits(false))
						{
							if (request.Output.ContentType.Contains("jpeg") || request.Output.ContentType.Contains("gif"))
								bitmapBits.Data.BlendLayer(maskData.Data, BlendingModes.OpaqueMask);
							else
								bitmapBits.Data.BlendLayer(maskData.Data, BlendingModes.Mask);
						}
					}
				}
			}
		}
	}
}