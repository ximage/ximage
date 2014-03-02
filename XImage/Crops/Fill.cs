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
			if (request.Width == null || request.Height == null)
				throw new ArgumentException("Using a fill crop requires that both w and h be specified.");

			response.OutputSize = GetOutputSize(request, response);
			response.CropBox = GetCropBox(request, response);
		}

		static Size GetOutputSize(XImageRequest request, XImageResponse response)
		{
			var outputSize = new Size(request.Width.Value, request.Height.Value);
			var imageSize = response.InputImage.Size;

			if (!request.AllowUpscaling && (outputSize.Width > imageSize.Width || outputSize.Height > imageSize.Height))
			{
				var targetIsWiderThanOutput = (float)request.Width / (float)request.Height < (float)imageSize.Width / (float)imageSize.Height;
				float scale = targetIsWiderThanOutput ? (float)response.InputImage.Height / (float)outputSize.Height : (float)response.InputImage.Width / (float)outputSize.Width;
				if (targetIsWiderThanOutput)
				{
					outputSize.Width = Convert.ToInt32(outputSize.Width * scale);
					outputSize.Height = imageSize.Height;
				}
				else
				{
					outputSize.Width = imageSize.Width;
					outputSize.Height = Convert.ToInt32(request.Height * scale);
				}
			}

			return outputSize;
		}

		static Rectangle GetCropBox(XImageRequest request, XImageResponse response)
		{
			var imageSize = response.InputImage.Size;
			var cropBox = new Rectangle(Point.Empty, imageSize);
			var outputAspectRatio = (float)response.OutputSize.Width / (float)response.OutputSize.Height;
			var targetIsWiderThanOutput = (float)request.Width / (float)request.Height < (float)imageSize.Width / (float)imageSize.Height;
			if (targetIsWiderThanOutput)
			{
				cropBox.Width = Convert.ToInt32((float)cropBox.Height * outputAspectRatio);
				cropBox.X = (imageSize.Width - cropBox.Width) / 2;
			}
			else
			{
				cropBox.Height = Convert.ToInt32((float)cropBox.Width / outputAspectRatio);
				cropBox.Y = (imageSize.Height - cropBox.Height) / 2;
			}
			return cropBox;
		}
	}
}