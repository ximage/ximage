using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace XImage.Filters
{
	[Documentation(Text = "bla bla bla")]
	public class BottomFade : IFilter
	{
		[Example(QueryString = "?sldkfjsadlfk jasdflks adf")]
		public BottomFade()
		{
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			Color color;
			if (!response.Palette.TryGetValue("Accent", out color))
				color = Color.Black;

			int w = response.CanvasSize.Width;
			int h = response.CanvasSize.Height / 2;

			var brush = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.Transparent, color, 90F);

			response.OutputGraphics.FillRectangle(brush, new Rectangle(0, response.CanvasSize.Height - h, w, h));
		}
	}
}