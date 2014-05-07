using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Rotates the image.")]
	public class Rotate : IFilter
	{
		float _angle;

		[Example(QueryString = "?w=100&f=rotate")]
		public Rotate() : this(180) { }

		[Example(QueryString = "?w=100&f=rotate(45)")]
		public Rotate(decimal angle)
		{
			_angle = (float)angle;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			int x = response.CanvasSize.Width / 2, y = response.CanvasSize.Height / 2;
			response.OutputGraphics.TranslateTransform(-x, -y, MatrixOrder.Append);
			response.OutputGraphics.RotateTransform(_angle, MatrixOrder.Append);
			response.OutputGraphics.TranslateTransform(x, y, MatrixOrder.Append);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}