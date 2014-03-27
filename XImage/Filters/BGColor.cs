using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class BGColor : IFilter
	{
		Color _color;

		public string Documentation
		{
			get { return "Background color."; }
		}

		public BGColor(int color)
			: this(color.ToString())
		{
		}

		public BGColor(string color)
		{
			try
			{
				try
				{
					_color = (Color)new ColorConverter().ConvertFromString("#" + color);
				}
				catch
				{
					_color = (Color)new ColorConverter().ConvertFromString(color);
				}
			}
			catch
			{
				throw new ArgumentException(string.Format("Invalid color {0} for the fit crop.", color));
			}
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.Clear(_color);
		}
	}
}