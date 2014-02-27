using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Crops
{
	public class None : ICrop
	{
		public string Documentation
		{
			get { return "No parts of the image will be cropped.  It will be resized proportionally until it fits completely within the w x h boundaries.  This will likely result in either width or height being smaller than the requested value."; }
		}

		public void SetSizeAndCrop(XImageRequest request, XImageResponse response)
		{
			response.CropBox = new Rectangle(Point.Empty, response.InputImage.Size);
			response.OutputSize = GetOutputSize(request, response.InputImage.Size);
		}

		static Size GetOutputSize(XImageRequest request, Size original)
		{
			if (request.Width == null && request.Height == null)
				return original;

			Size scaled = original;

			// If upscaling is not allowed (the default), cap those values.
			var parametersWidth = request.Width;
			if (parametersWidth != null && !request.AllowUpscaling)
				parametersWidth = Math.Min(original.Width, parametersWidth.Value);
			var parametersHeight = request.Height;
			if (parametersHeight != null && !request.AllowUpscaling)
				parametersHeight = Math.Min(original.Height, parametersHeight.Value);

			// In the event that just one dimension was specified, i.e. just w or just h,
			// then extrapolate the missing dimension.  This should only occur when fit is null.
			int w = parametersWidth ?? Convert.ToInt32(original.Width * parametersHeight.Value / (float)original.Height);
			int h = parametersHeight ?? Convert.ToInt32(original.Height * parametersWidth.Value / (float)original.Width);

			// Resize it such that the image fits within the crop's bounding box.  This does mean that 
			// the resulting Width or Height might end up smaller than the requested value.
			if ((float)w / (float)h < (float)original.Width / (float)original.Height)
			{
				scaled.Width = w;
				scaled.Height = Convert.ToInt32(original.Height * w / (float)original.Width);
			}
			else
			{
				scaled.Height = h;
				scaled.Width = Convert.ToInt32(original.Width * h / (float)original.Height);
			}
			return scaled;
		}
	}
}