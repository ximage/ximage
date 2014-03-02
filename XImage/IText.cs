﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage
{
	public interface IText
	{
		string Documentation { get; }

		void DrawText(XImageRequest request, XImageResponse response);
	}
}