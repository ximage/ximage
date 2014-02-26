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

	public class XImageRequest
	{
		static readonly int MAX_SIZE = ConfigurationManager.AppSettings["XImage.MaxSize"].AsNullableInt() ?? 1000;
		static readonly string[] PARAM_ORDER = { "w", "h", "c", "f", "m", "t", "o" };
		static readonly Dictionary<string, ICrop> _cropsLookup;
		static readonly Dictionary<string, IFilter> _filtersLookup;
		static readonly Dictionary<string, IMask> _masksLookup;
		static readonly Dictionary<string, IText> _textsLookup;
		static readonly Dictionary<string, IOutput> _outputsLookup;

		public int? Width { get; private set; }
		public int? Height { get; private set; }
		public bool AllowUpscaling { get; private set; }
		public string Crop { get; private set; }
		public Color? CropAsColor { get; private set; }
		public List<IFilter> Filters { get; private set; }
		public List<string[]> FiltersArgs { get; private set; }
		public IOutput Output { get; private set; }
		public string[] OutputArgs { get; private set; }
		public bool HasAnyValues
		{
			get
			{
				return
					Width != null ||
					Height != null ||
					Crop != null ||
					(Filters != null && Filters.Count > 0);
			}
		}

		static XImageRequest()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();

			_cropsLookup = GetInstances<ICrop>(types).ToDictionary(k => k.MethodName.ToLower(), v => v);
			_filtersLookup = GetInstances<IFilter>(types).ToDictionary(k => k.MethodName.ToLower(), v => v);
			_masksLookup = GetInstances<IMask>(types).ToDictionary(k => k.MethodName.ToLower(), v => v);
			_textsLookup = GetInstances<IText>(types).ToDictionary(k => k.MethodName.ToLower(), v => v);
			_outputsLookup = GetInstances<IOutput>(types).ToDictionary(k => k.MethodName.ToLower(), v => v);
		}

		static List<T> GetInstances<T>(List<Type> types)
			where T : class
		{
			var interfaceType = typeof(T);
			return types
				.Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
				.Select(t => Activator.CreateInstance(t) as T)
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
			FiltersArgs = new List<string[]>();

			var filterValues = q["f"];
			if (filterValues != null)
			{
				var filters = filterValues.SplitClean(';');
				if (filters.Length == 0)
					throw new ArgumentException("The f parameter cannot be empty.  Exclude this parameters if no filters are needed.");
				foreach (var filter in filters)
				{
					// Note: This doesn't account for strings as filter args yet, just numbers.
					if (filter.Contains(' '))
						throw new ArgumentException("Don't leave any spaces in your filter methods.  Enforcing this strictly helps optimize cache hit ratios.");
					var tokens = filter.Split('(', ')');
					if (tokens.Length == 3 && tokens[2] != "")
						throw new ArgumentException("Filter methods must be of the format 'method(arg1,arg2,...)'.");
					var method = tokens[0];
					var args = tokens.Length > 2 ? tokens[1].Split(',') : null;

					IFilter found;
					if (_filtersLookup.TryGetValue(filter, out found))
					{
						Filters.Add(found);
						FiltersArgs.Add(args);
					}
					else
					{
						throw new ArgumentException(string.Format("Couldn't find any filters by the name {0}.", filter));
					}
				}
			}
		}

		void ParseOutput(HttpContext httpContext, NameValueCollection q)
		{
			var o = q["o"] ?? httpContext.Response.ContentType;
			if (o == null)
				throw new ArgumentException("No output format specified.  Use ?o={output} or ensure the Content-Type response header is set.");

			o = o.Replace("image/", "").Replace("jpeg", "jpg");

			// Note: This doesn't account for strings as filter args yet, just numbers.
			if (o.Contains(' '))
				throw new ArgumentException("Don't leave any spaces in your filter methods.  Enforcing this strictly helps optimize cache hit ratios.");
			var tokens = o.Split('(', ')');
			if (tokens.Length == 3 && tokens[2] != "")
				throw new ArgumentException("Filter methods must be of the format 'method(arg1,arg2,...)'.");
			var method = tokens[0];
			var args = tokens.Length > 2 ? tokens[1].Split(',') : null;

			IOutput output;
			if (_outputsLookup.TryGetValue(method, out output))
			{
				Output = output;
				OutputArgs = args;
			}
			else
			{
				throw new ArgumentException("Unrecognized output type: {0}.", method);
			}
		}

		void ParseOrder(NameValueCollection q)
		{
			var requestedOrder = q.AllKeys.Where(k => PARAM_ORDER.Contains(k)).ToArray();
			var correctOrder = PARAM_ORDER.Where(p => requestedOrder.Contains(p)).ToArray();
			if (string.Concat(requestedOrder) != string.Concat(correctOrder))
				throw new ArgumentException("Each parameter is optional.  But they must appear in the order of w, h, c, f, m, t, o. Enforcing this strictly helps optimize cache hit ratios.");
		}
	}
}