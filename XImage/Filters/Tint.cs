using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	[Documentation(Text = "Applies a tint to the image.")]
	public class Tint : IFilter
	{
		Color _hue;
		decimal _amount;

		[Example(QueryString = "?w=300&h=100&f=tint")]
		public Tint() : this(Color.Red, .5M) { }

		[Example(QueryString = "?w=300&h=100&f=tint({red})")]
		public Tint(Color color) : this(color, .5M) { }

		[Example(QueryString = "?w=300&h=100&f=tint({blue},.5)")]
		public Tint(Color color, decimal amount)
		{
			// TODO: Make color work in the constructor.  Then update BGColor and the others.

			_hue = color;
			_amount = amount;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.ApplyTint(_hue, (int)(_amount * 100M));
		}
	}
}