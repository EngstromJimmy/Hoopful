using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Antropoid.Drawing
{
	/// <summary>
	/// Summary description for Image.
	/// </summary>
	public class Image
	{
		public static Bitmap Tile(Bitmap source,int width,int height)
		{
			Bitmap b=new Bitmap(width,height);
			Graphics g  = Graphics.FromImage(b);
			TextureBrush tBrush = new TextureBrush(source);
			g.FillRectangle(tBrush, new Rectangle(0, 0, width, height));

			return b;
		}

 
		public static System.Drawing.Image AddText(System.Drawing.Image source,string text)
		{
			Graphics g  = Graphics.FromImage(source);
			FontFamily fontFamily = new FontFamily("Verdana");
			Font font = new Font(fontFamily, 10, FontStyle.Bold, GraphicsUnit.Pixel);
			PointF pointF = new PointF(30, 10);
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(0, 0,0));
			g.DrawString(text, font, solidBrush, pointF);
			return source;
		}

		public static Bitmap Crop(Bitmap Source, int x, int y, int width,int height )

		{
			Bitmap Cropped = new Bitmap(width, height);
			Graphics g  = Graphics.FromImage(Cropped);
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			Rectangle rect = new Rectangle(0, 0, width, height);
			g.DrawImage(Source, rect, x, y, width, height, GraphicsUnit.Pixel);
			return Cropped;
		}

	}
}
