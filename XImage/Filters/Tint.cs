using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class Tint : IFilter
	{
		Color _hue;
		decimal _amount;

		public Tint() : this(Color.Red, .5M) { }

		public Tint(Color color) : this(color, .5M) { }

		public Tint(Color color, decimal amount)
		{
			// TODO: Make color work in the constructor.  Then update BGColor and the others.

			_hue = color;
			_amount = amount;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			response.OutputImage.ApplyTint(_hue, (int)(_amount * 100M));
		}
	}
}