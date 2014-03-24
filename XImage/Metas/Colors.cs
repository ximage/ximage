using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Meta
{
	public class Colors : IMeta
	{
		public string Documentation
		{
			get { return "Calculates several different color attributes such as accent, average, base, dominant and palette."; }
		}

		public void Calculate(XImageRequest request, XImageResponse response, byte[] data)
		{
			var byteCount = data.Length;
			var pixelCount = byteCount / (Bitmap.GetPixelFormatSize(response.OutputImage.PixelFormat) / 8);

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
	}
}