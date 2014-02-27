using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XImage
{
	public interface ICrop
	{
		string Documentation { get; }

		void SetSizeAndCrop(XImageRequest request, XImageResponse response);
	}
}
