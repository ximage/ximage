using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;

namespace XImage.Masks
{
	public class BorderRadius : IMask
	{
		int _radius = 10;

		public string Documentation
		{
			get { return "Rounds the corners with by a specified radius."; }
		}

		public BorderRadius()
		{
		}

		public BorderRadius(int radius)
		{
			_radius = radius;
		}

		public void DrawMask(XImageRequest request, XImageResponse response, Graphics mask)
		{
			int w = response.OutputSize.Width - 1, h = response.OutputSize.Height - 1;
			var diameter = Math.Min(_radius * 2, Math.Min(w, h));
			var path = new GraphicsPath();

			path.AddArc(0, 0, diameter, diameter, 180, 90);
			path.AddArc(w - diameter, 0, diameter, diameter, 270, 90);
			path.AddArc(w - diameter, h - diameter, diameter, diameter, 0, 90);
			path.AddArc(0, h - diameter, diameter, diameter, 90, 90);
			path.CloseAllFigures();

			mask.FillPath(Brushes.White, path);
		}
	}
}