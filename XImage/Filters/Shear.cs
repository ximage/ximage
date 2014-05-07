using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Applies a shear vector.")]
	public class Shear : IFilter
	{
		decimal _dx;
		decimal _dy;

		[Example(QueryString = "?w=100&f=shear(.1,.05),scale(.75)")]
		public Shear() : this(.1M, .2M) { }

		[Example(QueryString = "?w=100&f=shear(.1,.2)")]
		public Shear(decimal dx, decimal dy)
		{
			_dx = dx;
			_dy = dy;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.VectorTransform.Shear((float)_dx, (float)_dy, MatrixOrder.Append);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}