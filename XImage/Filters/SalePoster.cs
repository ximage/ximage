using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace XImage.Filters
{
	[Documentation(Text = "Add an overlay to look like a sale poster.")]
	public class SalePoster : IFilter
	{
		private string _message;
		private string _storeName;
		private string _location;

		[Example(QueryString = "?f=saleposter")]
		public SalePoster(string message, string storeName, string location)
		{
			_message = message.Replace('_', ' ');
			_storeName = storeName.Replace('_', ' ');
			_location = location.Replace('_', ' ');
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			response.OutputGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			response.OutputGraphics.SmoothingMode = SmoothingMode.HighQuality;

			var stringFormat = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisWord };
			var coloredBrush = new SolidBrush(Color.FromArgb(172, 0xe8, 0x26, 0x26));
			var whitePen = new Pen(Color.White, 1.6F);

			var coloredBoundary = new Rectangle(320, 320, 300, 300);
			var whiteOutline = Rectangle.Inflate(coloredBoundary, -7, -7);
			var text1Outline = Rectangle.Inflate(whiteOutline, -10, -40);
			text1Outline.Height = 90;
			var text2Outline = new Rectangle(text1Outline.X, 525, text1Outline.Width, 80);

			response.OutputGraphics.FillRectangle(coloredBrush, coloredBoundary);
			response.OutputGraphics.DrawRectangle(whitePen, whiteOutline);
			response.OutputGraphics.DrawLine(whitePen, 369, 491, 565, 491);

			var textPath1 = new GraphicsPath();
			textPath1.AddString(
				_message,
				//new FontFamily("Avenir Next"),
				FontFamily.GenericSansSerif, 
				(int)FontStyle.Bold, 
				38F, 
				text1Outline, 
				stringFormat);
			response.OutputGraphics.FillPath(Brushes.White, textPath1);

			var textPath2 = new GraphicsPath();
			textPath2.AddString(
				"spotted at " + _storeName + " in " + _location, 
				//new FontFamily("Avenir Next"),
				FontFamily.GenericSansSerif, 
				(int)FontStyle.Bold, 
				24F, 
				text2Outline, 
				stringFormat);
			response.OutputGraphics.FillPath(Brushes.White, textPath2);
		}
	}
}