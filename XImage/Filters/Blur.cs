using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class Blur : IFilter
	{
		int _radius;

		public string Documentation
		{
			get { return "Applies a gaussian blur."; }
		}

		public Blur() : this(50) { }

		public Blur(int radius)
		{
			_radius = radius;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response, byte[] data)
		{
			response.OutputImage.ApplyBlur(_radius, false);
		}
	}
}