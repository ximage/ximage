using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class BorderRadius : IFilter
	{
		int _topLeft;
		int _topRight;
		int _bottomRight;
		int _bottomLeft;

		public BorderRadius() : this(20) { }

		public BorderRadius(decimal radius) : this(radius, radius, radius, radius) { }

		public BorderRadius(decimal topLeft, decimal topRight, decimal bottomRight, decimal bottomLeft)
		{
			_topLeft = (int)topLeft;
			_topRight = (int)topRight;
			_bottomRight = (int)bottomRight;
			_bottomLeft = (int)bottomLeft;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			// Unless explicitly requested by the user, default to PNG for this filter.
			if (request.IsOutputImplicitlySet)
				request.Output = new Outputs.Png();
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			var contentArea = response.ContentArea;
			var size = contentArea.Size;
			var loc = contentArea.Location;

			int w = size.Width - 1, h = size.Height - 1;
			int diameter = 0;
			var path = new GraphicsPath();

			diameter = Math.Min(_topLeft * 2, Math.Min(w, h));
			if (diameter > 0)
				path.AddArc(loc.X, loc.Y, diameter, diameter, 180, 90);
			else
				path.AddLine(loc.X, loc.Y, loc.X + 1, loc.Y);

			diameter = Math.Min(_topRight * 2, Math.Min(w, h));
			if (diameter > 0)
				path.AddArc(loc.X + w - diameter, loc.Y, diameter, diameter, 270, 90);
			else
				path.AddLine(loc.X + w, loc.Y, loc.X + w, loc.Y + 1);

			diameter = Math.Min(_bottomRight * 2, Math.Min(w, h));
			if (diameter > 0)
				path.AddArc(loc.X + w - diameter, loc.Y + h - diameter, diameter, diameter, 0, 90);
			else
				path.AddLine(loc.X + w, loc.Y + h, loc.X + w - 1, loc.Y + h);

			diameter = Math.Min(_bottomLeft * 2, Math.Min(w, h));
			if (diameter > 0)
				path.AddArc(loc.X, loc.Y + h - diameter, diameter, diameter, 90, 90);
			else
				path.AddLine(loc.X, loc.Y + h, loc.X, loc.Y + h - 1);
			
			path.CloseAllFigures();

			response.OutputImage.ApplyMask(path, Brushes.White, !request.Output.SupportsTransparency);
		}
	}
}