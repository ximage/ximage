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
							new Outputs.Debug().PostProcess(null, null);
					}
				}
			}
			catch (Exception ex)
			{
				new Outputs.Help(ex.Message).PostProcess(null, null);
			}
		}

		public void Dispose()
		{
		}
	}
}
