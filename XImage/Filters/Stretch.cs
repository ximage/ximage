using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = @"Each edge will be resized disproportionally until it reaches its w x h boundaries.
							Results are exactly w x h and no edges are padded or clipped.  
							The image may appear distorted.")]
	public class Stretch : IForcibleFilter
	{
		public bool Force { get; set; } // Implies "allow upscaling."

		[Example(QueryString = "?w=300&h=100&f=stretch")]
		public Stretch()
		{
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			if (request.Width == null || request.Height == null)
				throw new ArgumentException("To use a 'stretch' crop, both 'w' and 'h' are required.  If you prefer to have dimensions inferred, use 'fit' or 'fit!'.");

			var inputImageSize = response.CropBox.Size;
			var outputImageSize = new Size(request.Width.Value, request.Height.Value);

			// Stretch breaks the rules a little.  If 'w' or 'h' are forced/locked, it implies allow upscaling.
			// It's better than padding or clipping the edges, the whole goal of stretch is to go from edge to edge.

			// In this IFilter, "Force" means "allow upscaling".
			if (!Force && !request.ForceWidth && !request.ForceHeight)
			{
				// Shring the image proportionally, if needed.
				if (outputImageSize.Width < inputImageSize.Width)
					outputImageSize = outputImageSize.ScaleToWidth(inputImageSize.Width);
				if (outputImageSize.Height < inputImageSize.Height)
					outputImageSize = outputImageSize.ScaleToWidth(inputImageSize.Height);
			}

			// And the canvas size and image size should ALWAYS be the same.
			response.CanvasSize = outputImageSize;

			response.ContentArea = new Rectangle(Point.Empty, outputImageSize);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}