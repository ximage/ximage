using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace XImage.Utilities
{
	public class BitmapBits : IDisposable
	{
		Bitmap _bitmap;
		bool _writeAccess;

		public byte[] Data { get; private set; }
		public BitmapData BitmapData { get; private set; }

		public BitmapBits(Bitmap bitmap, bool writeAccess = false)
		{
			_writeAccess = writeAccess;
			_bitmap = bitmap;
			BitmapData = _bitmap.LockBits(
				rect: new Rectangle(Point.Empty, _bitmap.Size), 
				flags: _writeAccess ? ImageLockMode.ReadWrite : ImageLockMode.ReadOnly,
				format:_bitmap.PixelFormat);
			Data = new byte[BitmapData.Stride * _bitmap.Height];
			Marshal.Copy(BitmapData.Scan0, Data, 0, Data.Length);
		}

		public void Dispose()
		{
			if (BitmapData != null && _bitmap != null)
			{
				if (_writeAccess)
					Marshal.Copy(Data, 0, BitmapData.Scan0, Data.Length);

				_bitmap.UnlockBits(BitmapData);
			}
			BitmapData = null;
			_bitmap = null;
			Data = null;
		}
	}
}