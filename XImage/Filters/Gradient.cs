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
		Rectangle _rectangle;

		public Gradient() : this(Color.White, Color.Black, 90) { }

		public Gradient(Color color1, Color color2) : this(color1, color2, 90) { }

		public Gradient(Color color1, Color color2, decimal angle) : this(color1, color2, angle, Rectangle.Empty) { }

		public Gradient(Color color1, Color color2, decimal angle, Rectangle rectangle)
		{
			_color1 = color1;
			_color2 = color2;
			_angle = (float)angle;
			_rectangle = rectangle;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			var size = response.OutputImage.Size;
			if (_rectangle.IsEmpty)
				_rectangle = new Rectangle(size.Width / -2, size.Height / -2, size.Width, size.Height);
			_rectangle.Offset(size.Width / -2, size.Height / -2);
			var brush = new LinearGradientBrush(_rectangle, _color1, _color2, _angle);
			response.OutputGraphics.FillRectangle(brush, _rectangle);
		}
	}
}