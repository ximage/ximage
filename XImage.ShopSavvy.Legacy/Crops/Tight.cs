using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XImage.Crops;

namespace XImage.ShopSavvy.Legacy.Crops
{
	/// <summary>
	/// Provides backwards compatibility.
	/// </summary>
	[Obsolete]
	public class Tight : Whitespace
	{
		public Tight() : base(0) { }

		public Tight(int padding) : base(padding, padding, padding, padding) { }

		public Tight(int topBottom, int leftRight) : base(topBottom, leftRight, topBottom, leftRight) { }

		public Tight(int top, int leftRight, int bottom) : base(top, leftRight, bottom, leftRight) { }

		public Tight(int top, int right, int bottom, int left) : base(top, right, bottom, left) { }
	}
}