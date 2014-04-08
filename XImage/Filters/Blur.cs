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
		decimal _radius;

		public Blur() : this(50) { }

		public Blur(decimal radius)
		{
			_radius = radius;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.ApplyBlur((int)(_radius * 2M), false);
		}
	}
}