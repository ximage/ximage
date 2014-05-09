using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage.Meta
{
	public class Colors : IMeta
	{
		public void Calculate(XImageRequest request, XImageResponse response)
		{
			foreach (var pair in response.Palette)
				response.Properties["X-Image-Color-" + pair.Key] = pair.Value.ToHex();
		}
	}
}