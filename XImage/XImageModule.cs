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
				app.Request.QueryString.ContainsAnyKeys(XImager2.XIMAGE_PARAMETERS))
			{
				// ASP.NET bug requires we hit the Filter getter before the setter.
				var outputStream = app.Response.Filter;

				app.Response.Filter = new InterceptingStream(_ =>
				{
					try
					{
						using (var xRequest = new XImageRequest(app.Context))
						{
							using (var xResponse = new XImageResponse(app.Context))
							{
								XImager2.ProcessImage(xRequest, xResponse);
							}
						}
					}
					catch (ArgumentException ex)
					{
						EndWithError(app, HttpStatusCode.BadRequest, ex.Message);
					}
				});
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
