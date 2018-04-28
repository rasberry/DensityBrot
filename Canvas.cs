using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface ICanvas : IDisposable
	{
		int Width { get; }
		int Height { get; }
		void SetPixel(int x,int y,ColorD c);
		ColorD GetPixel(int x,int y);
		void SavePng(string fileName);
	}

	public class MagicCanvas : ICanvas
	{
		public MagicCanvas(int width,int height)
		{
			bitmap = new MagickImage(MagickColors.Transparent,width,height);
			bitmap.ColorType = ColorType.TrueColorAlpha;
			bitmap.Alpha(AlphaOption.Transparent);
			bitmap.ColorAlpha(MagickColors.Transparent);
		}

		public int Width { get { return bitmap.Width; } }
		public int Height { get { return bitmap.Height; } }

		public ColorD GetPixel(int x, int y)
		{
			var rgba = Pixels.GetArea(x,y,1,1);
			Color sdc = Color.FromArgb(rgba[0],rgba[1],rgba[2],rgba[3]);
			return ColorD.FromColor(sdc);
		}

		public void SetPixel(int x, int y, ColorD c)
		{
			Color sdc = c.ToColor();
			var rgba = new byte[] { sdc.R, sdc.G, sdc.B, sdc.A };
			Pixels.SetArea(x,y,1,1,rgba);
		}

		public void DrawLine(ColorD color,double x1, double y1,double x2, double y2)
		{
			Draws.FillColor(color.ToColor());
			Draws.Line(x1,y1,x2,y2);
		}

		public void SavePng(string fileName)
		{
			if (_draws != null) {
				bitmap.Draw(_draws);
			}

			bitmap.Format = MagickFormat.Png32;
			bitmap.Write(fileName);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool dispose)
		{
			if (dispose) {
				if (_pixels != null) {
					_pixels.Dispose();
				}
				if (bitmap != null) {
					bitmap.Dispose();
				}
			}
		}

		IPixelCollection Pixels { get {
			if (_pixels == null) {
				_pixels = bitmap.GetPixels();
			}
			return _pixels;
		}}

		Drawables Draws { get {
			if (_draws == null) {
				_draws = new Drawables();
			}
			return _draws;
		}}

		MagickImage bitmap;
		IPixelCollection _pixels;
		Drawables _draws;
	}
}
