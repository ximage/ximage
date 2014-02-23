using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Linq;

namespace XImage
{
	public class XImageModule : IHttpModule
	{
		static readonly string HELP = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("XImage.Help.txt")).ReadToEnd();

		public void Init(HttpApplication context)
		{
			context.PostRequestHandlerExecute += InterceptImage;
		}

		void InterceptImage(object sender, EventArgs e)
		{
			var app = sender as HttpApplication;

			if (app.Response.StatusCode == 200 && app.Response.ContentType.StartsWith("image/"))
			{
				try
				{
					var stopwatch = Stopwatch.StartNew();

					var xImageParams = new XImageParameters(app.Context);

					if (xImageParams.HasAnyValues)
					{
						app.Response.ContentType = xImageParams.GetContentType();

						var output = app.Response.Filter;
						app.Response.Filter = new InterceptingStream(bufferedStream =>
						{
							var properties = new XImager(xImageParams).CopyTo(bufferedStream, output);

							foreach (var property in properties)
								app.Response.Headers.Add(property.Key, property.Value);

							app.Response.Headers.Add("X-Image-Processing-Time", string.Format("{0:N2}ms", 1000D * (double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency));
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
