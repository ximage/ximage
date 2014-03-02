using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class Invert : IFilter
	{
		public string Documentation
		{
			get { return "Inverts the colors"; }
		}

		public void ProcessImage(XImageRequest request, XImageResponse response, byte[] data)
		{
			for (int i = 0; i < data.Length; i++)
				if (i % 4 != 3) // ignore the alpha channel
					data[i] = (byte)(255 - data[i]);
		}
	}
}