using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Fit : IFilter
	{
		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			var inputSize = response.InputImage.Size;
			var canvasSize = response.CanvasSize;
			var cropDimensions = canvasSize;
			var cropBox = response.CropBox;

			if (inputSize.GetAspectRatio() < canvasSize.GetAspectRatio())
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