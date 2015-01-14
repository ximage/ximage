using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net;
using XImage.Utilities;

namespace XImage.Filters
{
	[Documentation(Text = "Overlay the sale poster in the corner of the product image.")]
	public class PinterestSale : IFilter
	{
		const int CANVAS_WIDTH = 736;
		const int FOOTER_HEIGHT = 150;
		const int AVATAR_MARGINS = 30;
		const int AVATAR_SIZE = 90;
		const int POSTER_SIZE = 330;
		const int POSTER_MARGINS = 18;

		Uri _salePoster;
		Uri _storeAvatar;
		string _storeName;

		[Example(QueryString = "?f=pinterestsale(...,...,...)")]
		public PinterestSale(string storeAvatar, string storeName)
		{
			_salePoster = null;
			_storeAvatar = new Uri(storeAvatar);
			_storeName = storeName.Replace("_", " ");
		}

		[Example(QueryString = "?f=pinterestsale(...,...,...)")]
		public PinterestSale(string salePoster, string storeAvatar, string storeName)
		{
			_salePoster = new Uri(salePoster);
			_storeAvatar = new Uri(storeAvatar);
			_storeName = storeName.Replace("_", " ");
		}

		public void PreProcess(XImageRequest request, XImageResponse response)
		{
			var productSize = response.InputImage.Size.ScaleToWidth(CANVAS_WIDTH);

			response.CanvasSize = new Size(productSize.Width, productSize.Height + FOOTER_HEIGHT);
		}

		public void PostProcess(XImageRequest request, XImageResponse response)
		{
			// TODO: async/await here OR (even better) fetch these somewhere else.

			var storeAvatarImage = Bitmap.FromStream(
				HttpWebRequest.CreateHttp(_storeAvatar)
				.GetResponse()
				.GetResponseStream()) as Bitmap;

			var productSize = response.InputImage.Size.ScaleToWidth(CANVAS_WIDTH);
			var avatarBounds = new Rectangle(AVATAR_MARGINS, productSize.Height + AVATAR_MARGINS, AVATAR_SIZE, AVATAR_SIZE);
			var avatarMask = new GraphicsPath();
			avatarMask.AddEllipse(avatarBounds.X, avatarBounds.Y, avatarBounds.Width - 1, avatarBounds.Height - 1);

			response.OutputGraphics.Clear(Color.White);

			response.OutputGraphics.DrawImage(storeAvatarImage, avatarBounds);
			response.OutputImage.ApplyMask(avatarMask, Brushes.White, true);

			response.OutputGraphics.DrawImage(response.InputImage, new Rectangle(Point.Empty, productSize));
			
			response.OutputGraphics.DrawString(
				"SALE",
				new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold),
				new SolidBrush(Color.FromArgb(113, 113, 113)),
				new Point(avatarBounds.Right + 7, avatarBounds.Top + 7));

			response.OutputGraphics.DrawString(
				_storeName,
				new Font(FontFamily.GenericSansSerif, 22, FontStyle.Regular),
				new SolidBrush(Color.FromArgb(183, 183, 183)),
				new Point(avatarBounds.Right + 7, avatarBounds.Top + 7 + AVATAR_SIZE / 2));

			if (_salePoster != null)
				DrawSalePoster(response, _salePoster, productSize);
		}

		private void DrawSalePoster(XImageResponse response, Uri salePoster, Size productSize)
		{
			if (salePoster == null)
				return;

			var salePosterImage = Bitmap.FromStream(
				HttpWebRequest.CreateHttp(salePoster)
				.GetResponse()
				.GetResponseStream()) as Bitmap;

			var salePosterSize = salePosterImage.Size.ScaleToWidth(POSTER_SIZE);
			if (salePosterSize.Height > salePosterSize.Width)
				salePosterSize = salePosterImage.Size.ScaleToHeight(POSTER_SIZE);
			var salePosterBounds = new Rectangle(
				productSize.Width - salePosterSize.Width - POSTER_MARGINS,
				productSize.Height + FOOTER_HEIGHT - salePosterSize.Height - POSTER_MARGINS,
				salePosterSize.Width,
				salePosterSize.Height);
			var salePosterBorder = Rectangle.Inflate(salePosterBounds, POSTER_MARGINS, POSTER_MARGINS);

			response.OutputGraphics.FillRectangle(Brushes.White, salePosterBorder);
			response.OutputGraphics.DrawImage(salePosterImage, salePosterBounds);
		}
	}
}