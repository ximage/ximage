using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage.Outputs
{
	public class Gif : IOutput
	{
		public string ContentType { get { return "image/gif"; } }

		public bool SupportsTransparency { get { return false; } }

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.Save(response.OutputStream, ImageFormat.Gif);
		}
	}
}