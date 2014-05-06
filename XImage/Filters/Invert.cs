using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Inverts the colors.")]
	public class Invert : IFilter
	{
		[Example(QueryString = "?w=100&f=invert")]
		public Invert()
		{
		}

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