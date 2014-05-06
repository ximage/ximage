using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	[Documentation(Text = "Flips the image on its y-axis.")]
	public class FlipY : Scale
	{
		[Example(QueryString = "?w=100&f=flipy")]
		public FlipY() : base(1, -1) { }
	}
}