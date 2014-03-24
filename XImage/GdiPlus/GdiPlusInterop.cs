using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace System.Drawing
{
	internal class GdiPlusInterop
	{
		/// <summary>
		/// Applys an effect to a bitmap.
		/// </summary>
		/// <param name="bitmap">A pointer or handle to the bitmap.</param>
		/// <param name="effect">A pointer or handle to the effect.</param>
		/// <param name="rectOfInterest">The rectangle to apply the effect, on out the area applied.</param>
		/// <param name="useAuxData">True to return effect auxillary data.</param>
		/// <param name="auxData">Contains pointer to auxillary data on out.</param>
		/// <param name="auxDataSize">Contains the size in bytes of the auxillary data.</param>
		/// <returns>A GpStatus value.</returns>
		[DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int GdipBitmapApplyEffect(HandleRef bitmap, HandleRef effect, IntPtr bounds, bool useAuxData, out IntPtr auxData, out int auxDataSize);

		/// <summary>
		/// Creates a new GDI Plus Effect.
		/// </summary>
		/// <param name="guid">The Guid for the effect.</param>
		/// <param name="effect">On out the pointer or handle to the effect.</param>
		/// <returns>A GpStatus value.</returns>
		[DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int GdipCreateEffect(Guid guid, out IntPtr effect);

		/// <summary>
		/// Sets the parameters for an effect.
		/// </summary>
		/// <param name="effect">A pointer or handle to the effect.</param>
		/// <param name="parameters">A pointer to the parameters to set.</param>
		/// <param name="size">The size in bytes of the parameters.</param>
		/// <returns>A GpStatus value.</returns>
		[DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int GdipSetEffectParameters(IntPtr effect, IntPtr parameters, uint size);
	}
}