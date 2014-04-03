using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Sepia : IFilter
	{
		static readonly ColorMatrix _sepia = new ColorMatrix(new float[][]
		{
			new float[] { .393f, .349f, .272f, 0, 0 }, 
			new float[] { .769f, .686f, .534f, 0, 0 },
			new float[] { .189f, .168f, .131f, 0, 0 },
			new float[] { 0, 0, 0, 1, 0 },
			new float[] { 0, 0, 0, 0, 1 }
		});


		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.ImageAttributes.SetColorMatrix(_sepia);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}