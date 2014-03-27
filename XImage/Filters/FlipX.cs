using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class FlipX : Scale
	{
		public FlipX() : base(-1, 1) { }
	}
}