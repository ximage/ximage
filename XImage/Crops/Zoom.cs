using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Crops
{
	public class Zoom : ICrop
	{
		public string Documentation
		{
			get { return "It will be resized proportionally until it fits completely within the w x h boundaries.  This will likely result in void space on the left/right or top/bottom which will be filled with the supplied color.  Must be a 6 or 8 digit lower case hex, don't use #."; }
		}

		public Color Color { get; set; }

		public Zoom()
		{
			// Fully transparent white.
			Color = Color.FromArgb(0, Color.White);
		}

		public Zoom(int color)
			: this(color.ToString())
		{
		}

		public Zoom(string color)
		{
			try
			{
				try
				{
					Color = (Color)new ColorConverter().ConvertFromString("#" + color);
				}
				catch
				{
					Color = (Color)new ColorConverter().ConvertFromString(color);
				}
			}
			catch
			{
				throw new ArgumentException(string.Format("Invalid color {0} for the fit crop.", color));
			}
		}

		public void SetSizeAndCrop(XImageRequest request, XImageResponse response)
		{
			if (request.Width == null || request.Height == null)
				throw new ArgumentException("Using a fit crop requires that both w and h be specified.");

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
				// Don't change the width, but grow the height to be proportional to the output size and scoot it up some.
				cropBox.Height = Convert.ToInt32((float)cropBox.Width / outputAspectRatio);
				cropBox.Y = (imageSize.Height - cropBox.Height) / 2;
			}
			else
			{
				// Don't change the height, but grow the width to be proportional to the output size and scoot it left some.
				cropBox.Width = Convert.ToInt32((float)cropBox.Height * outputAspectRatio);
				cropBox.X = (imageSize.Width - cropBox.Width) / 2;
			}

			// Set the values.
			response.OutputSize = outputSize;
			response.CropBox = cropBox;

			// Quirk in GDI where it treats fully transparent as black instead of the RGB component.
			if (request.Output.ContentType == "image/jpg")
				Color = Color.FromArgb(255, Color);

			response.OutputGraphics.Clear(Color);
		}
	}
}