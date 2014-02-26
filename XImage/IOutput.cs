using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XImage
{
	public interface IOutput
	{
		string MethodName { get; }

		string MethodDescription { get; }

		string[] ExampleQueryStrings { get; }

		string ContentType { get; }

		void ProcessImage(Bitmap outputImage, Stream outputStream, params string[] args);
	}
}
