using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage
{
	public static class Extensions
	{
		public static int? AsNullableInt(this string value)
		{
			if (value == null)
				return null;

			int i;
			if (int.TryParse(value, out i))
				return i;

			return null;
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue value;
			dictionary.TryGetValue(key, out value);
			return value;
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}
	}
}