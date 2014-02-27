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
	public class Crops
	{
		public const string NONE = "none";
		public const string FILL = "fill";
		public const string STRETCH = "stretch";
		public const string COLOR = "color";
	}

	public class XImageRequest : IDisposable
	{
		static readonly int MAX_SIZE = ConfigurationManager.AppSettings["XImage.MaxSize"].AsNullableInt() ?? 1000;
		static readonly Dictionary<string, Type> _cropsLookup;
		static readonly Dictionary<string, Type> _filtersLookup;
		static readonly Dictionary<string, Type> _masksLookup;
		static readonly Dictionary<string, Type> _textsLookup;
		static readonly Dictionary<string, Type> _outputsLookup;
		static readonly Dictionary<Type, Dictionary<string, Type>> _lookupLookup;

		public int? Width { get; private set; }
		public int? Height { get; private set; }
		public bool AllowUpscaling { get; private set; }
		public string Crop { get; private set; }
		public Color? CropAsColor { get; private set; }
		public List<IFilter> Filters { get; private set; }
		public IOutput Output { get; private set; }
		static XImageRequest()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();

			_cropsLookup = GetTypes<ICrop>(types).ToDictionary(k => k.Name.ToLower(), v => v);
			_filtersLookup = GetTypes<IFilter>(types).ToDictionary(k => k.Name.ToLower(), v => v);
			_masksLookup = GetTypes<IMask>(types).ToDictionary(k => k.Name.ToLower(), v => v);
			_textsLookup = GetTypes<IText>(types).ToDictionary(k => k.Name.ToLower(), v => v);
			_outputsLookup = GetTypes<IOutput>(types).ToDictionary(k => k.Name.ToLower(), v => v);

			_lookupLookup = new Dictionary<Type, Dictionary<string, Type>>()
			{
				{ typeof(ICrop), _cropsLookup },
				{ typeof(IFilter), _filtersLookup },
				{ typeof(IMask), _masksLookup },
				{ typeof(IText), _textsLookup },
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
			ParseCrop(q);
			ParseFilters(q);
			ParseOutput(httpContext, q);
			ParseOrder(q);

			httpContext.Response.ContentType = Output.ContentType;
		}

		void ParseHelp(HttpContext httpContext)
		{
			if (httpContext.Request.RawUrl.EndsWith("?help"))
				throw new ArgumentException(string.Empty);
		}

		void ParseWidthAndHeight(NameValueCollection q)
		{
			bool allowWUpscaling = false, allowHUpscaling = false;
			var w = q["w"];
			if (w != null)
			{
				allowWUpscaling = w.EndsWith("!");
				if (allowWUpscaling == true)
					w = w.Substring(0, w.Length - 1);
				Width = w.AsNullableInt();
				if (Width == null || Width <= 0)
					throw new ArgumentException("Width must be a positive integer.");
				if (Width > MAX_SIZE)
					throw new ArgumentException(string.Format("Cannot request a width larger than the max configured value of {0}.", MAX_SIZE));
			}

			var h = q["h"];
			if (h != null)
			{
				allowHUpscaling = h.EndsWith("!");
				if (allowHUpscaling == true)
					h = h.Substring(0, h.Length - 1);
				Height = h.AsNullableInt();
				if (Height == null || Height <= 0)
					throw new ArgumentException("Height must be a positive integer.");
				if (Height > MAX_SIZE)
					throw new ArgumentException(string.Format("Cannot request a height larger than max configured value of {0}.", MAX_SIZE));
			}

			if (Width != null && Height != null && allowWUpscaling != allowHUpscaling)
				throw new ArgumentException("If upscaling '!' is enabled and both w and h are specified, the '!' must be used on both.  Enforcing this strictly helps optimize cache hit ratios.");
			AllowUpscaling = allowWUpscaling || allowHUpscaling;
		}

		void ParseCrop(NameValueCollection q)
		{
			var c = q["c"];
			if (c != null)
			{
				if (c == Crops.NONE)
					throw new ArgumentException("It isn't necessary to spcify a crop of 'none'.  Just leave it off.  Enforcing this strictly helps optimize cache hit ratios.");
				else if (c == Crops.FILL || c == Crops.STRETCH)
					Crop = c;
				else
				{
					if (c.Length != 6 && (c.Length != 8 || c.StartsWith("ff")))
						throw new ArgumentException("When specifying a color for crop (c), it must be a valid 6 digit hex number (or an 8 digit hex number for translucency).  Enforcing this strictly helps optimize cache hit ratios.");
					try
					{
						CropAsColor = ColorTranslator.FromHtml("#" + c);
						Crop = Crops.COLOR;
						if (c != c.ToLower())
							throw new ArgumentException("When specifying a color for crop (c), do not use capital letters.");
					}
					catch
					{
						throw new ArgumentException("When specifying a color for crop (c), it must be a valid 6 digit hex number (or an 8 digit hex number for translucency).  Enforcing this strictly helps optimize cache hit ratios.");
					}
				}

				if (Crop == null)
					throw new ArgumentException("The only valid options for crop are (none), fill, stretch or a hex color.  See below for more details.");

				if ((Width == null || Height == null) && Crop != null)
					throw new ArgumentException("A cropping mode is only valid when both width and height are specified.");
			}
		}

		void ParseFilters(NameValueCollection q)
		{
			Filters = new List<IFilter>();

			var filterValues = q["f"];
			if (filterValues == null)
				return;

			var filters = filterValues.SplitClean(';');
			if (filters.Length == 0)
				throw new ArgumentException("The f parameter cannot be empty.  Exclude this parameters if no filters are needed.");

			foreach (var filter in filters)
				Filters.Add(ParseMethod<IFilter>(filter));

			if (Filters.Count == 0)
				throw new ArgumentException("No filters specified.  Use ?f={filter1};{filter2} or leave f out of the query string.");
		}

		void ParseOutput(HttpContext httpContext, NameValueCollection q)
		{
			var o = q["o"] ?? httpContext.Response.ContentType;
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

		void ParseOrder(NameValueCollection q)
		{
			var requestedOrder = q.AllKeys.Where(k => XImager.XIMAGE_PARAMETERS.Contains(k)).ToArray();
			var correctOrder = XImager.XIMAGE_PARAMETERS.Where(p => requestedOrder.Contains(p)).ToArray();
			if (string.Concat(requestedOrder) != string.Concat(correctOrder))
				throw new ArgumentException("Each parameter is optional.  But they must appear in the order of w, h, c, f, m, t, o. Enforcing this strictly helps optimize cache hit ratios.");
		}

		public void Dispose()
		{
		}
	}
}