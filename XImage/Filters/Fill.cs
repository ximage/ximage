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
							The image will not be resized larger than its original dimensions 
							unless you append ! to the end, e.g. 'fill!'")]
	public class Fill : IFilter
	{
		[Example(QueryString = "?w=100&h=100&f=fill")]
		public Fill()
		{
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			var inputSize = response.InputImage.Size;
			var canvasSize = response.CanvasSize;
			var cropDimensions = canvasSize;
			var cropBox = response.CropBox;

			if (inputSize.GetAspectRatio() > canvasSize.GetAspectRatio())
			{
				cropDimensions = cropDimensions.ScaleToHeight(inputSize.Height);
				cropBox = new Rectangle(Point.Empty, cropDimensions);
				cropBox.X += (inputSize.Width - cropDimensions.Width) / 2;
			}
			else
			{
				cropDimensions = cropDimensions.ScaleToWidth(inputSize.Width);
				cropBox = new Rectangle(Point.Empty, cropDimensions);
				cropBox.Y += (inputSize.Height - cropDimensions.Height) / 2;
			}

			response.CropBox = cropBox;
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}