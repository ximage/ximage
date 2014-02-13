using System;
using System.Net;
using System.Web;

namespace XImage
{
	public class XImageModule : IHttpModule
	{
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
					var xImageParams = new XImageParameters(app.Context);

					if (xImageParams.HasAnyValues)
					{
						app.Response.ContentType = xImageParams.GetContentType();
						app.Response.Filter = new XImageFilterStream(app.Response.Filter, xImageParams);
					}
				}
				catch (ArgumentException ex)
				{
					EndWithError(app, HttpStatusCode.BadRequest, ex.Message);
				}
			}
		}

		private void EndWithError(HttpApplication app, HttpStatusCode statusCode, string message)
		{
			app.Response.ClearHeaders();
			app.Response.ClearContent();
			app.Response.TrySkipIisCustomErrors = true;
			app.Response.StatusCode = (int)statusCode;
			app.Response.ContentType = "text/html";
			app.Response.Write("<pre>" + message + "</pre>");
			app.Response.End();
		}

		public void Dispose()
		{
		}
	}
}
