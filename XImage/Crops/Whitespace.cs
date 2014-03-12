using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Crops
{
	public class Whitespace : ICrop
	{
		const byte WHITE_ENOUGH = 252;
		int _top;
		int _right;
		int _bottom;
		int _left;

		public string Documentation
		{
			get { return "Removes any padding added to images."; }
		}

		public Whitespace() : this(0) { }

		public Whitespace(int padding) : this(padding, padding, padding, padding) { }

		public Whitespace(int topBottom, int leftRight) : this(topBottom, leftRight, topBottom, leftRight) { }

		public Whitespace(int top, int leftRight, int bottom) : this(top, leftRight, bottom, leftRight) { }

		public Whitespace(int top, int right, int bottom, int left)
		{
			_top = top;
			_right = right;
			_bottom = bottom;
			_left = left;

			if (_top < 0 || _right < 0 || _bottom < 0 || _left < 0)
				throw new ArgumentException("Padding must be a non-negative number.");
		}

		public void SetSizeAndCrop(XImageRequest request, XImageResponse response)
		{
			using (var bitmapBits = response.InputImage.GetBitmapBits())
			{
				int bytesPerPixel = Bitmap.GetPixelFormatSize(response.InputImage.PixelFormat) / 8;
				int i = 0, halfWidth = response.InputImage.Width / 2, halfHeight = response.InputImage.Height / 2;
				byte r, g, b;
				int left = halfWidth, right = halfWidth, top = halfHeight, bottom = halfHeight;
				var data = bitmapBits.Data;

				for (int y = 0; y < response.InputImage.Height; y++)
				{
					i = y * bitmapBits.BitmapData.Stride;

					for (int x = 0; x < halfWidth; x++)
					{
						// TODO: This does not account for other goodness like RGB565.
						r = data[i + 2];
						g = data[i + 1];
						b = data[i];

						if (r < WHITE_ENOUGH || g < WHITE_ENOUGH || b < WHITE_ENOUGH)
						{
							left = Math.Min(left, x);
							top = Math.Min(top, y);
							bottom = Math.Max(bottom, y);
							break;
						}

						i += bytesPerPixel;
					}

					i = y * bitmapBits.BitmapData.Stride + response.InputImage.Width * bytesPerPixel - bytesPerPixel;

					for (int x = response.InputImage.Width; x > halfWidth; x--)
					{
						// TODO: This does not account for other goodness like RGB565.
						r = data[i + 2];
						g = data[i + 1];
						b = data[i];

						if (r < WHITE_ENOUGH || g < WHITE_ENOUGH || b < WHITE_ENOUGH)
						{
							right = Math.Max(right, x);
							top = Math.Min(top, y);
							bottom = Math.Max(bottom, y);
							break;
						}

						i -= bytesPerPixel;
					}
				}

				response.CropBox = new Rectangle(left, top, right - left, bottom - top);
			}

			response.OutputSize = GetOutputSize(request, response.CropBox.Size);

			var cropBox = response.CropBox;

			// Readjust the cropbox's width or height so there is no stretching.
			if (request.Width != null && request.Height != null)
			{
				var targetIsWiderThanOutput = (float)cropBox.Width / (float)cropBox.Height > (float)response.OutputSize.Width / (float)response.OutputSize.Height;
				if (targetIsWiderThanOutput)
				{
					if (request.AllowClipping)
					{
						var size = response.OutputSize.ScaleToHeight(cropBox.Height);
						cropBox.Inflate(Convert.ToInt32((float)(size.Width - cropBox.Width) / 2F), 0);
					}
					else
					{
						var size = response.OutputSize.ScaleToWidth(cropBox.Width);
						cropBox.Inflate(0, Convert.ToInt32((float)(size.Height - cropBox.Height) / 2F));
					}
				}
				else
				{
					if (request.AllowClipping)
					{
						var size = response.OutputSize.ScaleToWidth(cropBox.Width);
						cropBox.Inflate(0, Convert.ToInt32((float)(size.Height - cropBox.Height) / 2F));
					}
					else
					{
						var size = response.OutputSize.ScaleToHeight(cropBox.Height);
						cropBox.Inflate(Convert.ToInt32((float)(size.Width - cropBox.Width) / 2F), 0);
					}
				}
			}

			// Account for padding.
			if (_top > 0 || _right > 0 || _bottom > 0 || _left > 0)
			{
				var scaleX = (float)(response.OutputSize.Width - _left - _right) / (float)response.OutputSize.Width;
				var scaleY = (float)(response.OutputSize.Height - _top - _bottom) / (float)response.OutputSize.Height;
				var scale = Math.Min(scaleX, scaleY);
				if (scale <= 0) // negative because padding was more than the image size
					scale = .01F;

				cropBox = new Rectangle(
					cropBox.X,
					cropBox.Y,
					Convert.ToInt32((float)cropBox.Width / scale),
					Convert.ToInt32((float)cropBox.Height / scale));
				cropBox.X -= Convert.ToInt32((float)_left / (float)response.OutputSize.Width * (float)cropBox.Width);
				cropBox.Y -= Convert.ToInt32((float)_top / (float)response.OutputSize.Height * (float)cropBox.Height);
			}

			response.CropBox = cropBox;

			response.OutputGraphics.Clear(Color.White);
		}

		Size GetOutputSize(XImageRequest request, Size original)
		{
			if (request.Width == null && request.Height == null)
				return new Size(original.Width + _left + _right, original.Height + _top + _bottom);

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
					if (request.AllowClipping)
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
					if (request.AllowClipping)
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
	}
}