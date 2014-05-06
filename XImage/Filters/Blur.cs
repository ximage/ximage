using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Applies a gaussian blur.")]
	public class Blur : IFilter
	{
		decimal _radius;

		[Example(QueryString = "?w=100&f=blur")]
		public Blur() : this(50) { }

		[Example(QueryString = "?w=100&f=blur(5)")]
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