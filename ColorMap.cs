using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IColorMap
	{
		Color GetColor(long index, long maximum);
	}

	public class GrayColorMap : IColorMap
	{
		public Color GetColor(long index, long maximum)
		{
			double li = Math.Max(0,Math.Log(index));
			double lm = Math.Max(0,Math.Log(maximum));

			double pct = li / lm;
			int gray = (int)Math.Min(255.0,pct * 256.0);
			return Color.FromArgb(gray,gray,gray);
		}
	}

	public class FullRangeRGBColorMap : IColorMap
	{
		public Color GetColor(long index, long maximum)
		{
			return FindColorFromRange(0,
				Math.Max(0,Math.Log(maximum)),
				Math.Max(0,Math.Log(index))
			);
		}

		//iterate HSL L=[0 to 1] S=1 H[0 to 360]
		static Color FindColorFromRange(double min, double max, double val)
		{
			double range = max - min;
			double spacemax = 256.0 * 360.0; //L * H
			double pos = (val - min) / range * spacemax;
	
			//basically map 2D HxL space into one dimension
			double s = 1.0;
			double l = (pos / spacemax) * 0.9 + 0.05; //clip the edges by 5%
			double h = (pos % (360.0 * 4)) / (360.0 * 4); //4 slows down the cycle 4x
	
			return HSLToRGB(h,s,l);
		}

		//https://stackoverflow.com/questions/2353211/hsl-to-rgb-color-conversion
		static Color HSLToRGB(double h, double s, double l)
		{
			double r=0, g=0, b=0;
			if (s == 0) {
				r = b = b = l; //gray scale
			} else {
				double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
				double p = 2 * l - q;
				r = HueToRGB(p, q, h + 1/3);
				g = HueToRGB(p, q, h);
				b = HueToRGB(p, q, h - 1/3);
			}

			int ir = (int)Math.Round(r * 255);
			int ig = (int)Math.Round(g * 255);
			int ib = (int)Math.Round(b * 255);

			return Color.FromArgb(ir,ig,ib);
		}

		static double HueToRGB(double p, double q, double t)
		{
			if(t < 0) { t += 1; }
			if(t > 1) { t -= 1; }
			if(t < 1/6) { return p + (q - p) * 6 * t; }
			if(t < 1/2) { return q; }
			if(t < 2/3) { return p + (q - p) * (2/3 - t) * 6; }
			return p;
		}
	}
}
