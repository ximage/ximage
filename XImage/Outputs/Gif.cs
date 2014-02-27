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
		public string Documentation { get { return "Uses a gif encoder."; } }

		public string ContentType { get { return "image/gif"; } }

		public void ProcessImage(Bitmap outputImage, Stream outputStream)
		{
			outputImage.Save(outputStream, ImageFormat.Gif);
		}
	}
}