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
		static readonly string HELP = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("XImage.Resources.Help.txt")).ReadToEnd();

		public void Init(HttpApplication context)
		{
			context.PostRequestHandlerExecute += InterceptImage2;
		}

		void InterceptImage2(object sender, EventArgs e)
		{
			var app = sender as HttpApplication;

			// Keep it lightweight.  Don't do any XImage work unless we're 200 OK and we 
			// received an image content-type and the query string has at least one modifier.
			if (app.Response.StatusCode == 200 && 
				app.Response.ContentType.StartsWith("image/") &&
				app.Request.QueryString.AllKeys.Any(p => XImager2.XIMAGE_PARAMETERS.Contains(p)))
			{
				try
				{
					var outputStream = app.Response.Filter;
					app.Response.Filter = new InterceptingStream(bufferedStream =>
					{
						using (var request = new XImageRequest(app.Context))
						{
							using (var response = new XImageResponse(app.Context))
							{
								XImager2.ProcessImage(request, response);
							}
						}
					});
				}
				catch (ArgumentException ex)
				{
					EndWithError(app, HttpStatusCode.BadRequest, ex.Message);
				}
			}
		}

		void InterceptImage(object sender, EventArgs e)
		{
			var app = sender as HttpApplication;

			if (app.Response.StatusCode == 200 && app.Response.ContentType.StartsWith("image/"))
			{
				try
				{
					// Don't do any work if the query string doesn't have any modifiers.
					if (app.Request.QueryString.AllKeys.Any(p => XImager2.XIMAGE_PARAMETERS.Contains(p)))
					{
						var outputStream = app.Response.Filter;
						app.Response.Filter = new InterceptingStream(bufferedStream =>
						{
							var stopwatch = Stopwatch.StartNew();

							var request = new XImageRequest(app.Context);
							var properties = new XImager(request).CopyTo(bufferedStream, outputStream);

							app.Response.ContentType = request.Output.ContentType;
							app.Response.Headers.Add("X-Image-Processing-Time", string.Format("{0:N2}ms", 1000D * (double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency));
							foreach (var property in properties)
								app.Response.Headers.Add(property.Key, property.Value);
						});
					}
				}
				catch (ArgumentException ex)
				{
					EndWithError(app, HttpStatusCode.BadRequest, ex.Message);
				}
			}
		}

		void EndWithError(HttpApplication app, HttpStatusCode statusCode, string error = null)
		{
			app.Response.ClearHeaders();
			app.Response.ClearContent();
			app.Response.TrySkipIisCustomErrors = true;
			app.Response.StatusCode = (int)statusCode;
			app.Response.ContentType = "text/html";
			app.Response.Output.WriteLine("<html><body><pre>");
			if (!error.IsNullOrEmpty())
			{
				app.Response.Output.WriteLine("ERROR");
				app.Response.Output.WriteLine("-----");
				app.Response.Output.WriteLine(error);
				app.Response.Output.WriteLine("");

				app.Response.AddHeader("X-Image-Error", error);
			}
			app.Response.Output.WriteLine(HELP, app.Request.Url.Segments.Last());
			app.Response.Output.WriteLine("</pre></body></html>");
	
			app.Response.End();
		}

		public void Dispose()
		{
		}
	}
}
