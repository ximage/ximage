using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Crops
{
	public class Fit : ICrop
	{
		public string Documentation
		{
			get { return "It will be resized proportionally until it fits completely within the w x h boundaries.  This will likely result in void space on the left/right or top/bottom which will be filled with the supplied color.  Must be a 6 or 8 digit lower case hex, don't use #."; }
		}

		public Color? Color { get; set; }

		public Fit()
		{

		}

		public Fit(string color)
		{
			// TODO: Parse the color in its many forms.
		}

		public void SetSizeAndCrop(XImageRequest request, XImageResponse response)
		{
			if (Color != null)
				response.OutputGraphics.Clear(Color.Value);

			// TODO: Size and crop!
		}
	}
}