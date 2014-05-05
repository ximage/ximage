using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage
{
	public class DocumentationAttribute : Attribute
	{
		public string Text { get; set; }
	}
}