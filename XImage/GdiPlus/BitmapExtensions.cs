using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;

namespace System.Drawing
{
	public static class BitmapExtensions
	{
		public static void ApplyBlur(this Bitmap bitmap, int radius, bool expandEdges = false)
		{
			bitmap.ApplyEffect(
				new GdiPlusEffect(
					"{633C80A4-1843-482B-9EF2-BE2834C5FDD4}",
					new GdiPlusParams.BlurParams
					{
						Radius = radius,
						ExpandEdges = expandEdges,
					}));
		}

		public static void ApplyTint(this Bitmap bitmap, Color hue, int amount)
		{
			bitmap.ApplyEffect(
				new GdiPlusEffect(
					"{1077AF00-2848-4441-9489-44AD4C2D7A2C}",
					new GdiPlusParams.TintParams
					{
						Amount = amount,
						Hue = hue.ToColorWheelColor(),
					}));
		}

		internal static void ApplyEffect(this Bitmap bitmap, GdiPlusEffect effect)
		{
			IntPtr auxData;
			int auxDataSize;

			var status = GdiPlusInterop.GdipBitmapApplyEffect(
				bitmap: new HandleRef(bitmap, bitmap.NativeHandle()),
				effect: new HandleRef(effect, effect.NativeHandle()),
				bounds: IntPtr.Zero,
				useAuxData: false,
				auxData: out auxData,
				auxDataSize: out auxDataSize);
		}

		internal static int ToColorWheelColor(this Color color)
		{
			// TODO: Figure this guy out.
			return 160;
		}

		public static IntPtr NativeHandle(this Bitmap bitmap)
		{
			return bitmap.GetPrivateField<IntPtr>("nativeImage");
		}

		internal static T GetPrivateField<T>(this object o, string fieldName)
		{
			if (o == null)
				return default(T);

			var type = o.GetType();

			var field = type.GetField(fieldName, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);

			if (field != null)
				return (T)field.GetValue(o);
			else
				throw new InvalidOperationException(
					string.Format(
						"Instance field '{0}' could not be located in object of type '{1}'.",
						fieldName, 
						type.FullName));
		}
	}
}