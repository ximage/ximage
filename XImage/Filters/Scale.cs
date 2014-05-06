using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Scales the image (the canvas stays the same).")]
	public class Scale : IFilter
	{
		float _scaleX;
		float _scaleY;

		[Example(QueryString = "?w=100&f=scale")]
		public Scale() : this(2) { }

		[Example(QueryString = "?w=100&f=scale(.5)")]
		public Scale(decimal scale) : this(scale, scale) { }

		[Example(QueryString = "?w=100&f=scale(1,2)")]
		public Scale(decimal scaleX, decimal scaleY)
		{
			_scaleX = (float)scaleX;
			_scaleY = (float)scaleY;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.ScaleTransform(_scaleX, _scaleY, MatrixOrder.Append);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}