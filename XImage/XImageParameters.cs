using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Web;

namespace XImage
{
	public class Crops
	{
		public const string NONE = "none";
		public const string FILL = "fill";
		public const string STRETCH = "stretch";
		public const string COLOR = "color";
	}

	public class XImageParameters
	{
		static readonly string[] PARAM_ORDER = { "w", "h", "c", "q", "f" };
		static readonly int MAX_IMAGE_WIDTH_OR_HEIGHT = 1000;

		static readonly Dictionary<string, ImageFormat> _supportedFormats = new Dictionary<string, ImageFormat>()
		{
			{ "jpg", ImageFormat.Jpeg },
			{ "gif", ImageFormat.Gif },
			{ "png", ImageFormat.Png },
		};
		static readonly Dictionary<string, ImageFormat> _conentTypeFormats = new Dictionary<string, ImageFormat>()
		{
			{ "image/jpg", ImageFormat.Jpeg },
			{ "image/jpeg", ImageFormat.Jpeg },
			{ "image/gif", ImageFormat.Gif },
			{ "image/png", ImageFormat.Png },
		};

		public int? Width { get; private set; }
		public int? Height { get; private set; }
		public bool AllowUpscaling { get; private set; }
		public string Crop { get; private set; }
		public Color? CropAsColor { get; private set; }
		public int? Quality { get; private set; }
		public bool QualityAsKb { get; private set; }
		public ImageFormat Format { get; private set; }
		public ImageFormat SourceFormat { get; private set; }

		public bool HasAnyValues
		{
			get
			{
				return
					Width != null ||
					Height != null ||
					Crop != null ||
					Quality != null ||
					Format != null;
			}
		}

		public XImageParameters(HttpContext httpContext)
		{
			ParseValues(httpContext);
		}

		private void ParseValues(HttpContext httpContext)
		{
			var uri = httpContext.Request.Url;

			if (httpContext.Request.RawUrl.Contains("?") && uri.Query.IsNullOrEmpty())
				throw new ArgumentException(GetHelp(uri));

			var q = HttpUtility.ParseQueryString(uri.Query);

			bool allowWUpscaling = false, allowHUpscaling = false;
			var w = q["w"];
			if (w != null)
			{
				allowWUpscaling = w.EndsWith("!");
				if (allowWUpscaling == true)
					w = w.Substring(0, w.Length - 1);
				Width = w.AsNullableInt();
				if (Width == null || Width <= 0)
					ThrowArgumentException(uri, "Width must be a positive integer.");
				if (Width > MAX_IMAGE_WIDTH_OR_HEIGHT)
					ThrowArgumentException(uri, "Cannot request a width larger than the max configured value of {0}.", MAX_IMAGE_WIDTH_OR_HEIGHT);
			}

			var h = q["h"];
			if (h != null)
			{
				allowHUpscaling = h.EndsWith("!");
				if (allowHUpscaling == true)
					h = h.Substring(0, h.Length - 1);
				Height = h.AsNullableInt();
				if (Height == null || Height <= 0)
					ThrowArgumentException(uri, "Height must be a positive integer.");
				if (Height > MAX_IMAGE_WIDTH_OR_HEIGHT)
					ThrowArgumentException(uri, "Cannot request a height larger than max configured value of {0}.", MAX_IMAGE_WIDTH_OR_HEIGHT);
			}

			if (Width != null && Height != null && allowWUpscaling != allowHUpscaling)
				ThrowArgumentException(uri, "If upscaling '!' is enabled and both w and h are specified, the '!' must be used on both.  Enforcing this strictly helps optimize cache hit ratios.");
			AllowUpscaling = allowWUpscaling || allowHUpscaling;

			var c = q["c"];
			if (c != null)
			{
				if (c == Crops.NONE)
					ThrowArgumentException(uri, "It isn't necessary to spcify a crop of 'none'.  Just leave it off.  Enforcing this strictly helps optimize cache hit ratios.");
				else if (c == Crops.FILL || c == Crops.STRETCH)
					Crop = c;
				else
				{
					if (c.Length != 6 && (c.Length != 8 || c.StartsWith("ff")))
						ThrowArgumentException(uri, "When specifying a color for crop (c), it must be a valid 6 digit hex number (or an 8 digit hex number for translucency).  Enforcing this strictly helps optimize cache hit ratios.");
					try
					{
						CropAsColor = ColorTranslator.FromHtml("#" + c);
						Crop = Crops.COLOR;
						if (c != c.ToLower())
							ThrowArgumentException(uri, "When specifying a color for crop (c), do not use capital letters.");
					}
					catch
					{
						ThrowArgumentException(uri, "When specifying a color for crop (c), it must be a valid 6 digit hex number (or an 8 digit hex number for translucency).  Enforcing this strictly helps optimize cache hit ratios.");
					}
				}

				if (Crop == null)
					ThrowArgumentException(uri, "The only valid options for crop are (none), fill, stretch or a hex color.  See below for more details.");

				if ((Width == null || Height == null) && Crop != null)
					ThrowArgumentException(uri, "A cropping mode is only valid when both width and height are specified.");
			}

			var quality = q["q"];
			if (quality != null)
			{
				QualityAsKb = quality.EndsWith("kb");
				if (QualityAsKb)
					quality = quality.Substring(0, quality.Length - 2);
				Quality = quality.AsNullableInt();
				if (Quality == null)
					ThrowArgumentException(uri, "Quality must be an integer between 1 and 100 or a size in kb, e.g. 150kb.");
				if (Quality <= 0)
					ThrowArgumentException(uri, "Quality must be an integer between 1 and 100.");
				if (!QualityAsKb && Quality > 100)
					ThrowArgumentException(uri, "Quality must be an integer between 1 and 100 unless you are specifing a content size, e.g. 150kb.");
			}

			var f = q["f"];
			if (f != null)
			{
				Format = _supportedFormats.GetValueOrDefault(f);
				if (Format == null)
					ThrowArgumentException(uri, "Format must be either jpg, gif or png.");
				foreach (var format in _supportedFormats.Keys)
					if (uri.AbsolutePath.EndsWith("." + format, StringComparison.OrdinalIgnoreCase) && f == format)
						ThrowArgumentException(uri, "If the source image is a {0}, don't specify f={0}.  Enforcing this strictly helps optimize cache hit ratios.", format);
			}
			SourceFormat = _conentTypeFormats.GetValueOrDefault(httpContext.Response.ContentType);
			if (SourceFormat == null)
				ThrowArgumentException(uri, "Unrecognized content-type.");

			var requestedOrder = q.AllKeys.Where(k => PARAM_ORDER.Contains(k)).ToArray();
			var correctOrder = PARAM_ORDER.Where(p => requestedOrder.Contains(p)).ToArray();
			if (string.Concat(requestedOrder) != string.Concat(correctOrder))
				ThrowArgumentException(uri, "Each parameter is optional.  But they must appear in the order of w, h, c, q, f. Enforcing this strictly helps optimize cache hit ratios.");
		}

		public string GetContentType()
		{
			return "image/" + (Format ?? SourceFormat).ToString().ToLower();
		}

		private void ThrowArgumentException(Uri uri, string message, params object[] args)
		{
			var sb = new StringBuilder();
			sb.AppendLine("ERROR");
			sb.AppendLine("-----");
			sb.AppendLine(string.Format(message, args));
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine(GetHelp(uri));
			throw new ArgumentException(sb.ToString());
		}

		private static string GetHelp(Uri uri)
		{
			var help = string.Format(
@"____  ___.___                               
\   \/  /|   | _____ _____     ____   ____  
 \     / |   |/     \\__  \   / ___\_/ __ \ 
 /     \ |   |  Y Y  \/ __ \_/ /_/  >  ___/ 
/___/\  \|___|__|_|  (____  /\___  / \___  >  by Rylan Barnes.  See <a href=""http://ximage.co"">http://ximage.co</a> for full docs and demos.
      \_/          \/     \//_____/      \/ 
XImage (Extensible Image) is an open technology for real-time image manipulation over HTTP.  It defines a standard 
interface for performing common image operations such as resizing, cropping, compressing, filtering, etc.  
In addition, it describes how to add extended functionality that might be custom for your use case in a manner 
that is discoverable and reusable.  

The X image operates at the server level it can be made to operate at application level if desired
Developing for multiple 
The goal of the XImage interface is to simplify the development of of apps
that span across devices with a wide range of screen depths and sizes and varyingly 

QUERY STRING
------------
To maximize CDN and browser cache hit ratios, the following arguments are designed to have a strict set of rules.  
Therefore, capitalization and parameter order are important and superfluous parameters will be treated as errors. 

  ?          Help.  Include a '?' with no parameters to show this help screen.  Each implementaion of XImage 
             must show this help screen which is primarily used to show which features it chose to support or extend.
  w          Width of the image.  If h is not supplied, it will be inferred proportionally.
             e.g. <a href=""{0}?w=100"">{0}?w=100</a>
  h          Height of the image.  If w is not supplied, it will be inferred proportionally.
             e.g. <a href=""{0}?h=100"">{0}?h=100</a>
  !          Allow upscaling.  By default, the image will not be resized larger than it's original dimensions.
             To override this, append '!' to the end of the width or height.
             e.g. <a href=""{0}?w=1000!&h=1000!"">{0}?w=1000!&h=1000!</a>
  c          Cropping mode.  This is only valid when both w and h are specified.  Available options are:
                (none)       Don't actually supply 'none', just leave it off.  No parts of the image will be cropped.
                             It will be resized proportionally until it fits completely within the w x h boundaries.
                             This will likely result in either width or height being smaller than the requested value.
                             e.g. <a href=""{0}?w=200&h=200"">{0}?w=200&h=200</a>
                fill         The image will be resized proportionally until it completely fills the w x h boundares.
                             This will likely result in cropping off either top/bottom edges or left/right edges
                             but the resulting dimensions will be exactly w x h.
                             e.g. <a href=""{0}?w=200&h=100&c=fill"">{0}?w=200&h=100&c=fill</a>
                stretch      Each edge will be resized disproportionally until it reaches its w x h boundaries.
                             Results are exactly w x h and no edges are cropped but the image will appear distorted.
                             e.g. <a href=""{0}?w=200&h=100&c=stretch"">{0}?w=200&h=100&c=stretch</a>
                (color)      It will be resized proportionally until it fits completely within the w x h boundaries.
                             This will likely result in void space on the left/right or top/bottom which will be 
                             filled with the supplied color.  Must be a 6 or 8 digit lower case hex, don't use #.
                             e.g. <a href=""{0}?w=200&h=100&c=3b5999"">{0}?w=200&h=100&c=3b5999</a>
                extend...    XImage is an extensible protocol.  Other XImage implementations are free to add additional 
                             cropping modes.  These implementations must make their functionality discoverable by 
                             appending the name to the bottom of this sub-list.  
  m          Apply mask...
  f          Apply a filter.
  q          Quality of the image.  Values, as percentages, must be an integer between 1 - 100.  
             Alternatively, you can specify a max content size (in kilobytes) by appending 'kb', e.g. q=150kb.
             However some file sizes are impossibly small for some formats which will result in a best effort size.
             e.g. <a href=""{0}?q=5"">{0}?q=5</a> or  <a href=""{0}?q=5kb"">{0}?q=5kb</a>
  f          Format of the image.  Supported values are jpg, gif, png.
             e.g. <a href=""{0}?w=200&f=gif"">{0}?w=200&f=gif</a>
  extend...  XImage is an extensible protocol.  Other XImage implementations are free to add additional features.  These 
             implementations must make their functionality discoverable by appending the name to the bottom of this list.

RESPONSE HEADERS
----------------
XImage uses reponse headers to specify suported capabilities and also to communicate meta data about the image.
The meta data, in paticular, can become useful with a HEAD request especially when used in conjuction with a CDN for 
fast, single-digit millisecond response times carrying rich details about that image.

  X-Image: [?] [w=1-1000(!)] [h=1-1000(!)] [c=(none)|fill|stretch|(color)] [q=1-100|(size)kb] [f=jpg|gif|png]
  X-Image-Original-Width: 500
  X-Image-Original-Height: 600
  X-Image-Original-Format: image/png
  X-Image-Width: 100
  X-Image-Height: 200
  X-Image-Color-Accent: #ff0000
  X-Image-Color-Average: #336699
  X-Image-Color-Base: #555555
  X-Image-Color-Dominant: #6699cc
  x-Image-Color-Palette: #111111,#222222,#333333,#444444,#555555,#666666
",
															uri.Segments.Last());
			return help;
		}
	}
}