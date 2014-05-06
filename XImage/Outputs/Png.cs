using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage.Outputs
{
	[Documentation(Text = @"Uses a PNG encoder.")]
	public class Png : IOutput
	{
		[Example(QueryString = "?w=100&o=png")]
		public Png()
		{
		}

		public string ContentType { get { return "image/png"; } }

		public bool SupportsTransparency { get { return true; } }

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.Save(response.OutputStream, ImageFormat.Png);
		}
	}
}