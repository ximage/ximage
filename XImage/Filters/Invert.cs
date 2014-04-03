using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class Invert : IFilter
	{
		static readonly ColorMatrix _invert = new ColorMatrix(new float[][]
		{
			new float[] { -1, 0, 0, 0, 0 }, 
			new float[] { 0, -1, 0, 0, 0 },  
			new float[] { 0, 0, -1, 0, 0 }, 
			new float[] { 0, 0, 0, 1, 0 },
			new float[] { 1, 1, 1, 0, 1 }
		});

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.ImageAttributes.SetColorMatrix(_invert);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}