using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Web;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Draws an image over the current one.")]
	public class Layer : IFilter
	{
		Uri _uri;

		[Example(QueryString = "?w=100&f=layer(url(overlay.png))")]
		public Layer(Uri uri)
		{
			_uri = uri;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			// Unless explicitly requested by the user, default to PNG for this filter.
			if (request.IsOutputImplicitlySet)
				request.Output = new Outputs.Png();
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			// TODO: async/await here OR (even better) fetch these somewhere else.

			var layerRequest = HttpWebRequest.CreateHttp(_uri);
			var layerResponse = Bitmap.FromStream(layerRequest.GetResponse().GetResponseStream()) as Bitmap;

			int w = response.OutputImage.Width, h = response.OutputImage.Height;
			using (var layer = new Bitmap(w, h, PixelFormat.Format32bppArgb))
			{
				using (var graphics = Graphics.FromImage(layer))
				{
					graphics.DrawImage(layerResponse, new Rectangle(0, 0, w, h));
				}

				response.OutputGraphics.DrawImage(layer, new Rectangle(w / -2, h / -2, w, h));
			}
		}
	}
}