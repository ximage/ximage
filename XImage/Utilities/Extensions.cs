using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
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

		public static Color? AsNullableColor(this string value)
		{
			var color = value;
			if (color.Contains('{') || color.Contains('}'))
				color = color.Replace("{", "").Replace("}", "");

			// Yup, you're seeing that right.  Maybe later I'll find a non-throwing converter.
			try
			{
				return ColorTranslator.FromHtml(color);
			}
			catch
			{
				try
				{
					return ColorTranslator.FromHtml("#" + color);
				}
				catch
				{
					if (value.Contains('{'))
						throw new ArgumentException(value + " is not a valid color.  Try something more like: white, {6699ff}, or {255,255,255} or {255,255,255,64}");

					return null;
				}
			}
		}

		public static Rectangle? AsNullableRectangle(this string value)
		{
			if (!value.Contains('[') || !value.Contains(']'))
				return null;

			var args = value.SplitClean('[', ',', ']');
			if (args.Length != 4)
				throw new ArgumentException("Rectangles must have four arguments: [x,y,w,h]");

			var x = args[0].AsNullableInt();
			var y = args[1].AsNullableInt();
			var w = args[2].AsNullableInt();
			var h = args[3].AsNullableInt();

			if (x == null || y == null || w == null || h == null)
				throw new ArgumentException("Rectangle arguments must be non-decimal values.");

			return new Rectangle(x.Value, y.Value, w.Value, h.Value);
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

		public static Size ScaleToWidth(this Size size, int width)
		{
			return new Size(width, Convert.ToInt32((float)size.Height / (float)size.Width * (float)width));
		}

		public static Size ScaleToHeight(this Size size, int height)
		{
			return new Size(Convert.ToInt32((float)size.Width / (float)size.Height * (float)height), height);
		}

		public static float GetAspectRatio(this Size size)
		{
			return (float)size.Width / (float)size.Height;
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

		public static void ApplyMask(this Bitmap bitmap, GraphicsPath path, Brush brush, bool opaqueMask = false)
		{
			// TODO: This next line needs to be batch-able so we're not copying bytes over and over.  (Maybe use unsafe?)
			using (var bitmapBits = bitmap.GetBitmapBits(true))
			{
				using (var maskBitmap = new Bitmap(bitmap.Width, bitmap.Height))
				{
					using (var maskGraphics = Graphics.FromImage(maskBitmap))
					{
						maskGraphics.SmoothingMode = SmoothingMode.HighQuality;

						maskGraphics.FillPath(brush, path);

						using (var maskData = maskBitmap.GetBitmapBits(false))
						{
							if (opaqueMask)
								bitmapBits.Data.BlendLayer(maskData.Data, BlendingModes.OpaqueMask);
							else
								bitmapBits.Data.BlendLayer(maskData.Data, BlendingModes.Mask);
						}
					}
				}
			}
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
				case BlendingModes.OpaqueMask:
					for (int i = 0; i < length; i += 4)
					{
						targetLayer[i + 3] = layerToBlend[i + 3];
						var p = (float)(255 - layerToBlend[i + 3]) / 255F;
						targetLayer[i] += Convert.ToByte((float)(255 - targetLayer[i]) * p);
						targetLayer[i + 1] += Convert.ToByte((float)(255 - targetLayer[i + 1]) * p);
						targetLayer[i + 2] += Convert.ToByte((float)(255 - targetLayer[i + 2]) * p);
					}
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
		OpaqueMask,
		Normal,
		Multiply,
		Screen
	}
}