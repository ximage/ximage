using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XImage
{
	public class XImageDiagnostics
	{
		NameValueCollection _properties;

		public XImageDiagnostics(NameValueCollection properties)
		{
			_properties = properties;
		}

		public IDisposable Measure(string name)
		{
			var stopwatch = Stopwatch.StartNew();
			var startTimestamp = stopwatch.ElapsedTicks;
			return new BlockEndAction(() =>
			{
				var endTimestamp = stopwatch.ElapsedTicks;
				_properties.Add(
					name, 
					string.Format(
						"{0:N2}ms",
						1000D * (double)(endTimestamp - startTimestamp) / (double)Stopwatch.Frequency));
			});
		}

		class BlockEndAction : IDisposable
		{
			Action _action;

			public BlockEndAction(Action action)
			{
				_action = action;
			}

			public void Dispose()
			{
				_action();
			}
		}
	}
}
