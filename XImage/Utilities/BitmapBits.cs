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
		BitmapData _bitmapData;
		bool _writeAccess;

		public byte[] Data { get; private set; }

		public BitmapBits(Bitmap bitmap, bool writeAccess = false)
		{
			_writeAccess = writeAccess;
			_bitmap = bitmap;
			_bitmapData = _bitmap.LockBits(
				rect: new Rectangle(Point.Empty, _bitmap.Size), 
				flags: _writeAccess ? ImageLockMode.ReadWrite : ImageLockMode.ReadOnly,
				format:_bitmap.PixelFormat);
			Data = new byte[_bitmapData.Stride * _bitmap.Height];
			Marshal.Copy(_bitmapData.Scan0, Data, 0, Data.Length);
		}

		public void Dispose()
		{
			if (_bitmapData != null && _bitmap != null)
			{
				if (_writeAccess)
					Marshal.Copy(Data, 0, _bitmapData.Scan0, Data.Length);

				_bitmap.UnlockBits(_bitmapData);
			}
			_bitmapData = null;
			_bitmap = null;
			Data = null;
		}
	}
}