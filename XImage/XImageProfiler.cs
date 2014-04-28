using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XImage
{
	public class XImageProfiler
	{
		Stopwatch _stopwatch = Stopwatch.StartNew();
		NameValueCollection _properties;

		public List<Tuple<string, long>> Markers { get; private set; }

		public XImageProfiler(NameValueCollection properties)
		{
			_properties = properties;
			Markers = new List<Tuple<string, long>>();
			Mark(""); // Gotta have a starting point.
		}

		public void Mark(string name)
		{
			Markers.Add(new Tuple<string, long>(name, _stopwatch.ElapsedTicks));
		}

		public IDisposable Measure(string name)
		{
			var startTimestamp = _stopwatch.ElapsedTicks;
			return new BlockEndAction(() =>
			{
				var endTimestamp = _stopwatch.ElapsedTicks;
				
				//Markers.Add(new Tuple<string, long>(name, endTimestamp));

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
