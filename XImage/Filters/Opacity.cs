using System;
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
		public string Documentation
		{
			get { return "Opacity level."; }
		}

		public Opacity() : this(.5M) { }

		public Opacity(int amount) : this((decimal)amount) { }

		public Opacity(decimal amount)
		{
			_amount = amount;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			var matrix = new ColorMatrix();
			matrix.Matrix00 = matrix.Matrix11 = matrix.Matrix22 = matrix.Matrix44 = 1;
			matrix.Matrix33 = (float)_amount;

			response.ImageAttributes.SetColorMatrix(matrix);
		}
	}
}