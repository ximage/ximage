using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Brightness : IFilter
	{
		decimal _amount;

		public string Documentation
		{
			get { return "Brightness"; }
		}

		public Brightness() : this(5) { }

		public Brightness(int amount)
			: this((decimal)amount)
		{
		}

		public Brightness(decimal amount)
		{
			_amount = amount;

			if (_amount > 10 || _amount < 0)
				throw new ArgumentException("The brightness amount must be between 0 and 10.");
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
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
	}
}