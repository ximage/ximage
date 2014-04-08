using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace XImage.Filters
{
	public class Gradient : IFilter
	{
		Color _color1;
		Color _color2;
		float _angle;

		public Gradient() : this(Color.White, Color.Black, 90) { }

		public Gradient(Color color1, Color color2) : this(color1, color2, 90) { }

		public Gradient(Color color1, Color color2, decimal angle)
		{
			_color1 = color1;
			_color2 = color2;
			_angle = (float)angle;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			var size = response.OutputImage.Size;
			var rect = new Rectangle(size.Width / -2, size.Height / -2, size.Width, size.Height);
			var brush = new LinearGradientBrush(rect, _color1, _color2, _angle);
			response.OutputGraphics.FillRectangle(brush, rect);
		}
	}
}