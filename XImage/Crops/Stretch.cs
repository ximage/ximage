using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Crops
{
	public class Stretch : ICrop
	{
		public string Documentation
		{
			get { return "Each edge will be resized disproportionally until it reaches its w x h boundaries.  Results are exactly w x h and no edges are cropped but the image will appear distorted."; }
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
			var w = request.Width;
			if (w != null && !request.AllowUpscaling)
				w = Math.Min(original.Width, w.Value);
			var h = request.Height;
			if (h != null && !request.AllowUpscaling)
				h = Math.Min(original.Height, h.Value);

			// In the event that just one dimension was specified, i.e. just w or just h,
			// then extrapolate the missing dimension.  This should only occur when fit is null.
			scaled.Width = w ?? Convert.ToInt32(original.Width * h.Value / (float)original.Height);
			scaled.Height = h ?? Convert.ToInt32(original.Height * w.Value / (float)original.Width);

			return scaled;
		}
	}
}