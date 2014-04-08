﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Opacity : IFilter
	{
		decimal _amount;

		public Opacity() : this(.5M) { }

		public Opacity(decimal amount)
		{
			_amount = amount;

			// TODO: Assert correct args here.
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			var matrix = new ColorMatrix();
			matrix.Matrix00 = matrix.Matrix11 = matrix.Matrix22 = matrix.Matrix44 = 1;
			matrix.Matrix33 = (float)_amount;

			response.ImageAttributes.SetColorMatrix(matrix);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}