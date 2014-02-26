using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class Invert : IFilter
	{
		public string MethodName
		{
			get { return "Invert"; }
		}

		public string MethodDescription
		{
			get { return "Inverts the colors"; }
		}

		public string ExampleQueryString
		{
			get { return "invert"; }
		}

		public void ProcessImage(byte[] data, params string[] args)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(255 - data[i]);
			}
		}
	}
}