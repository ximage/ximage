using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XImage.Filters
{
	public class Grayscale : IFilter
	{
		decimal _amount;

		public string Documentation
		{
			get { return "Desaturates the colors"; }
		}

		public Grayscale()
			: this(1)
		{
		}

		public Grayscale(int amount)
			: this((decimal)amount)
		{
		}

		public Grayscale(decimal amount)
		{
			_amount = amount;

			if (_amount > 1 || _amount < 0)
				throw new ArgumentException("The grayscale amount must be between 0 and 1.");
		}

		public void ProcessImage(XImageRequest request, XImageResponse response, byte[] data)
		{
			if (_amount == 0)
			{
				// Do nothing.
			}
			if (_amount == 1) 
			{
				// Faster than the block below.
				for (int i = 0; i < data.Length; i += 4)
					data[i] = data[i + 1] = data[i + 2] = (byte)((data[i] + data[i + 1] + data[i + 2]) / 3);
			}
			else
			{
				int avg;
				float a = (float)_amount;
				for (int i = 0; i < data.Length; i += 4)
				{
					avg = (data[i] + data[i + 1] + data[i + 2]) / 3;
					data[i] -= (byte)((data[i] - avg) * a);
					data[i + 1] -= (byte)((data[i + 1] - avg) * a);
					data[i + 2] -= (byte)((data[i + 2] - avg) * a);
				}
			}
		}
	}
}