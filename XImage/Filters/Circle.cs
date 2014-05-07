using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Crops the image into a circle.  This will return a square image if it isn't already.  It also defaults to a PNG output (unless overridden) to support transparency.")]
	public class Circle : IFilter
	{
		[Example(QueryString = "?w=100&f=circle")]
		public Circle()
		{
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			// Unless explicitly requested by the user, default to PNG for this filter.
			if (request.IsOutputImplicitlySet)
			{
				request.Outputs.RemoveAll(o => o.ContentType.StartsWith("image"));
				request.Outputs.Add(new Outputs.Png());
			}

			// Make the assumption that if they want a circle, they also want square dimensions.
			var smallestSide = Math.Min(response.CanvasSize.Width, response.CanvasSize.Height);
			response.CanvasSize = new Size(smallestSide, smallestSide);
			response.ContentArea = new Rectangle(Point.Empty, response.CanvasSize);
			new Fill().PreProcess(request, response);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			var size = response.ContentArea.Size;

			var origin = response.ContentArea.Location;
			if (size.Width < size.Height)
				origin.Y += (size.Height - size.Width) / 2;
			else
				origin.X += (size.Width - size.Height) / 2;

			var d = Math.Min(size.Width - 1, size.Height - 1);
			size = new Size(d, d);

			var path = new GraphicsPath();
			path.AddEllipse(new Rectangle(origin, size));

			response.OutputImage.ApplyMask(path, Brushes.White, !request.Outputs.Exists(o => o.SupportsTransparency));
		}
	}
}