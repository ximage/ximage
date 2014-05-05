using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Linq;
using XImage.Utilities;

namespace XImage
{
	public class XImageModule : IHttpModule
	{
		public void Init(HttpApplication app)
		{
			app.BeginRequest += (_, __) =>
			{
				// Keep it lightweight.  Don't do any XImage work unless the query string has XImage keys.
				// Note: At this point, we don't necessarily know if it's an image yet.
				if (app.Request.HasXImageParameters())
				{
					app.Context.Items["XImage.Profiler"] = new XImageProfiler(app.Response.Headers);
				}
			};

			app.PostRequestHandlerExecute += (_, __) =>
			{
				// Keep it lightweight.  Don't do any XImage work unless the query string has XImage keys,
				// and we're 200 OK and we received an image/* content-type.
				if (app.Request.HasXImageParameters() &&
					app.Response.StatusCode == 200 &&
					app.Response.ContentType.StartsWith("image/"))
				{
					// ASP.NET bug requires we hit the Filter getter before the setter.
					var outputStream = app.Response.Filter;

					app.Response.Filter = new InterceptingStream(RunXImage);
				}
			};
		}

		void RunXImage(Stream stream)
		{
			var profiler = HttpContext.Current.Items["XImage.Profiler"] as XImageProfiler;

			try
			{
				profiler.Mark("Image downloaded");

				using (var xRequest = new XImageRequest(HttpContext.Current))
				{
					using (var xResponse = new XImageResponse(HttpContext.Current, profiler))
					{
						profiler.Mark("Image decoded");

						XImager.ProcessImage(xRequest, xResponse);

						if (xRequest.IsDebug)
							EndWithDebug(HttpContext.Current.ApplicationInstance);
					}
				}
			}
			catch (Exception ex)
			{
				new Outputs.Help(ex.Message).PostProcess(null, null);
			}
		}

		void EndWithDebug(HttpApplication app)
		{
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

		public void Dispose()
		{
		}
	}
}
