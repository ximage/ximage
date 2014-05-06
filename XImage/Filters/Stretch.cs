using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = @"Each edge will be resized disproportionally until it reaches its w x h boundaries.
							Results are exactly w x h and no edges are cropped but the image will appear distorted.")]
	public class Stretch : IFilter
	{
		[Example(QueryString = "?w=300&h=100&f=stretch")]
		public Stretch()
		{
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			response.CropBox = new Rectangle(Point.Empty, response.InputImage.Size);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}
	}
}