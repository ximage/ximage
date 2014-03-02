using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Web;

namespace XImage.Utilities
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

		public static decimal? AsNullableDecimal(this string value)
		{
			if (value == null)
				return null;

			decimal d;
			if (decimal.TryParse(value, out d))
				return d;

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

		public static string ToHex(this Color color)
		{
			return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
		}

		public static string[] SplitClean(this string value, params char[] separator)
		{
			return value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		}

		public static bool ContainsAnyKeys(this NameValueCollection collection, IEnumerable<string> keys)
		{
			// It seems that when the querystring has a key but no value, e.g. '?help' NVC reports 'help'
			// as the value and null for the key.  I was expecting 'help' as the key.  

			return Enumerable
				.Range(0, collection.AllKeys.Length)
				.Select(i => collection.AllKeys[i] ?? collection.Get(i))
				.Any(i => keys.Contains(i));
		}

		public static BitmapBits GetBitmapBits(this Bitmap bitmap, bool writeAccess = false)
		{
			return new BitmapBits(bitmap, writeAccess);
		}

		public static void BlendLayer(this byte[] targetLayer, byte[] layerToBlend, BlendingModes blendingMode = BlendingModes.Normal)
		{
			if (targetLayer.Length != layerToBlend.Length)
				throw new ArgumentException("The two layers must be the same size.");

			var length = targetLayer.Length;
			switch (blendingMode)
			{
				case BlendingModes.Mask:
					for (int i = 3; i < length; i += 4)
						targetLayer[i] = layerToBlend[i];
						break;
				case BlendingModes.Normal:
				case BlendingModes.Multiply:
				case BlendingModes.Screen:
					throw new NotImplementedException();
			}
		}
	}

	public enum BlendingModes
	{
		Mask,
		Normal,
		Multiply,
		Screen
	}
}