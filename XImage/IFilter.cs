using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XImage
{
	public interface IFilter
	{
		void PreProcess(XImageRequest request, XImageResponse response);

		void PostProcess(XImageRequest request, XImageResponse response);
	}
}
