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
		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.CropBox = new Rectangle(Point.Empty, response.InputImage.Size);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}