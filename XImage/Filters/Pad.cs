using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Pad : IFilter
	{
		int _top;
		int _right;
		int _bottom;
		int _left;

		// TODO: Bug, causes disproportionate resizing when image isn't 1:1 aspect ratio.  Try a padding of 200 on ping.jpg.
		public Pad() : this(10) { }

		public Pad(decimal padding) : this(padding, padding, padding, padding) { }

		// TODO: Add ChangeSize method to IFilter.
		// The ctors below are private because of a bug.  
		// Stretching and incorrect offsets may occur if the padding values aren't all equal.

		private Pad(decimal topBottom, decimal leftRight) : this(topBottom, leftRight, topBottom, leftRight) { }

		private Pad(decimal top, decimal leftRight, decimal bottom) : this(top, leftRight, bottom, leftRight) { }

		private Pad(decimal top, decimal right, decimal bottom, decimal left)
		{
			_top = (int)top;
			_right = (int)right;
			_bottom = (int)bottom;
			_left = (int)left;

			if (_top < 0 || _right < 0 || _bottom < 0 || _left < 0)
				throw new ArgumentException("Padding must be a non-negative number.");
		}
		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.ContentArea = new Rectangle(
				response.ContentArea.X + _left,
				response.ContentArea.Y + _top,
				response.ContentArea.Width - _left - _right,
				response.ContentArea.Height - _top - _bottom);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}