using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Blur : IFilter
	{
		int _radius;

		public Blur() : this(50) { }

		public Blur(int radius)
		{
			_radius = radius;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.ApplyBlur(_radius * 2, false);
		}
	}
}