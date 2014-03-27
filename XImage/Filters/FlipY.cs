using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class FlipY : Scale
	{
		public FlipY() : base(1, -1) { }
	}
}