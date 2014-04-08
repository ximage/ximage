﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Grayscale : IFilter
	{
		// Luminance vector for linear RGB
		const float rwgt = 0.3086f;
		const float gwgt = 0.6094f;
		const float bwgt = 0.0820f; 
		
		decimal _amount;

		public Grayscale() : this(1) { }

		public Grayscale(decimal amount)
		{
			_amount = amount;

			if (_amount > 1 || _amount < 0)
				throw new ArgumentException("The grayscale amount must be between 0 and 1.");
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			var matrix = new ColorMatrix();

			float baseSaturation = (float)_amount;
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

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}