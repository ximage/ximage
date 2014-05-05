using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "neato mosquito")]
	public class Rotate : IFilter
	{
		float _angle;

		public Rotate() : this(180) { }

		public Rotate(decimal angle)
		{
			_angle = (float)angle;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.RotateTransform(_angle, MatrixOrder.Append);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}