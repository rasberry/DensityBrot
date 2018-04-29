using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public enum Planes { XY, XW, XZ, YW, YZ, WZ }

	public class FractalConfig
	{
		public double X,Y,W,Z;
		public Planes Plane;
		public double Resolution;
		public double Escape;
		public int IterMax;
		public double OffsetX;
		public double OffsetY;
		public bool HideEscaped;
		public bool HideContained;
		public int SamplesPerPoint;

		public static FractalConfig Default { get {
			return new FractalConfig {
				X = 0.0,Y = 0.0,
				W = 0.0,Z = 0.0,
				Plane = Planes.XY,
				Resolution = 20.0,
				Escape = 4.0,
				IterMax = 100,
				HideContained = false,
				HideEscaped = false,
				SamplesPerPoint = 10
			};
		}}
	}

	public enum ColorComponent { None = 0, R, G ,B, A }

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
		//public MagickColor ToMagickColor()
		//{
		//	//var m = new MagickColor();
		//	//m.r
		//}

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
