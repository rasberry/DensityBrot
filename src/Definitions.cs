using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface ISaveable
	{
		long ToStream(Stream writeStream);
	}

	public enum Planes : int { ZrZi=0, ZrCr=1, ZrCi=2, ZiCr=3, ZiCi=4, CrCi=5 }

	public enum ColorComponent { None = 0, R, G ,B, A }

	public enum Axes : int { /*X*/ Zr=0, /*Y*/ Zi=1, /*W*/ Cr=2, /*Z*/ Ci=3 }

	//abstracting color in case someday i want to use a higer bit depth than 8bpp
	public struct ColorD
	{
		public double R;
		public double G;
		public double B;
		public double A;

		ColorD(double a, double r, double g, double b)
		{
			A = ColorHelpers.Clamp(a);
			R = ColorHelpers.Clamp(r);
			G = ColorHelpers.Clamp(g);
			B = ColorHelpers.Clamp(b);
		}

		public Color ToColor()
		{
			return Color.FromArgb((int)(255*A),(int)(255*R),(int)(255*G),(int)(255*B));
		}
		
		public MagickColor ToMagickColor()
		{
			var m = new MagickColor((byte)(255*A),(byte)(255*R),(byte)(255*G),(byte)(255*B));
			return m;
		}

		public double GetComponent(ColorComponent comp)
		{
			switch(comp)
			{
			case ColorComponent.A: return A;
			case ColorComponent.R: return R;
			case ColorComponent.G: return G;
			case ColorComponent.B: return B;
			}
			return 0.0;
		}

		public static ColorD FromArgb(double a,double r, double g, double b)
		{
			return new ColorD(a,r,g,b);
		}
		public static ColorD FromColor(Color c)
		{
			return new ColorD(c.A/255.0,c.R/255.0,c.G/255.0,c.B/255.0);
		}
	}
}
