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

		public bool ForceWidth { get; set; }

		public int? Height { get; private set; }
	
		public bool ForceHeight { get; private set; }
		
		public List<IFilter> Filters { get; private set; }
		
		public List<IMeta> Metas { get; private set; }

		public List<IOutput> Outputs { get; private set; }
		
		public bool IsOutputImplicitlySet { get; private set; }

		public XImageRequest(HttpContext httpContext)
		{
			_httpContext = httpContext;

			var q = HttpUtility.ParseQueryString(httpContext.Request.Url.Query);

			ParseWidthAndHeight(q);
			ParseFiltersAndOutput(httpContext, q);
			ParseMetas(q);

			ParseBackwardsCompatibility(httpContext, q);
		}

		void ParseWidthAndHeight(NameValueCollection q)
		{
			var w = q["w"] ?? q["width"];
			if (w != null)
			{
				if (w.EndsWith("!"))
				{
					ForceWidth = true;
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
					ForceHeight = true;
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
			Outputs = new List<IOutput>();

			var f = q["f"] ?? q["filter"] ?? q["filters"];
			if (f != null)
			{
				var filterMethodsWithArgs = f.SplitMethods();
				if (filterMethodsWithArgs.Count == 0)
					throw new ArgumentException("The f parameter cannot be empty.  Exclude this parameters if no filters are needed.");

				foreach (var filterString in filterMethodsWithArgs)
				{
					var filter = XImageFactory.CreateInstance<IFilter>(filterString);
					if (filter is IOutput)
						Outputs.Add(filter as IOutput);
					else
						Filters.Add(filter);
				}

				if (Filters.Count == 0 && Outputs.Count == 0)
					throw new ArgumentException("No filters specified.  Use ?f={filter1};{filter2} or leave f out of the query string.");
			}

			// Parse o param in case it was used separately.
			var o = q["o"] ?? q["output"] ?? q["outputs"];
			if (o != null)
			{
				var outputMethodsWithArgs = o.SplitMethods();
				if (outputMethodsWithArgs.Count == 0)
					throw new ArgumentException("The o parameter cannot be empty.  Exclude this parameters if no outputs are needed.");

				foreach (var outputString in outputMethodsWithArgs)
					Outputs.Add(XImageFactory.CreateInstance<IOutput>(outputString));

				if (Outputs.Count == 0)
					throw new ArgumentException("No outputs specified.  Use ?o={output1};{output2} or leave o out of the query string.");
			}

			// If this request doesn't already have an image-based output, implicity set one.
			if (!Outputs.Exists(oo => oo.ContentType.ToLower().Contains("image/")))
			{
				IsOutputImplicitlySet = true;

				var implicitOutput = httpContext.Response.ContentType ?? "jpg";
				implicitOutput = implicitOutput.Replace("image/", "").Replace("jpeg", "jpg");
				var output = XImageFactory.CreateInstance<IOutput>(implicitOutput);
				Outputs.Insert(0, output);
			}

			_httpContext.Response.ContentType = Outputs.LastOrDefault().ContentType;
		}

		void ParseMetas(NameValueCollection q)
		{
			Metas = new List<IMeta>();

			// TODO: Use the query string somehow?

			Metas.AddRange(XImageFactory.MetaTypes.Select(m => Activator.CreateInstance(m) as IMeta));
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
			var disposables = 
				Filters.OfType<IDisposable>()
				.Union(Metas.OfType<IDisposable>())
				.Union(Outputs.OfType<IDisposable>());

			foreach (var disposable in disposables)
				disposable.Dispose();
		}
	}
}