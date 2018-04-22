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
	public interface ICanvas
	{
		int Width { get; }
		int Height { get; }
		void SetPixel(int x,int y,Color c);
		Color GetPixel(int x,int y);
		Bitmap Source { get; }
		void SavePng(string fileName);
	}

	#if false
	public class BitmapCanvas : ICanvas, IDisposable
	{
		public BitmapCanvas(int width,int height)
		{
			bitmap = new Bitmap(width,height,PixelFormat.Format32bppArgb);
			fast = new FastBitmap.LockBitmap(bitmap);
		}

		public int Width { get { return bitmap.Width; } }
		public int Height { get { return bitmap.Height; } }

		public void SetPixel(int x, int y, Color c)
		{
			fast.LockBits();
			fast.SetPixel(x,y,c);
		}

		public Color GetPixel(int x, int y)
		{
			fast.LockBits();
			return fast.GetPixel(x,y);
		}

		public Bitmap Source { get {
			fast.UnlockBits();
			return bitmap;
		} }

		public void SavePng(string filename)
		{
			fast.UnlockBits();
			bitmap.Save(filename,ImageFormat.Png);
		}

		public void Dispose()
		{
			fast.Dispose();
		}

		Bitmap bitmap;
		FastBitmap.LockBitmap fast;
	}
	#endif

	public class MagicCanvas : ICanvas
	{
		public MagicCanvas(int width,int height)
		{
			bitmap = new MagickImage(MagickColor.FromRgba(0,0,0,0),width,height);
			pixels = bitmap.GetPixels();
		}

		public int Width { get { return bitmap.Width; } }
		public int Height { get { return bitmap.Height; } }

		public Bitmap Source { get { return bitmap.ToBitmap(); } }

		public Color GetPixel(int x, int y)
		{
			var rgba = pixels.GetArea(x,y,1,1);
			return Color.FromArgb(rgba[0],rgba[1],rgba[2],rgba[3]);
		}

		public void SetPixel(int x, int y, Color c)
		{
			var rgba = new byte[] { c.R, c.G, c.B, c.A };
			pixels.SetArea(x,y,1,1,rgba);
		}

		public void SavePng(string fileName)
		{
			bitmap.Format = MagickFormat.Png;
			bitmap.Write(fileName);
		}

		MagickImage bitmap;
		IPixelCollection pixels;
	}
}
