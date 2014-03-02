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

			var imageSize = response.InputImage.Size;
			var outputSize = new Size(request.Width.Value, request.Height.Value);
			var cropBox = new Rectangle(Point.Empty, imageSize);
			var targetIsWiderThanOutput = (float)request.Width / (float)request.Height < (float)imageSize.Width / (float)imageSize.Height;

			// Hmmm, we're not allowed to make the image bigger than its original size.
			if (!request.AllowUpscaling && (outputSize.Width > imageSize.Width || outputSize.Height > imageSize.Height))
			{
				float scale = targetIsWiderThanOutput ? (float)imageSize.Height / (float)outputSize.Height : (float)imageSize.Width / (float)outputSize.Width;
				if (targetIsWiderThanOutput)
				{
					// The height can be the same as the original, but scale the width proportionally.
					outputSize.Width = Convert.ToInt32(outputSize.Width * scale);
					outputSize.Height = imageSize.Height;
				}
				else
				{
					// The width can be the same as the original, but scale the height proportionally.
					outputSize.Width = imageSize.Width;
					outputSize.Height = Convert.ToInt32(request.Height * scale);
				}
			}

			var outputAspectRatio = (float)outputSize.Width / (float)outputSize.Height;
			if (targetIsWiderThanOutput)
			{
				// Don't change the height, but shrink the width to be proportional to the output size and scoot it over some.
				cropBox.Width = Convert.ToInt32((float)cropBox.Height * outputAspectRatio);
				cropBox.X = (imageSize.Width - cropBox.Width) / 2;
			}
			else
			{
				// Don't change the width, but shrink the height to be proportional to the output size and scoot it down some.
				cropBox.Height = Convert.ToInt32((float)cropBox.Width / outputAspectRatio);
				cropBox.Y = (imageSize.Height - cropBox.Height) / 2;
			}

			// Set the values.
			response.OutputSize = outputSize;
			response.CropBox = cropBox;
		}
	}
}