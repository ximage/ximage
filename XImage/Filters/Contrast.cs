using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Contrast : IFilter
	{
		decimal _amount;

		public Contrast() : this(5) { }

		public Contrast(decimal amount)
		{
			_amount = amount;

			if (_amount > 10 || _amount < 0)
				throw new ArgumentException("The contrast amount must be between 0 and 10.");
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			float contrast = (float)_amount;
			if (contrast > 1F) // After 1 it goes to a 1-10 scale, weird.
				contrast = contrast / 10F + 1F;
			float adjustedBrightness = 0;

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