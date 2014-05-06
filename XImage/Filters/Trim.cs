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
			bool allowClipping = request.Width != null && request.Height != null && request.Filters.OfType<Fill>().Any();

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

			response.CanvasSize = GetOutputSize(request, response.CropBox.Size, allowClipping);
			response.ContentArea = new Rectangle(Point.Empty, response.CanvasSize);

			var cropBox = response.CropBox;

			// Readjust the cropbox's width or height so there is no stretching.
			if (request.Width != null && request.Height != null)
			{
				var targetIsWiderThanOutput = (float)cropBox.Width / (float)cropBox.Height > (float)response.CanvasSize.Width / (float)response.CanvasSize.Height;
				if (targetIsWiderThanOutput)
				{
					if (allowClipping)
					{
						var size = response.CanvasSize.ScaleToHeight(cropBox.Height);
						cropBox.Inflate(Convert.ToInt32((float)(size.Width - cropBox.Width) / 2F), 0);
					}
					else
					{
						var size = response.CanvasSize.ScaleToWidth(cropBox.Width);
						cropBox.Inflate(0, Convert.ToInt32((float)(size.Height - cropBox.Height) / 2F));
					}
				}
				else
				{
					if (allowClipping)
					{
						var size = response.CanvasSize.ScaleToWidth(cropBox.Width);
						cropBox.Inflate(0, Convert.ToInt32((float)(size.Height - cropBox.Height) / 2F));
					}
					else
					{
						var size = response.CanvasSize.ScaleToHeight(cropBox.Height);
						cropBox.Inflate(Convert.ToInt32((float)(size.Width - cropBox.Width) / 2F), 0);
					}
				}
			}

			response.CropBox = cropBox;
		}

		Size GetOutputSize(XImageRequest request, Size original, bool allowClipping)
		{
			if (request.Width == null && request.Height == null)
				return new Size(original.Width, original.Height);

			var w = request.Width;
			var h = request.Height;

			// In the event that just one dimension was specified, i.e. just w or just h,
			// then extrapolate the missing dimension.
			w = w ?? Convert.ToInt32(original.Width * h / (float)original.Height);
			h = h ?? Convert.ToInt32(original.Height * w / (float)original.Width);

			var size = new Size(w.Value, h.Value);

			// If upscaling is not allowed (the default), cap those values.
			if (!request.AllowUpscaling && (size.Width > original.Width || size.Height > original.Height))
			{
				var targetIsWiderThanOutput = (float)original.Width / (float)original.Height > (float)size.Width / (float)size.Height;
				if (targetIsWiderThanOutput)
				{
					if (allowClipping)
					{
						if (original.Height <= size.Height)
							size = size.ScaleToHeight(original.Height);
					}
					else
					{
						if (original.Width <= size.Width)
							size = size.ScaleToWidth(original.Width);
					}
				}
				else
				{
					if (allowClipping)
					{
						if (original.Width <= size.Width)
							size = size.ScaleToWidth(original.Width);
					}
					else
					{
						if (original.Height <= size.Height)
							size = size.ScaleToHeight(original.Height);
					}
				}
			}

			return size;
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}