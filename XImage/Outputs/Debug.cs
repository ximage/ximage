using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace XImage.Outputs
{
	public class Debug : IOutput, IDisposable
	{
		public string ContentType { get { return "text/html"; } }

		public bool SupportsTransparency { get { return false; } }

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void Dispose()
		{
			var app = HttpContext.Current.ApplicationInstance;

			var url = app.Request.RawUrl.Replace("debug", "");

			var profiler = app.Context.Items["XImage.Profiler"] as XImageProfiler;
			app.Response.ClearHeaders();
			app.Response.ClearContent();
			app.Response.TrySkipIisCustomErrors = true;
			app.Response.StatusCode = 200;
			app.Response.ContentType = "text/html";
			app.Response.Output.WriteLine("<!DOCTYPE html><html><head><title>XImage</title></head><body>");

			if (profiler.Markers.Count > 0)
			{
				var previous = profiler.Markers.First();
				foreach (var marker in profiler.Markers.Skip(1))
				{
					//app.Response.Output.Write("<span style=\"padding: 20px; display: inline-block\">");
					app.Response.Output.Write("<div>");
					{
						app.Response.Output.Write(marker.Item1);
						app.Response.Output.Write(" <b style=\"padding-left: 20px;\">");
						var ticks = marker.Item2 - previous.Item2;
						app.Response.Output.Write(string.Format(
							"{0:N2}ms",
							1000D * (double)ticks / (double)Stopwatch.Frequency));
						app.Response.Output.Write("</b> ");

						//app.Response.Output.Write("<br /><img src=\"");
						//{
						//	app.Response.Output.Write(url);
						//}
						//app.Response.Output.Write("\" />");
					}
					app.Response.Output.Write("</div>");
					//app.Response.Output.Write("</span>");

					previous = marker;
				}
			}

			app.Response.Output.WriteLine("</body></html>");
		}
	}
}