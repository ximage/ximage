using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace XImage.Utilities
{
	/// <summary>
	/// Exposes a BitmapData and a managed, lazy copied, byte array to work on.  
	/// Optionally will auto-copy modified bytes back to the image on dispose.
	/// </summary>
	public class BitmapBits : IDisposable
	{
		Bitmap _bitmap;
		bool _writeAccess;

		byte[] _data = null;
		public byte[] Data
		{
			get
			{
				if (_data == null)
				{
					_data = new byte[BitmapData.Stride * _bitmap.Height];
					Marshal.Copy(BitmapData.Scan0, _data, 0, Data.Length);
				}
				return _data;
			}
		}
		public BitmapData BitmapData { get; private set; }

		public BitmapBits(Bitmap bitmap, bool writeAccess = false)
		{
			_writeAccess = writeAccess;
			_bitmap = bitmap;
			BitmapData = _bitmap.LockBits(
				rect: new Rectangle(Point.Empty, _bitmap.Size), 
				flags: _writeAccess ? ImageLockMode.ReadWrite : ImageLockMode.ReadOnly,
				format: _bitmap.PixelFormat);
		}

		public void Dispose()
		{
			if (BitmapData != null && _bitmap != null)
			{
				// Copies the data back.
				if (_writeAccess && _data != null)
					Marshal.Copy(Data, 0, BitmapData.Scan0, Data.Length);

				_bitmap.UnlockBits(BitmapData);
			}
			BitmapData = null;
			_bitmap = null;
			_data = null;
		}
	}
}