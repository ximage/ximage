using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace System.Drawing
{
	/// <summary>
	/// Specifies the number and type of histograms that represent the color channels of a bitmap.
	/// </summary>
	public enum GpHistogramFormat
	{
		/// <summary>
		/// Four histograms, Alpha, Red, Green and Blue.
		/// </summary>
		HistogramFormatARGB,

		/// <summary>
		/// Four histograms, Alpha, Red, Green, Blue, but the Red, Green and Blue channels 
		/// are premultiplied by the Alpha.
		/// </summary>
		HistogramFormatPARGB,

		/// <summary>
		/// Three histograms, Red, Green, Blue.
		/// </summary>
		HistogramFormatRGB,

		/// <summary>
		/// One histogram, Grayscale.
		/// </summary>
		HistogramFormatGray,

		/// <summary>
		/// One histogram, Blue.
		/// </summary>
		HistogramFormatB,

		/// <summary>
		/// One histogram, Green.
		/// </summary>
		HistogramFormatG,

		/// <summary>
		/// One histogram, Red.
		/// </summary>
		HistogramFormatR,

		/// <summary>
		/// One histogram, Alpha.
		/// </summary>
		HistogramFormatA
	}

	internal class Utils
	{
		/// <summary>
		/// Pins a set of objects and returns the GC Handles. Fails if any pins fail.
		/// </summary>
		/// <param name="objs">The objects to pin.</param>
		/// <returns>An array of GCHandles.</returns>
		public static GCHandle[] PinObjects(params object[] objs)
		{
			var lhHandles = new GCHandle[objs.Length];

			try
			{
				for (int liCounter = 0; liCounter < objs.Length; liCounter++)
					lhHandles[liCounter] = GCHandle.Alloc(objs[liCounter], GCHandleType.Pinned);
			}
			catch
			{
				UnpinObjects(lhHandles);
				throw;
			}

			return lhHandles;
		}

		/// <summary>
		/// Unpins an array of GCHandles.
		/// </summary>
		/// <param name="handles">The handles to unpin.</param>
		public static void UnpinObjects(params GCHandle[] handles)
		{
			foreach (GCHandle lhHandle in handles)
				if (lhHandle.IsAllocated)
					lhHandle.Free();
		}
	}

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

		/// <summary>
		/// Gets histogram data for a bitmap.
		/// </summary>
		/// <param name="bitmap">A pointer or handle to the bitmap.</param>
		/// <param name="format">The format of the histogram data. This determines the number of channels of data returned.</param>
		/// <param name="numberOfEntries">The number of entries provided in the channel data.</param>
		/// <param name="uiChannel0">A pointer to the first channel data.</param>
		/// <param name="uiChannel1">A pointer to the second channel data or null if not needed.</param>
		/// <param name="uiChannel2">A pointer to the third channel data or null if not needed.</param>
		/// <param name="uiChannel3">A pointer to the forth channel data or null if not needed.</param>
		/// <returns>A GpStatus value.</returns>
		[DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int GdipBitmapGetHistogram(HandleRef bitmap, GpHistogramFormat format, uint numberOfEntries, IntPtr uiChannel0, IntPtr uiChannel1, IntPtr uiChannel2, IntPtr uiChannel3);

		/// <summary>
		/// Gets the histogram channel data size as a number of elements.
		/// </summary>
		/// <param name="format">The format of the histogram data.</param>
		/// <param name="numberOfEntries">On out the number of entires required per channel data.</param>
		/// <returns>A GpStatus value.</returns>
		[DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int GdipBitmapGetHistogramSize(GpHistogramFormat format, out uint numberOfEntries);
	}
}