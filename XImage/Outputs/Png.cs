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
		public string MethodName { get { return "png"; } }
		public string MethodDescription { get { return "Uses a png encoder."; } }
		public string[] ExampleQueryStrings { get { return new string[] { "png" }; } }
		public string ContentType { get { return "image/png"; } }

		public void ProcessImage(Bitmap outputImage, Stream outputStream, params string[] args)
		{
			outputImage.Save(outputStream, ImageFormat.Png);
		}
	}
}