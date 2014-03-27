using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Offset : IFilter
	{
		decimal _dx;
		decimal _dy;

		public string Documentation
		{
			get { return "Offsets the position by x and y."; }
		}

		public Offset() : this(10, 10) { }

		public Offset(int dx, int dy)
		{
			_dx = dx;
			_dy = dy;
		}

		public Offset(decimal scaleX, decimal scaleY)
		{
			_dx = scaleX;
			_dy = scaleY;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.TranslateTransform((float)_dx, (float)_dy, MatrixOrder.Append);
		}
	}
}