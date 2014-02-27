using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage
{
	public interface IText
	{
		string Documentation { get; }

		void ProcessImage(XImageRequest request, XImageResponse response, params string[] args);
	}
}