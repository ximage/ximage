using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace XImage.Outputs
{
	public class Json : IOutput
	{
		public string ContentType
		{
			get { return "application/json"; }
		}

		public bool SupportsTransparency { get { return false; } }

		public void FormatImage(XImageRequest request, XImageResponse response)
		{
			// Super simple JSON output.  No JSON lib necessary, reduces dependencies.
			// Everything is a string?  What about numbers and arrays?

			var keys = response.Properties.AllKeys.Where(k => k.StartsWith("X-Image")).ToList();
			var sb = new StringBuilder();
			sb.AppendLine("{");
			foreach (var key in keys)
			{
				sb.Append("  \"");
				sb.Append(key);
				sb.Append("\": \"");
				sb.Append(response.Properties[key]);
				sb.Append("\"");
				if (key != keys.Last())
					sb.Append(',');
				sb.AppendLine();
			}
			sb.AppendLine("}");

			var bytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
			response.OutputStream.Write(bytes, 0, bytes.Length);
		}
	}
}