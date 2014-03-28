using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Saturate : IFilter
	{
		// Luminance vector for linear RGB
		const float rwgt = 0.3086f;
		const float gwgt = 0.6094f;
		const float bwgt = 0.0820f;

		decimal _amount;

		public Saturate() : this(5) { }

		public Saturate(int amount) : this((decimal)amount) { }

		public Saturate(decimal amount)
		{
			_amount = amount;

			if (_amount > 10 || _amount < 0)
				throw new ArgumentException("The grayscale amount must be between 0 and 10.");
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			var matrix = new ColorMatrix();

			float baseSaturation = 1F - (float)_amount;
			float saturation = 1F - baseSaturation;

			matrix[0, 0] = baseSaturation * rwgt + saturation;
			matrix[0, 1] = baseSaturation * rwgt;
			matrix[0, 2] = baseSaturation * rwgt;
			matrix[1, 0] = baseSaturation * gwgt;
			matrix[1, 1] = baseSaturation * gwgt + saturation;
			matrix[1, 2] = baseSaturation * gwgt;
			matrix[2, 0] = baseSaturation * bwgt;
			matrix[2, 1] = baseSaturation * bwgt;
			matrix[2, 2] = baseSaturation * bwgt + saturation;

			response.ImageAttributes.SetColorMatrix(matrix);
		}
	}
}