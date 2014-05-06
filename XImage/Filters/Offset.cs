using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Offsets the position of the image by a specified amount.")]
	public class Offset : IFilter
	{
		decimal _dx;
		decimal _dy;

		[Example(QueryString = "?w=100&f=offset")]
		public Offset() : this(10, 10) { }

		[Example(QueryString = "?w=100&f=offset(20,20)")]
		public Offset(decimal dx, decimal dy)
		{
			_dx = dx;
			_dy = dy;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.TranslateTransform((float)_dx, (float)_dy, MatrixOrder.Append);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}