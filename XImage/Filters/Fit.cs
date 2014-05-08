using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = @"The image will be resized proportionally until it fits inside the canvas.  
							This may result in padding on the top/bottom or left/right edges.  
							Upscaling the original image is not performed unless an '!' 
							is appended to the end, e.g. 'fill!'")]
	public class Fit : IForcibleFilter
	{
		public bool Force { get; set; } // Implies "allow upscaling."

		[Example(QueryString = "?w=100&h=100&f=fit")]
		public Fit()
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

			var canvasSize = response.CropBox.Size;

			// Both the w & h are locked...
			if (request.ForceWidth && request.ForceHeight)
			{
				// ...so go ahead and set the canvasSize.
				canvasSize = new Size(request.Width.Value, request.Height.Value);
			}
			// The w is locked but h is not...
			else if (request.ForceWidth && !request.ForceHeight)
			{
				// ...so go ahead and set the canvas width (keeping the height proportional for now).
				canvasSize = canvasSize.ScaleToWidth(request.Width.Value);

				// If an h is specified (but not locked, remember), only adjust the canvas if it needs to be smaller.
				if (request.Height != null && request.Height < canvasSize.Height)
					canvasSize.Height = request.Height.Value;
			}
			// The w is not locked but h is...
			else if (!request.ForceWidth && request.ForceHeight)
			{
				// ...so go ahead and set the canvas height (keeping the width proportional for now).
				canvasSize = canvasSize.ScaleToHeight(request.Height.Value);

				// If a w is specified (but not locked, remember), only adjust the canvas if it needs to be smaller.
				if (request.Width != null && request.Width < canvasSize.Width)
					canvasSize.Width = request.Width.Value;
			}
			// Neither w nor h are locked...
			else if (!request.ForceWidth && !request.ForceHeight)
			{
				// When either w or h are specified, that means the resulting canvas can't go beyond that.
				// If w or h are unspecified, MAX_SIZE is implied.
				var maxAllowedWidth = request.Width ?? XImageRequest.MAX_SIZE;
				var maxAllowedHeight = request.Height ?? XImageRequest.MAX_SIZE;

				if (Force) // In this IFilter, "Force" means "allow upscaling".
				{
					// Start by matching the widths...
					canvasSize = canvasSize.ScaleToWidth(maxAllowedWidth);
					// If the image size is larget than max-allowed, bring it back down a little.
					if (canvasSize.Height > maxAllowedHeight)
						canvasSize = canvasSize.ScaleToHeight(maxAllowedHeight);
				}
				else
				{
					// If upscaling isn't allowed, make sure the canvas doesn't get any bigger.
					maxAllowedWidth = Math.Min(maxAllowedWidth, canvasSize.Width);
					maxAllowedHeight = Math.Min(maxAllowedHeight, canvasSize.Height);

					// If the canvas is too big, scale it down some (proportionally).
					if (canvasSize.Width > maxAllowedWidth)
						canvasSize = canvasSize.ScaleToWidth(maxAllowedWidth);
					if (canvasSize.Height > maxAllowedHeight)
						canvasSize = canvasSize.ScaleToHeight(maxAllowedHeight);
				}
			}

			return canvasSize;
		}

		Size GetOutputImageSize(Size inputImageSize, Size canvasSize)
		{
			var outputImageSize = inputImageSize;

			if (Force) // In this IFilter, "Force" means "allow upscaling".
			{
				// Start by scaling the outputImage's width to equal the canvas width...
				outputImageSize = outputImageSize.ScaleToWidth(canvasSize.Width);
				// ...and if this causes the outputImages's height to be larger than the canvas height, come back a little.
				if (outputImageSize.Height > canvasSize.Height)
					outputImageSize = outputImageSize.ScaleToHeight(canvasSize.Height);
			}
			else // Upscaling isn't allowed.
			{
				// If outputImage is bigger than canvasSize, then scale it down until it fits inside.
				if (outputImageSize.Width > canvasSize.Width)
					outputImageSize = outputImageSize.ScaleToWidth(canvasSize.Width);
				if (outputImageSize.Height > canvasSize.Height)
					outputImageSize = outputImageSize.ScaleToHeight(canvasSize.Height);
			}

			return outputImageSize;
		}
	}
}