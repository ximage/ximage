using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Stretch : IFilter
	{
		public string Documentation
		{
			get { return "Stretch crop."; }
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.CropBox = new Rectangle(Point.Empty, response.InputImage.Size);
		}
	}
}