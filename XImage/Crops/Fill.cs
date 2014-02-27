using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Crops
{
	public class Fill : ICrop
	{
		public string Documentation
		{
			get { return "The image will be resized proportionally until it completely fills the w x h boundaries.  This will likely result in cropping off either top/bottom edges or left/right edges but the resulting dimensions will be exactly w x h."; }
		}

		public void SetSizeAndCrop(XImageRequest request, XImageResponse response)
		{
			var targetImageSize = GetTargetImageSize(request, response.InputImage.Size);
			var targetIsWiderThanOutput = (float)request.Width.Value / (float)request.Height.Value < (float)targetImageSize.Width / (float)targetImageSize.Height;
			response.OutputSize = GetOutputDimensions(request, targetImageSize, targetIsWiderThanOutput);
			var origin = GetImageOrigin(request, targetImageSize, targetIsWiderThanOutput, response.OutputSize);
			response.CropBox = new Rectangle(origin, response.OutputSize);
		}

		static Size GetTargetImageSize(XImageRequest request, Size original)
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

			// Resize it such that the image fills up the entire crop's bounding box.  This means that 
			// the resulting Width or Height will be exactly as specified but the image may be cropped.
			if ((float)w / (float)h > (float)original.Width / (float)original.Height)
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

		static Size GetOutputDimensions(XImageRequest request, Size targetImageSize, bool targetIsWiderThanOutput)
		{
			Size outputDimensions = targetImageSize;

			outputDimensions.Width = request.Width.Value;
			outputDimensions.Height = request.Height.Value;

			if (!request.AllowUpscaling)
			{
				float scale = targetIsWiderThanOutput ? (float)targetImageSize.Height / (float)outputDimensions.Height : (float)targetImageSize.Width / (float)outputDimensions.Width;
				outputDimensions.Height = Convert.ToInt32(request.Height * scale);
				outputDimensions.Width = Convert.ToInt32(request.Width * scale);
			}

			return outputDimensions;
		}

		static Point GetImageOrigin(XImageRequest request, Size targetImageSize, bool targetIsWiderThanOutput, Size outputDimensions)
		{
			Point origin = Point.Empty;

			if (targetIsWiderThanOutput)
				origin.X -= (targetImageSize.Width - outputDimensions.Width) / 2;
			else
				origin.Y -= (targetImageSize.Height - outputDimensions.Height) / 2;

			return origin;
		}
	}
}