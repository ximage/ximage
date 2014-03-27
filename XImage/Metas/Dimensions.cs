using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Meta
{
	public class Dimensions : IMeta
	{
		public string Documentation
		{
			get { return "Calculates several different size attributes widht and height for the original image and output image."; }
		}

		public void Calculate(XImageRequest request, XImageResponse response, byte[] data)
		{
			response.Properties["X-Image-Original-Format"] = "image/" + new ImageFormatConverter().ConvertToString(response.InputImage.RawFormat).ToLower();
			response.Properties["X-Image-Original-Width"] = response.InputImage.Width.ToString();
			response.Properties["X-Image-Original-Height"] = response.InputImage.Height.ToString();
			response.Properties["X-Image-Width"] = response.CanvasSize.Width.ToString();
			response.Properties["X-Image-Height"] = response.CanvasSize.Height.ToString();
		}
	}
}