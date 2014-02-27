using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XImage
{
	public interface IMask
	{
		string Documentation { get; }

		void ProcessImage(XImageRequest request, XImageResponse response, params string[] args);
	}
}
