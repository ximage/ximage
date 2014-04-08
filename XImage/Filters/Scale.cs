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
		float _scaleX;
		float _scaleY;

		public Scale() : this(2) { }

		public Scale(decimal scale) : this(scale, scale) { }

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