using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace System.Drawing
{
	internal class GdiPlusEffect
	{
		Guid _guid;
		object _args;
		IntPtr _nativeHandle = IntPtr.Zero;

		public GdiPlusEffect(string guid, object args)
		{
			_guid = new Guid(guid);
			_args = args;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal IntPtr NativeHandle()
		{
			if (_nativeHandle == IntPtr.Zero)
			{
				var status = GdiPlusInterop.GdipCreateEffect(_guid, out _nativeHandle);
				//Utils10.CheckErrorStatus(liStatus);

				try
				{
					var size = Marshal.SizeOf(_args);
					var ptrArgs = Marshal.AllocHGlobal(size);

					try
					{
						Marshal.StructureToPtr(_args, ptrArgs, true);
						status = GdiPlusInterop.GdipSetEffectParameters(_nativeHandle, ptrArgs, (uint)size);
						//Utils10.CheckErrorStatus(status);
					}
					finally
					{
						Marshal.FreeHGlobal(ptrArgs);
					}
				}
				finally
				{
				}
			}

			return _nativeHandle;
		}

	}
}