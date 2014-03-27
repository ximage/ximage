using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage.Outputs
{
	public class Png : IOutput
	{
		public string Documentation { get { return "Uses a png encoder."; } }

		public string ContentType { get { return "image/png"; } }

		public bool SupportsTransparency { get { return true; } }

		public void FormatImage(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.Save(response.OutputStream, ImageFormat.Png);
		}
	}
}