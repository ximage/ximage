using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;

namespace XImage.Outputs
{
	public class Help : IOutput
	{
		static readonly string HELP = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("XImage.Resources.Help.html")).ReadToEnd();

		int _statusCode;
		string _errorMessage;

		public string ContentType { get { return "text/html"; } }

		public bool SupportsTransparency { get { return false; } }

		public Help() : this((int)HttpStatusCode.BadRequest, null) { }

		public Help(int statusCode) : this(statusCode, null) { }

		public Help(string errorMessage) : this((int)HttpStatusCode.BadRequest, errorMessage) { }

		public Help(int statusCode, string errorMessage)
		{
			_statusCode = statusCode;
			_errorMessage = errorMessage;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			var app = HttpContext.Current.ApplicationInstance;

			app.Response.ClearHeaders();
			app.Response.ClearContent();
			app.Response.TrySkipIisCustomErrors = true;
			app.Response.StatusCode = (int)_statusCode;
			app.Response.ContentType = "text/html";

			app.Response.Output.WriteLine(HELP);

			app.Response.End();
		}
	}
}