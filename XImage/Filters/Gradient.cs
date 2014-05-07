using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace XImage.Filters
{
	[Documentation(Text = "Applies a gradient over the image.")]
	public class Gradient : IFilter
	{
		Color _color1;
		Color _color2;
		float _angle;
		Rectangle _rectangle;

		[Example(QueryString = "?w=100&f=gradient")]
		public Gradient() : this(Color.White, Color.Black, 90) { }

		[Example(QueryString = "?w=100&f=gradient({red},{blue})")]
		public Gradient(Color color1, Color color2) : this(color1, color2, 90) { }

		[Example(QueryString = "?w=100&f=gradient({red},{blue},45)")]
		public Gradient(Color color1, Color color2, decimal angle) : this(color1, color2, angle, Rectangle.Empty) { }

		[Example(QueryString = "?w=100&f=gradient({red},{blue},45,[25,25,50,50])")]
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
			var size = response.CanvasSize;
			if (_rectangle.IsEmpty)
				_rectangle = new Rectangle(0, 0, size.Width, size.Height);

			var brush = new LinearGradientBrush(_rectangle, _color1, _color2, _angle);
			response.OutputGraphics.FillRectangle(brush, _rectangle);
		}
	}
}