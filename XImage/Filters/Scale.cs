using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Scale : IFilter
	{
		decimal _scaleX;
		decimal _scaleY;

		public Scale() : this(2) { }

		public Scale(int scale) : this(scale, scale) { }

		public Scale(decimal scale) : this(scale, scale) { }

		public Scale(int scaleX, int scaleY)
		{
			_scaleX = scaleX;
			_scaleY = scaleY;
		}

		public Scale(decimal scaleX, decimal scaleY)
		{
			_scaleX = scaleX;
			_scaleY = scaleY;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.ScaleTransform((float)_scaleX, (float)_scaleY, MatrixOrder.Append);
		}
	}
}