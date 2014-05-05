using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using XImage.Utilities;

namespace XImage
{
	public class XImageRequest : IDisposable
	{
		public static readonly int MAX_SIZE = ConfigurationManager.AppSettings["XImage.MaxSize"].AsNullableInt() ?? 1000;

		HttpContext _httpContext;

		public int? Width { get; private set; }

		public int? Height { get; private set; }
	
		public bool AllowUpscaling { get; private set; }
		
		public List<IFilter> Filters { get; private set; }
		
		public List<IMeta> Metas { get; private set; }

		IOutput _output = null;
		public IOutput Output
		{
			get { return _output; }
			set
			{
				_output = value;
				if (_output != null)
					_httpContext.Response.ContentType = _output.ContentType;
			}
		}
		
		public bool IsOutputImplicitlySet { get; private set; }

		public bool IsDebug { get; private set; }

		public XImageRequest(HttpContext httpContext)
		{
			_httpContext = httpContext;

			var q = HttpUtility.ParseQueryString(httpContext.Request.Url.Query);

			ParseHelp(httpContext);
			ParseWidthAndHeight(q);
			ParseFiltersAndOutput(httpContext, q);
			ParseMetas(q);
			ParseDebug(q);

			ParseBackwardsCompatibility(httpContext, q);
		}

		void ParseHelp(HttpContext httpContext)
		{
			if (httpContext.Request.RawUrl.EndsWith("?help"))
				throw new ArgumentException(string.Empty);
		}

		void ParseWidthAndHeight(NameValueCollection q)
		{
			var w = q["w"] ?? q["width"];
			if (w != null)
			{
				if (w.EndsWith("!"))
				{
					AllowUpscaling = true;
					w = w.Substring(0, w.Length - 1);
				}
				Width = w.AsNullableInt();
				if (Width == null || Width <= 0)
					throw new ArgumentException("Width must be a positive integer.");
				if (Width > MAX_SIZE)
					throw new ArgumentException(string.Format("Cannot request a width larger than the max configured value of {0}.", MAX_SIZE));
			}

			var h = q["h"] ?? q["height"];
			if (h != null)
			{
				if (h.EndsWith("!"))
				{
					AllowUpscaling = true;
					h = h.Substring(0, h.Length - 1);
				}
				Height = h.AsNullableInt();
				if (Height == null || Height <= 0)
					throw new ArgumentException("Height must be a positive integer.");
				if (Height > MAX_SIZE)
					throw new ArgumentException(string.Format("Cannot request a height larger than max configured value of {0}.", MAX_SIZE));
			}
		}

		void ParseFiltersAndOutput(HttpContext httpContext, NameValueCollection q)
		{
			Filters = new List<IFilter>();

			var filterValues = q["f"] ?? q["filter"] ?? q["filters"];
			if (filterValues != null)
			{
				var filterMethodsWithArgs = filterValues.SplitMethods(); ;
				if (filterMethodsWithArgs.Count == 0)
					throw new ArgumentException("The f parameter cannot be empty.  Exclude this parameters if no filters are needed.");

				foreach (var filterString in filterMethodsWithArgs)
				{
					var filter = XImageFactory.CreateInstance<IFilter>(filterString);
					if (filter is IOutput)
						Output = filter as IOutput;
					else
						Filters.Add(filter);
				}

				if (Filters.Count == 0 && Output == null)
					throw new ArgumentException("No filters specified.  Use ?f={filter1};{filter2} or leave f out of the query string.");
			}

			if (Output == null)
			{
				// Parse o param in case it was used separately.
				var o = q["o"] ?? q["output"];
				if (o == null)
					IsOutputImplicitlySet = true;

				// If o is still null, pull from the content type of the input image.
				o = o ?? httpContext.Response.ContentType;

				// If o is still null, we have bigger problems.
				if (o == null)
					throw new ArgumentException("No output format specified.  Use ?o={output} or ensure the Content-Type response header is set.");

				o = o.Replace("image/", "").Replace("jpeg", "jpg");

				Output = XImageFactory.CreateInstance<IOutput>(o);
			}
		}

		void ParseMetas(NameValueCollection q)
		{
			Metas = new List<IMeta>();

			// TODO: Use the query string somehow?

			Metas.AddRange(XImageFactory.MetaTypes.Select(m => Activator.CreateInstance(m) as IMeta));
		}

		void ParseDebug(NameValueCollection q)
		{
			IsDebug = q.ContainsKey("debug");
		}

		private void ParseBackwardsCompatibility(HttpContext httpContext, NameValueCollection q)
		{
			var c = q["c"] ?? q["crop"];
			if (c != null)
			{
				switch (c)
				{
					case "fit":
					case "zoom":
					case "ffffff":
						Filters.Insert(0, new Filters.Fit());
						break;
					case "fill":
					case "zoom!":
						Filters.Insert(0, new Filters.Fill());
						break;
					case "stretch":
						Filters.Insert(0, new Filters.Stretch());
						break;
					case "whitespace":
						Filters.Insert(0, new Filters.Trim());
						break;
					case "whitespace!":
						Filters.Insert(0, new Filters.Trim());
						Filters.Insert(0, new Filters.Fill());
						break;
				}
			}
		}

		public void Dispose()
		{
		}
	}
}