using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace XImage.Outputs
{
	[Documentation(Text = "Shows this help page.")]
	public class Help : IOutput
	{
		static readonly string HELP = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("XImage.Resources.Help.html")).ReadToEnd();

		int _statusCode;
		string _errorMessage;

		public string ContentType { get { return "text/html"; } }

		public bool SupportsTransparency { get { return false; } }

		[Example(QueryString = "?o=help")]
		public Help() : this((int)HttpStatusCode.OK, null) { }

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
			app.Response.ContentType = ContentType;

			var filtersHtml = BuildFunctionsDocs(XImageFactory.FilterTypes.Where(t => !t.GetInterfaces().Any(i => i == typeof(IOutput))));
			var outputsHtml = BuildFunctionsDocs(XImageFactory.OutputTypes);

			app.Response.Output.WriteLine(
				HELP
				.Replace("{{error}}", _errorMessage ?? "")
				.Replace("{{filters}}", filtersHtml.ToString())
				.Replace("{{outputs}}", outputsHtml.ToString())
				.Replace("{{url}}", app.Request.Url.Segments.Last()));
		}

		private static StringBuilder BuildFunctionsDocs(IEnumerable<Type> types)
		{
			var app = HttpContext.Current.ApplicationInstance;

			var html = new StringBuilder();
			foreach (var functionType in types.OrderBy(f => f.Name))
			{
				string functionString = null;

				var docs = Attribute.GetCustomAttribute(functionType, typeof(DocumentationAttribute)) as DocumentationAttribute;
				if (docs != null)
				{
					functionString = string.Format(
						METHOD_TEMPLATE
						.Replace("{{name}}", functionType.Name.ToLower())
						.Replace("{{text}}", docs.Text));

					var examplesHtml = new StringBuilder();
					foreach (var constructor in functionType.GetConstructors())
					{
						var exampleAttr = Attribute.GetCustomAttribute(constructor, typeof(ExampleAttribute)) as ExampleAttribute;
						if (exampleAttr != null)
						{
							var args = string.Join(",", constructor.GetParameters().Select(p => p.Name.ToLower()));
							examplesHtml.Append(
								EXAMPLE_TEMPLATE
								.Replace("{{url}}", app.Request.Url.Segments.Last() + exampleAttr.QueryString)
								.Replace("{{ctor}}", functionType.Name.ToLower() + string.Format("({0})", args)));
						}
					}

					functionString = functionString.Replace("{examples}", examplesHtml.ToString());
				}

				html.Append(functionString);
			}
			return html;
		}

		const string METHOD_TEMPLATE = @"
			<li>
				<h3>{{name}}</h3>
				<p>
					{{text}}
				</p>
				<ul>
					{{examples}}
				</ul>
			</li>";

		const string EXAMPLE_TEMPLATE = @"
					<li>
						<a href=""{{url}}"">{{ctor}}</a>
					</li>";
	}
}