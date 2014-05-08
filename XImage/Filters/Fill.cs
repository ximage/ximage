using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = @"The image will be resized proportionally until it fills up the canvas.  
							This may result in clipping off the top/bottom or left/right edges.  
							Upscaling the original image is not performed unless an '!' 
							is appended to the end, e.g. 'fill!'")]
	public class Fill : IForcibleFilter
	{
		public bool Force { get; set; } // Implies "allow upscaling."

		[Example(QueryString = "?w=100&h=100&f=fill")]
		public Fill()
		{
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.CanvasSize = GetCanvasSize(request, response);

			var outputImageSize = GetOutputImageSize(response.CropBox.Size, response.CanvasSize);

			response.ContentArea = new Rectangle(
				(response.CanvasSize.Width - outputImageSize.Width) / 2,
				(response.CanvasSize.Height - outputImageSize.Height) / 2,
				outputImageSize.Width,
				outputImageSize.Height);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}

		Size GetCanvasSize(XImageRequest request, XImageResponse response)
		{
			// For starter, we know if the user requests a dimension larger than MAX_SIZE it will throw in XImageRequest.ctor.

			if (request.Width == null || request.Height == null)
				throw new ArgumentException("To use a 'fill' crop, both 'w' and 'h' are required.  If you prefer to have dimensions inferred, use 'fit' or 'fit!'.");

			var canvasSize = new Size(request.Width.Value, request.Height.Value);
			var inputImageSize = response.CropBox.Size;

			if (!Force) // In this IFilter, "Force" means "allow upscaling".
			{
				if (inputImageSize.Width < canvasSize.Width && !request.ForceWidth)
					canvasSize = canvasSize.ScaleToWidth(inputImageSize.Width);
				if (inputImageSize.Height < canvasSize.Height && !request.ForceHeight)
					canvasSize = canvasSize.ScaleToHeight(inputImageSize.Height);
			}

			// But if the dimension is locked/forced, make it so.
			if (request.ForceWidth)
				canvasSize.Width = request.Width.Value;
			if (request.ForceHeight)
				canvasSize.Height = request.Height.Value;

			return canvasSize;
		}

		Size GetOutputImageSize(Size inputImageSize, Size canvasSize)
		{
			var outputImageSize = inputImageSize;

			if (Force) // In this IFilter, "Force" means "allow upscaling".
			{
				// Start by scaling the outputImage's width to equal the canvas width...
				outputImageSize = outputImageSize.ScaleToWidth(canvasSize.Width);
				// ...and if the height doesn't fill the void yet, keep going.
				if (outputImageSize.Height < canvasSize.Height)
					outputImageSize = outputImageSize.ScaleToHeight(canvasSize.Height);
			}
			else // Upscaling isn't allowed.
			{
				// If outputImage is bigger than canvasSize, then scale it down until it fits inside...
				if (outputImageSize.Width > canvasSize.Width)
					outputImageSize = outputImageSize.ScaleToWidth(canvasSize.Width);
				// ... but if that causes the image height to shrink beyond the canvas height, then cover that gap.
				// Be careful not to go higher than the image's original height though.
				if (outputImageSize.Height < canvasSize.Height)
					outputImageSize = outputImageSize.ScaleToHeight(Math.Min(canvasSize.Height, inputImageSize.Height));
			}

			return outputImageSize;
		}
	}
}