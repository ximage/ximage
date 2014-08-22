using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	[Documentation(Text = @"A pixelation filter that creates a mosaic.")]
	public class Mosaic : IFilter
	{
		int _cellSize;

		[Example(QueryString = "?f=mosaic")]
		public Mosaic() : this(20) { }

		[Example(QueryString = "?f=mosaic(30)")]
		public Mosaic(decimal cellSize)
		{
			_cellSize = (int)cellSize;

			if (_cellSize <= 1)
				throw new ArgumentException("Cell size must be greater than 1.");
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			// Unless explicitly requested by the user, default to PNG for this filter.
			if (request.IsOutputImplicitlySet)
			{
				request.Outputs.RemoveAll(o => o.ContentType.StartsWith("image"));
				request.Outputs.Add(new Outputs.Png());
			}
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			// Golly.  This does the XImager.Rasterize twice.  Not very efficient yet.
			// Also doesn't work with ContentArea (e.g. pad).

			_cellSize = Math.Min(Math.Min(response.CanvasSize.Width, response.CanvasSize.Height), _cellSize);

			int w = response.CanvasSize.Width / _cellSize;
			int h = response.CanvasSize.Height / _cellSize;
			var thumbnail = new Bitmap(response.InputImage, w, h);

			var canvasSize = response.CanvasSize;
			var contentArea = response.ContentArea;
			var cropBox = response.CropBox;

			// Set the OutputImage and OutputGraphics objects which can be used in the PostProcess methods.
			response.OutputGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			response.OutputGraphics.SmoothingMode = SmoothingMode.HighQuality;
			response.OutputGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			//response.OutputGraphics.CompositingQuality = CompositingQuality.HighQuality;

			// Apply any vector transformations.
			response.OutputGraphics.Transform = response.VectorTransform;

			// Set the background.
			var clearColor = request.Outputs.Exists(o => o.SupportsTransparency) ? Color.Transparent : Color.White;
			response.OutputGraphics.Clear(clearColor);

			// Draw the image in the proper position with the proper image attributes (color transformations).
			response.OutputGraphics.DrawImage(
				image: thumbnail,
				destRect: new Rectangle(contentArea.X, contentArea.Y, contentArea.Width, contentArea.Height),
				srcX: 0,
				srcY: 0,
				srcWidth: thumbnail.Width,
				srcHeight: thumbnail.Height,
				srcUnit: GraphicsUnit.Pixel,
				imageAttr: response.ImageAttributes);
		}
	}
}