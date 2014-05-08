using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = @"Trims the image to fit tightly around the image content while ignoring any whitespace.
							Optionally indicate the whitespace threshold.")]
	public class Trim : IFilter
	{
		int _threshold;

		[Example(QueryString = "?w=100&h=100&f=trim")]
		public Trim() : this(25) { }

		[Example(QueryString = "?w=t00&h=100&f=trim(50)")]
		public Trim(decimal threshold)
		{
			_threshold = (int)threshold;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			using (var bitmapBits = response.InputImage.GetBitmapBits())
			{
				int bytesPerPixel = Bitmap.GetPixelFormatSize(response.InputImage.PixelFormat) / 8;
				int i = 0, halfWidth = response.InputImage.Width / 2, halfHeight = response.InputImage.Height / 2;
				int r, g, b, r0, g0, b0;
				int left = halfWidth, right = halfWidth, top = halfHeight, bottom = halfHeight;
				var data = bitmapBits.Data;

				for (int y = 0; y < response.InputImage.Height; y++)
				{
					i = y * bitmapBits.BitmapData.Stride;
					r0 = 255 - _threshold;
					g0 = 255 - _threshold;
					b0 = 255 - _threshold;

					for (int x = 0; x < halfWidth; x++)
					{
						// TODO: This does not account for other goodness like RGB565.
						r = data[i + 2];
						g = data[i + 1];
						b = data[i];

						if (r0 - r > _threshold || g0 - g > _threshold || b0 - b > _threshold)
						{
							left = Math.Min(left, x);
							top = Math.Min(top, y);
							bottom = Math.Max(bottom, y);
							break;
						}

						r0 = r;
						g0 = g;
						b0 = b;
						i += bytesPerPixel;
					}

					i = y * bitmapBits.BitmapData.Stride + response.InputImage.Width * bytesPerPixel - bytesPerPixel;
					r0 = 255 - _threshold;
					g0 = 255 - _threshold;
					b0 = 255 - _threshold;

					for (int x = response.InputImage.Width; x > halfWidth; x--)
					{
						// TODO: This does not account for other goodness like RGB565.
						r = data[i + 2];
						g = data[i + 1];
						b = data[i];

						if (r0 - r > _threshold || g0 - g > _threshold || b0 - b > _threshold)
						{
							right = Math.Max(right, x);
							top = Math.Min(top, y);
							bottom = Math.Max(bottom, y);
							break;
						}

						r0 = r;
						g0 = g;
						b0 = b;
						i -= bytesPerPixel;
					}
				}

				response.CropBox = new Rectangle(left, top, right - left, bottom - top);
			}

			// Re-run the default crop.  This can be overridden (e.g. by fill) by 
			// adding additional filters to the querystring if desired.
			new Fit().PreProcess(request, response);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}