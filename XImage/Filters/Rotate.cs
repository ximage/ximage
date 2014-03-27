using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Rotate : IFilter
	{
		int _angle;

		public string Documentation
		{
			get { return "Rotate."; }
		}

		public Rotate() : this(180) { }

		public Rotate(int angle)
		{
			_angle = angle;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.RotateTransform(_angle, MatrixOrder.Append);
		}
	}
}