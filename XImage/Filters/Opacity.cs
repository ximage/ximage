using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = @"Adjusts the opacity by a specified amount.  Use values 0-1.  
							Additionally, unless the output format is explicitly set, it defaults to PNG 
							to preserve transparency.")]
	public class Opacity : IFilter
	{
		decimal _amount;

		[Example(QueryString = "?w=100&f=opacity")]
		public Opacity() : this(.5M) { }

		[Example(QueryString = "?w=100&f=opacity(.25)")]
		public Opacity(decimal amount)
		{
			_amount = amount;

			// TODO: Assert correct args here.
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			// Unless explicitly requested by the user, default to PNG for this filter.
			if (request.IsOutputImplicitlySet)
				request.Output = new Outputs.Png();

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