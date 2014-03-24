using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Invert : IFilter
	{
		public string Documentation
		{
			get { return "Inverts the colors"; }
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			using (var bitmapBits = response.OutputImage.GetBitmapBits(true))
			{
				var data = bitmapBits.Data;

				for (int i = 0; i < data.Length; i++)
					if (i % 4 != 3) // ignore the alpha channel
						data[i] = (byte)(255 - data[i]);
			}
		}
	}
}