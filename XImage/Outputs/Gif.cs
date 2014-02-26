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
		public string MethodName { get { return "gif"; } }
		public string MethodDescription { get { return "Uses a gif encoder."; } }
		public string[] ExampleQueryStrings { get { return new string[]{ "gif" }; } }
		public string ContentType { get { return "image/gif"; } }

		public void ProcessImage(Bitmap outputImage, Stream outputStream, params string[] args)
		{
			outputImage.Save(outputStream, ImageFormat.Gif);
		}
	}
}