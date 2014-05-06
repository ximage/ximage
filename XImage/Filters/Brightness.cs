using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Increases or decreases the brightness.  Use 0-1 to decrease brightness and 1-10 to increase.")]
	public class Brightness : IFilter
	{
		decimal _amount;

		[Example(QueryString = "?w=100&f=brightness")]
		public Brightness() : this(5) { }

		[Example(QueryString = "?w=100&f=brightness(10)")]
		public Brightness(decimal amount)
		{
			_amount = amount;

			if (_amount > 10 || _amount < 0)
				throw new ArgumentException("The brightness amount must be between 0 and 10.");
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			float brightness = (float)_amount;
			if (brightness > 1F) // After 1 it goes to a 1-10 scale, weird.
				brightness = brightness / 10F + 1F;
			float contrast = 1F;
			float adjustedBrightness = brightness - 1F;

			var matrix = new ColorMatrix(new float[][]
			{
				new float[] {contrast, 0, 0, 0, 0},
				new float[] {0, contrast, 0, 0, 0},
				new float[] {0, 0, contrast, 0, 0},
				new float[] {0, 0, 0, 1F, 0},
				new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
			});

			response.ImageAttributes.SetColorMatrix(matrix);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}