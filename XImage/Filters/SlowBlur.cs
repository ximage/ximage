using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	public class SlowBlur : IFilter
	{
		public string Documentation
		{
			get { return "Applies a gaussian blur."; }
		}

		public int Radius { get; set; }

		public SlowBlur() : this(10) { }

		public SlowBlur(int radius)
		{
			Radius = radius / 2 * 2 + 1;
		}

		public void ProcessImage(XImageRequest request, XImageResponse response)
		{
			using (var bitmapBits = response.OutputImage.GetBitmapBits(true))
			{
				var filterMatrix = CalculateFilterMatrix(Radius, Radius);

				ConvolutionFilter(request, response, bitmapBits.Data, filterMatrix);
			}
		}

		static float[,] CalculateFilterMatrix(int length, float weight) 
		{
			float[,] kernel = new float[length, length]; 
			float sumTotal = 0; 
 
			int kernelRadius = length / 2; 
			float distance = 0; 
 
			float calculatedEuler = 1F / (2F * (float)Math.PI * (float)Math.Pow(weight, 2)); 
 
			for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++) 
			{ 
				for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++) 
				{ 
					distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (weight * weight)); 
 
					kernel[filterY + kernelRadius, filterX + kernelRadius] = calculatedEuler * (float)Math.Exp(-distance); 
 
					sumTotal += kernel[filterY + kernelRadius, filterX + kernelRadius]; 
				} 
			} 
 
			for (int y = 0; y < length; y++) 
			{ 
				for (int x = 0; x < length; x++) 
				{ 
					kernel[y, x] = kernel[y, x] * (1F / sumTotal); 
				} 
			} 
 
			return kernel; 
		}

		static void ConvolutionFilter(XImageRequest request, XImageResponse response, byte[] data, float[,] filterMatrix)
		{
			byte[] buffer = new byte[data.Length];
			Array.Copy(data, buffer, data.Length);

			int imageWidth = response.OutputImage.Width,
				imageHeight = response.OutputImage.Height,
				imageStride = imageWidth * 4, // Note: This only works for images with 0 extra stride.
				filterWidth = filterMatrix.GetLength(1),
				filterHeight = filterMatrix.GetLength(0),
				filterOffset = (filterWidth - 1) / 2,
				calcOffset = 0,
				byteOffset = 0;

			float factor = 1F / Enumerable
									.Range(0, filterWidth)
									.SelectMany(i => Enumerable
										.Range(0, filterHeight)
										.Select(j => filterMatrix[i, j])).Sum();
			float b = 0, g = 0, r = 0;

			for (int offsetY = filterOffset; offsetY < imageHeight - filterOffset; offsetY++)
			{
				for (int offsetX = filterOffset; offsetX < imageWidth - filterOffset; offsetX++)
				{
					b = g = r = 0;

					byteOffset = offsetY * imageStride + offsetX * 4;

					for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
					{
						for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
						{
							calcOffset = byteOffset + (filterX * 4) + (filterY * imageStride);

							b += (float)(buffer[calcOffset]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
							g += (float)(buffer[calcOffset + 1]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
							r += (float)(buffer[calcOffset + 2]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
						}
					}

					b = factor * b;
					g = factor * g;
					r = factor * r;

					b = (b > 255 ? 255 : (b < 0 ? 0 : b));
					g = (g > 255 ? 255 : (g < 0 ? 0 : g));
					r = (r > 255 ? 255 : (r < 0 ? 0 : r));

					data[byteOffset] = (byte)(b);
					data[byteOffset + 1] = (byte)(g);
					data[byteOffset + 2] = (byte)(r);
				}
			}
		}
	}
}