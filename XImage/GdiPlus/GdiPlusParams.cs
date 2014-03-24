using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace System.Drawing
{
	internal static class GdiPlusParams
	{
		/// <summary>
		/// Contains members that specify the nature of a Gaussian blur.
		/// </summary>
		/// <remarks>Cannot be pinned with GCHandle due to bool value.</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BlurParams
		{
			/// <summary>
			/// Real number that specifies the blur radius (the radius of the Gaussian convolution kernel) in 
			/// pixels. The radius must be in the range 0 through 255. As the radius increases, the resulting 
			/// bitmap becomes more blurry.
			/// </summary>
			public float Radius;

			/// <summary>
			/// Boolean value that specifies whether the bitmap expands by an amount equal to the blur radius. 
			/// If TRUE, the bitmap expands by an amount equal to the radius so that it can have soft edges. 
			/// If FALSE, the bitmap remains the same size and the soft edges are clipped.
			/// </summary>
			public bool ExpandEdges;
		};

		/// <summary>
		/// Contains members that specify the nature of a brightness or contrast adjustment.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BrightnessContrastParams
		{
			/// <summary>
			/// Integer in the range -255 through 255 that specifies the brightness level. If the value is 0, 
			/// the brightness remains the same. As the value moves from 0 to 255, the brightness of the image 
			/// increases. As the value moves from 0 to -255, the brightness of the image decreases.
			/// </summary>
			public int BrightnessLevel;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies the contrast level. If the value is 0, 
			/// the contrast remains the same. As the value moves from 0 to 100, the contrast of the image 
			/// increases. As the value moves from 0 to -100, the contrast of the image decreases.
			/// </summary>
			public int ContrastLevel;
		}

		/// <summary>
		/// Contains members that specify the nature of a color balance adjustment.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ColorBalanceParams
		{
			/// <summary>
			/// Integer in the range -100 through 100 that specifies a change in the amount of red in the 
			/// image. If the value is 0, there is no change. As the value moves from 0 to 100, the amount 
			/// of red in the image increases and the amount of cyan decreases. As the value moves from 0 to 
			/// -100, the amount of red in the image decreases and the amount of cyan increases.
			/// </summary>
			public int CyanRed;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies a change in the amount of green in the 
			/// image. If the value is 0, there is no change. As the value moves from 0 to 100, the amount 
			/// of green in the image increases and the amount of magenta decreases. As the value moves from 
			/// 0 to -100, the amount of green in the image decreases and the amount of magenta increases.
			/// </summary>
			public int MagentaGreen;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies a change in the amount of blue in the 
			/// image. If the value is 0, there is no change. As the value moves from 0 to 100, the amount 
			/// of blue in the image increases and the amount of yellow decreases. As the value moves from 
			/// 0 to -100, the amount of blue in the image decreases and the amount of yellow increases.
			/// </summary>
			public int YellowBlue;
		}

		///// <summary>
		///// Contains members that specify an adjustment to the colors of a bitmap.
		///// </summary>
		//[StructLayout(LayoutKind.Sequential, Pack = 1)]
		//public struct ColorCurveParams
		//{
		//	/// <summary>
		//	/// Element of the GpCurveAdjustments enumeration that specifies the adjustment to be applied.
		//	/// </summary>
		//	public GpCurveAdjustments Adjustment;

		//	/// <summary>
		//	/// Element of the GpCurveChannel enumeration that specifies the color channel to which the 
		//	/// adjustment applies.
		//	/// </summary>
		//	public GpCurveChannel Channel;

		//	/// <summary>
		//	/// Integer that specifies the intensity of the adjustment. The range of acceptable values 
		//	/// depends on which adjustment is being applied. To see the range of acceptable values for a 
		//	/// particular adjustment, see the GpCurveAdjustments enumeration.
		//	/// </summary>
		//	public int AdjustValue;
		//}

		/// <summary>
		/// Contains members (color lookup tables) that specify color adjustments to a bitmap.
		/// </summary>
		/// <remarks>Cannot be pinned with GCHandle due to arrays.</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ColorLUTParams
		{
			/// <summary>
			/// Array of 256 bytes that specifies the adjustment for the blue channel.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
			public byte[] B;

			/// <summary>
			/// Array of 256 bytes that specifies the adjustment for the green channel.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
			public byte[] G;

			/// <summary>
			/// Array of 256 bytes that specifies the adjustment for the red channel.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
			public byte[] R;

			/// <summary>
			/// Array of 256 bytes that specifies the adjustment for the alpha channel.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
			public byte[] A;
		}

		/// <summary>
		/// Cntains 5 matrix rows to make up a 5×5 matrix of real numbers.
		/// </summary>
		/// <remarks>
		/// A 5×5 color matrix is a homogeneous matrix for a 4-space transformation. The element 
		/// in the fifth row and fifth column of a 5×5 homogeneous matrix must be 1, and all of 
		/// the other elements in the fifth column must be 0. Color matrices are used to transform 
		/// color vectors. The first four components of a color vector hold the red, green, blue, 
		/// and alpha components (in that order) of a color. The fifth component of a color vector 
		/// is always 1.
		/// </remarks>
		/// <remarks>Cannot be pinned with GCHandle due to arrays.</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ColorMatrixParams
		{
			/// <summary>
			/// Row for Red input of matrix in RGBAw order.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public float[] Mr;

			/// <summary>
			/// Row for Green input of matrix in RGBAw order.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public float[] Mg;

			/// <summary>
			/// Row for Blue input of matrix in RGBAw order.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public float[] Mb;

			/// <summary>
			/// Row for Alpha input of matrix in RGBAw order.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public float[] Ma;

			/// <summary>
			/// Row for w input of matrix in RGBAw order.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public float[] Mw;
		}

		/// <summary>
		/// Contains members that specify hue, saturation and lightness adjustments to a bitmap.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct HueSaturationLightnessParams
		{
			/// <summary>
			/// Integer in the range -180 through 180 that specifies the change in hue. A value 
			/// of 0 specifies no change. Positive values specify counterclockwise rotation on the 
			/// color wheel. Negative values specify clockwise rotation on the color wheel.
			/// </summary>
			public int HueLevel;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies the change in saturation. A 
			/// value of 0 specifies no change. Positive values specify increased saturation and 
			/// negative values specify decreased saturation.
			/// </summary>
			public int SaturationLevel;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies the change in lightness. A 
			/// value of 0 specifies no change. Positive values specify increased lightness and 
			/// negative values specify decreased lightness.
			/// </summary>
			public int LightnessLevel;
		}

		/// <summary>
		/// Contains members that specify adjustments to the light, midtone, or dark areas of a bitmap.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LevelsParams
		{
			/// <summary>
			/// Integer in the range 0 through 100 that specifies which pixels should be lightened. 
			/// You can use this adjustment to lighten pixels that are already lighter than a certain 
			/// threshold. Setting highlight to 100 specifies no change. Setting highlight to t 
			/// specifies that a color channel value is increased if it is already greater than t 
			/// percent of full intensity. For example, setting highlight to 90 specifies that all 
			/// color channel values greater than 90 percent of full intensity are increased.
			/// </summary>
			public int Highlight;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies how much to lighten or darken 
			/// an image. Color channel values in the middle of the intensity range are altered more 
			/// than color channel values near the minimum or maximum intensity. You can use this 
			/// adjustment to lighten (or darken) an image without loosing the contrast between the 
			/// darkest and lightest portions of the image. A value of 0 specifies no change. 
			/// Positive values specify that the midtones are made lighter, and negative values 
			/// specify that the midtones are made darker.
			/// </summary>
			public int Midtone;

			/// <summary>
			/// Integer in the range 0 through 100 that specifies which pixels should be darkened. 
			/// You can use this adjustment to darken pixels that are already darker than a certain 
			/// threshold. Setting shadow to 0 specifies no change. Setting shadow to t specifies 
			/// that a color channel value is decreased if it is already less than t percent of 
			/// full intensity. For example, setting shadow to 10 specifies that all color channel 
			/// values less than 10 percent of full intensity are decreased.
			/// </summary>
			public int Shadow;
		}

		/// <summary>
		/// Contains members that specify the areas of a bitmap to which a red-eye correction is applied.
		/// </summary>
		/// <remarks>This is the 64-bit version of the structure.</remarks>
		[StructLayout(LayoutKind.Explicit)]
		public struct RedEyeCorrectionParams64Bit
		{
			/// <summary>
			/// Number of areas to filter
			/// </summary>
			[FieldOffset(0)]
			public uint NumberOfAreas;

			/// <summary>
			/// Memory address of RECT structs
			/// </summary>
			/// <remarks>
			/// Must be aligned to 64-bit boundary due to
			/// 64-bit cpu environment and this being a pointer.
			/// Im assuming C++ does this by default.
			/// </remarks>
			[FieldOffset(8)]
			public IntPtr Areas;
		}

		/// <summary>
		/// Contains members that specify the areas of a bitmap to which a red-eye correction is applied.
		/// </summary>
		/// <remarks>This is the 32-bit version of the structure.</remarks>
		[StructLayout(LayoutKind.Explicit)]
		public struct RedEyeCorrectionParams32Bit
		{
			/// <summary>
			/// Number of areas to filter
			/// </summary>
			[FieldOffset(0)]
			public uint NumberOfAreas;

			/// <summary>
			/// Memory address of RECT structs
			/// </summary>
			[FieldOffset(4)]
			public IntPtr Areas;
		}

		/// <summary>
		/// Contains members that specify the nature of a sharpening adjustment to a bitmap.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SharpenParams
		{
			/// <summary>
			/// Real number that specifies the sharpening radius (the radius of the convolution 
			/// kernel) in pixels. The radius must be in the range 0 through 255. As the radius 
			/// increases, more surrounding pixels are involved in calculating the new value of 
			/// a given pixel.
			/// </summary>
			public float Radius;

			/// <summary>
			/// Real number in the range 0 through 100 that specifies the amount of sharpening 
			/// to be applied. A value of 0 specifies no sharpening. As the value of amount 
			/// increases, the sharpness increases.
			/// </summary>
			public float Amount;
		}

		/// <summary>
		/// Contains members that specify the nature of a tint adjustment to a bitmap.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct TintParams
		{
			/// <summary>
			/// From MSDN:
			/// Integer in the range -180 through 180 that specifies the hue to be strengthened 
			/// or weakened. A value of 0 specifies blue. A positive value specifies a clockwise 
			/// angle on the color wheel. For example, positive 60 specifies cyan and positive 
			/// 120 specifies green. A negative value specifies a counter-clockwise angle on the 
			/// color wheel. For example, negative 60 specifies magenta and negative 120 
			/// specifies red.
			/// --- WRONG AGAIN MICROSOFT...
			/// Actual values are:
			/// -180 is cyan. -120 is blue. -60 is magenta. 0 is red. 60 is yellow. 120 is green. 180 is cyan.
			/// One would think they would document this properly.
			/// </summary>
			public int Hue;

			/// <summary>
			/// Integer in the range -100 through 100 that specifies how much the hue (given by 
			/// the hue parameter) is strengthened or weakened. A value of 0 specifies no change. 
			/// Positive values specify that the hue is strengthened and negative values specify 
			/// that the hue is weakened.
			/// </summary>
			public int Amount;
		}
	}
}