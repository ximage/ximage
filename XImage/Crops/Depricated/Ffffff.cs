using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Crops
{
	/// <summary>
	/// Provides backwards compatibility where you could specify the color arg without a function name.
	/// </summary>
	[Obsolete]
	public class Ffffff : Zoom
	{
		public Ffffff()
		{
			Color = Color.White;
		}
	}
}