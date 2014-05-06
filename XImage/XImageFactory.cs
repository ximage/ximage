using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XImage.Utilities;

namespace XImage
{
	public class XImageFactory
	{
		static readonly Dictionary<string, Type> _filtersLookup;
		static readonly Dictionary<string, Type> _metasLookup;
		static readonly Dictionary<string, Type> _outputsLookup;
		static readonly Dictionary<Type, Dictionary<string, Type>> _lookupLookup;

		public static IEnumerable<Type> FilterTypes { get { return _filtersLookup.Values; } }
		public static IEnumerable<Type> MetaTypes { get { return _metasLookup.Values; } }
		public static IEnumerable<Type> OutputTypes { get { return _outputsLookup.Values; } }

		static XImageFactory()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();

			_filtersLookup = GetTypes<IFilter>(types).Where(t => t != typeof(IOutput)).ToDictionary(k => k.Name.ToLower(), v => v);
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

		public static T CreateInstance<T>(string methodWithArgs)
			where T : class
		{
			if (methodWithArgs.Contains(' '))
				throw new ArgumentException("Don't leave any spaces in your filter methods.  Enforcing this strictly helps optimize cache hit ratios.");
			var tokens = methodWithArgs.Split('(', ')');
			if (tokens.Length == 3 && tokens[2] != "")
				throw new ArgumentException("Filter methods must be of the format 'method(arg1,arg2,...)'.");
			var methodName = tokens[0];
			object[] args = null;
			if (tokens.Length > 2)
			{
				// Object array of strongly-typed (parsed) objects.
				var strArgs = tokens[1].SplitMethods();
				args = new object[strArgs.Count];
				for (int c = 0; c < args.Length; c++)
				{
					var s = strArgs[c];

					// If it's "url" take the next value to be the URL.
					if (s == "url" && tokens.Length > 2)
					{
						var url = tokens[2];
						if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://"))
							url = "http://" + url;
						args[c] = new Uri(url);
						continue;
					}

					// If in quotes, force it to be a string.
					if (s.Contains('"'))
					{
						args[c] = s.Replace("\"", "");
						continue;
					}

					// Is it a number?
					var number = s.AsNullableDecimal();
					if (number != null)
					{
						args[c] = number.Value;
						continue;
					}

					// Is it a color?
					var color = s.AsNullableColor();
					if (color != null)
					{
						args[c] = color.Value;
						continue;
					}

					// Is it a rectangle?
					var rectangle = s.AsNullableRectangle();
					if (rectangle != null)
					{
						args[c] = rectangle.Value;
						continue;
					}

					// Default to a string then.
					args[c] = s.Replace("\"", "");
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
					throw new ArgumentException(string.Format("There is no constructor for {0}.", methodWithArgs), ex);
				}
			}
			else
			{
				throw new ArgumentException(string.Format("Could not find any function by that name and/or arguments: {0}.", methodName));
			}
		}
	}
}