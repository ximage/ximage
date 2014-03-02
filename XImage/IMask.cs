using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XImage
{
	public interface IMask
	{
		string Documentation { get; }

		void DrawMask(XImageRequest request, XImageResponse response, Graphics mask);
	}
}
