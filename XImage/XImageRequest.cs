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
using System.Web;
using XImage.Utilities;

namespace XImage
{
	public class XImageRequest : IDisposable
	{
		public static readonly int MAX_SIZE = ConfigurationManager.AppSettings["XImage.MaxSize"].AsNullableInt() ?? 1000;
		static readonly Dictionary<string, Type> _filtersLookup;
		static readonly Dictionary<string, Type> _metasLookup;
		static readonly Dictionary<string, Type> _outputsLookup;
		static readonly Dictionary<Type, Dictionary<string, Type>> _lookupLookup;

		public int? Width { get; private set; }
		public int? Height { get; private set; }
		public bool AllowUpscaling { get; private set; }
		public List<IFilter> Filters { get; private set; }
		public List<IMeta> Metas { get; private set; }
		public IOutput Output { get; private set; }
		static XImageRequest()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();

			_filtersLookup = GetTypes<IFilter>(types).ToDictionary(k => k.Name.ToLower(), v => v);
			_metasLookup = GetTypes<IMeta>(types).ToDictionary(k => k.Name.ToLower(), v => v);
			_outputsLookup = GetTypes<IOutput>(types).ToDictionary(k => k.Name.ToLower(), v => v);

			_lookupLookup = new Dictionary<Type, Dictionary<string, Type>>()
			{
				{ typeof(IFilter), _filtersLookup },
				{ typeof(IMeta), _metasLookup },
				{ typeof(IOutput), _outputsLookup },
			};
		}

		static List<Type> GetTypes<T>(List<Type> types)
			where T : class
		{
			var interfaceType = typeof(T);
			return types
				.Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
				.ToList();
		}

		public XImageRequest(HttpContext httpContext)
		{
			var q = HttpUtility.ParseQueryString(httpContext.Request.Url.Query);

			ParseHelp(httpContext);
			ParseWidthAndHeight(q);
			ParseFilters(q);
			ParseMetas(q);
			ParseOutput(httpContext, q);

			httpContext.Response.ContentType = Output.ContentType;
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

		void ParseFilters(NameValueCollection q)
		{
			Filters = new List<IFilter>();

			var filterValues = q["f"] ?? q["filter"] ?? q["filters"];
			if (filterValues == null)
				return;

			var filterNames = filterValues.SplitClean(';');
			if (filterNames.Length == 0)
				throw new ArgumentException("The f parameter cannot be empty.  Exclude this parameters if no filters are needed.");

			foreach (var filterName in filterNames)
				Filters.Add(ParseMethod<IFilter>(filterName));

			if (Filters.Count == 0)
				throw new ArgumentException("No filters specified.  Use ?f={filter1};{filter2} or leave f out of the query string.");
		}

		void ParseMetas(NameValueCollection q)
		{
			Metas = new List<IMeta>();

			// TODO: Use the query string somehow?

			Metas.AddRange(_metasLookup.Select(m => Activator.CreateInstance(m.Value) as IMeta));
		}

		void ParseOutput(HttpContext httpContext, NameValueCollection q)
		{
			// If nothing is specified, default to the format of the original image.
			var o = q["o"] ?? q["output"] ?? httpContext.Response.ContentType;
			if (o == null)
				throw new ArgumentException("No output format specified.  Use ?o={output} or ensure the Content-Type response header is set.");

			o = o.Replace("image/", "").Replace("jpeg", "jpg");

			Output = ParseMethod<IOutput>(o);
		}

		T ParseMethod<T>(string method)
			where T : class
		{
			// Note: This doesn't account for strings as filter args yet, just numbers.
			if (method.Contains(' '))
				throw new ArgumentException("Don't leave any spaces in your filter methods.  Enforcing this strictly helps optimize cache hit ratios.");
			var tokens = method.Split('(', ')');
			if (tokens.Length == 3 && tokens[2] != "")
				throw new ArgumentException("Filter methods must be of the format 'method(arg1,arg2,...)'.");
			var methodName = tokens[0];
			object[] args = null;
			if (tokens.Length > 2)
			{
				// Object array of strongly-typed (parsed) objects.
				var strArgs = tokens[1].Split(',');
				args = new object[strArgs.Length];
				for (int c = 0; c < args.Length; c++)
				{
					var s = strArgs[c];
					var d = s.AsNullableDecimal();
					var i = s.AsNullableInt();
					if (i.HasValue)
						args[c] = i;
					else if (d.HasValue)
						args[c] = d;
					else
						args[c] = s;
				}
			}

			Type type;
			if (_lookupLookup[typeof(T)].TryGetValue(methodName, out type))
			{
				try
				{
					return Activator.CreateInstance(type, args) as T;
				}
				catch (MissingMethodException ex)
				{
					throw new ArgumentException(string.Format("There is no constructor for {0}.", method), ex);
				}
			}
			else
			{
				throw new ArgumentException(string.Format("Unrecognized type: {0}.", methodName));
			}
		}

		public void Dispose()
		{
		}
	}
}