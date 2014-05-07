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
	[Documentation(Text = "Masks the current image with the specified bitmap.")]
	public class Mask : IFilter
	{
		Uri _uri;

		[Example(QueryString = "?w=100&f=mask(url(mask.png))")]
		public Mask(Uri uri)
		{
			_uri = uri;
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			// Unless explicitly requested by the user, default to PNG for this filter.
			if (request.IsOutputImplicitlySet)
			{
				request.Outputs.RemoveAll(o => o.ContentType.StartsWith("image"));
				request.Outputs.Add(new Outputs.Png());
			}
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			// TODO: async/await here OR (even better) fetch these somewhere else.

			var maskRequest = HttpWebRequest.CreateHttp(_uri);
			var maskResponse = Bitmap.FromStream(maskRequest.GetResponse().GetResponseStream()) as Bitmap;

			int w = response.OutputImage.Width, h = response.OutputImage.Height;
			using (var mask = new Bitmap(w, h, PixelFormat.Format32bppArgb))
			{
				using (var graphics = Graphics.FromImage(mask))
				{
					graphics.DrawImage(maskResponse, new Rectangle(0, 0, w, h));
				}

				response.OutputImage.ApplyMask(mask, !request.Outputs.Exists(o => o.SupportsTransparency));
			}
		}
	}
}