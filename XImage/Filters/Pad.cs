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

		public Pad() : this(10) { }

		public Pad(int padding) : this(padding, padding, padding, padding) { }

		// TODO: Add ChangeSize method to IFilter.
		// The ctors below are private because of a bug.  
		// Stretching and incorrect offsets may occur if the padding values aren't all equal.

		private Pad(int topBottom, int leftRight) : this(topBottom, leftRight, topBottom, leftRight) { }

		private Pad(int top, int leftRight, int bottom) : this(top, leftRight, bottom, leftRight) { }

		private Pad(int top, int right, int bottom, int left)
		{
			_top = top;
			_right = right;
			_bottom = bottom;
			_left = left;

			if (_top < 0 || _right < 0 || _bottom < 0 || _left < 0)
				throw new ArgumentException("Padding must be a non-negative number.");
		}
		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.ContentArea = new Rectangle(
				response.ContentArea.X + _left,
				response.ContentArea.Y + _top,
				response.ContentArea.Width - _left - _right,
				response.ContentArea.Height - _top - _bottom);
		}
	}
}