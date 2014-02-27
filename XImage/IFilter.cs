using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XImage
{
	public interface IFilter
	{
		string Documentation { get; }

		void ProcessImage(byte[] data);
	}
}
