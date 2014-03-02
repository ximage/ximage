using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Masks
{
	public class Circle : IMask
	{
		public string Documentation
		{
			get { return "Applies a mask in the shape of a circle with a diameter of either w or h, whichever is shortest."; }
		}

		public void DrawMask(XImageRequest request, XImageResponse response, Graphics mask)
		{
			var size = response.OutputSize;

			var origin = Point.Empty;
			if (size.Width < size.Height)
				origin.Y = (size.Width - size.Height) / 2;
			else
				origin.X = (size.Width - size.Height) / 2;

			var d = Math.Min(size.Width - 1, size.Height - 1);
			size = new Size(d, d);

			mask.FillEllipse(Brushes.White, new Rectangle(origin, size));
		}
	}
}