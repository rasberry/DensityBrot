using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public enum SomeColorMaps
	{
		Gray = 0,
		AllColors = 1,
	}

	public static class ColorHelpers
	{
		public static IColorMap GetColorMap(SomeColorMaps map)
		{
			switch(map)
			{
			default:
			case SomeColorMaps.Gray: return new GrayColorMap();
			case SomeColorMaps.AllColors: return new FullRangeRGBColorMap();
			}
		}

		public static IColorMap GetGimpColorMap(string ggrFile)
		{
			return new GimpGGRColorMap(ggrFile);
		}

		//https://stackoverflow.com/questions/2353211/hsl-to-rgb-color-conversion
		public static void HSLToRGB(double h, double s, double l,out double r, out double g, out double b)
		{
			r = g = b = 0;
			if (s == 0) {
				r = g = b = l; //gray scale
			} else {
				double q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
				double p = 2.0 * l - q;
				r = HueToRGB(p, q, h + 1.0/3.0);
				g = HueToRGB(p, q, h);
				b = HueToRGB(p, q, h - 1.0/3.0);
			}
		}

		static double HueToRGB(double p, double q, double t)
		{
			if(t < 0) { t += 1; }
			if(t > 1) { t -= 1; }
			if(t < 1.0/6.0) { return p + (q - p) * 6.0 * t; }
			if(t < 1.0/2.0) { return q; }
			if(t < 2.0/3.0) { return p + (q - p) * (2.0/3.0 - t) * 6.0; }
			return p;
		}

		public static void RGBtoHSV(double r,double g,double b,out double h,out double s,out double v)
		{
			double max = Math.Max(r,Math.Max(g,b));
			double min = Math.Min(r,Math.Min(g,b));
			double d = max - min;
			h = 0.0;
			s = (max == 0.0 ? 0.0 : d / max);
			v = max;

			if (max == min) {
				h = 0.0;
			} else if (max == r) {
				h = (g - b) + d * (g < b ? 6.0: 0.0); h /= 6.0 * d;
			} else if (max == g) {
				h = (b - r) + d * 2.0; h /= 6.0 * d;
			} else if (max == b) {
				h = (r - g) + d * 4.0; h /= 6.0 * d;
			}
		}

		public static void HSVtoRGB(double h,double s,double v,out double r,out double g,out double b)
		{
			r=0; g=0; b=0;
			int i = (int)Math.Floor(h * 6.0);
			double f = h * 6.0 - i;
			double p = v * (1.0 - s);
			double q = v * (1.0 - f * s);
			double t = v * (1.0 - (1.0 - f) * s);
			switch(i % 6) {
				case 0: r = v; g = t; b = p; break;
				case 1: r = q; g = v; b = p; break;
				case 2: r = p; g = v; b = t; break;
				case 3: r = p; g = q; b = v; break;
				case 4: r = t; g = p; b = v; break;
				case 5: r = v; g = p; b = q; break;
			}
		}
	}
}