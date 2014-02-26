using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage
{
	public class XImageResponse
	{
		public Bitmap SourceImage { get; private set; }
		public Bitmap OutputImage { get; private set; }
		public Graphics OutputGraphics { get; private set; }
		public Dictionary<string, string> Properties { get; private set; }

		public XImageResponse(Bitmap sourceImage, Bitmap outputImage, Graphics outputGraphics, Dictionary<string, string> properties)
		{
			SourceImage = sourceImage;
			OutputImage = outputImage;
			OutputGraphics = outputGraphics;
			Properties = properties;
		}
	}
}