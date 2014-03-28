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
		string ContentType { get; }

		bool SupportsTransparency { get; }

		void FormatImage(XImageRequest request, XImageResponse response);
	}
}
