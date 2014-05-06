using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	[Documentation(Text = "Flips the image on its x-axis.")]
	public class FlipX : Scale
	{
		[Example(QueryString = "?w=100&f=flipx")]
		public FlipX() : base(-1, 1) { }
	}
}