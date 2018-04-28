using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IColorMap
	{
		ColorD GetColor(double index, double maximum);
	}

	public class GrayColorMap : IColorMap
	{
		public ColorD GetColor(double index, double maximum)
		{
			double pct = index / maximum;
			return ColorD.FromArgb(1.0,pct,pct,pct);
		}
	}

	public class FullRangeRGBColorMap : IColorMap
	{
		public ColorD GetColor(double index, double maximum)
		{
			return FindColorFromRange(0,maximum,index);
		}

		static ColorD FindColorFromRange(double min, double max, double val)
		{
			//iterate HSL L=[0 to 1] S=1 H[0 to 360]
			double range = max - min;
			double spacemax = 256.0 * 360.0; //L * H
			double pos = (val - min) / range * spacemax;
	
			//basically map 2D HxL space into one dimension
			double s = 1.0;
			//double l = (pos / spacemax) * 0.9 + 0.05; //clip the edges by 5%
			double l = pos / spacemax;
			double h = (pos % (360.0 * 4)) / (360.0 * 4); //4 slows down the cycle 4x
	
			ColorHelpers.HSLToRGB(h,s,l, out double r,out double g,out double b);
			return ColorD.FromArgb(1.0,r,g,b);
		}
	}

	public class GimpGGRColorMap : IColorMap
	{
		// https://github.com/jjgreen/cptutils/blob/master/src/common/ggr.c	
		// https://nedbatchelder.com/code/modules/ggr.py
		// https://stackoverflow.com/questions/3462295/exporting-gimp-gradient-file

		public GimpGGRColorMap(string ggrfile)
		{
			Grad = LoadGradient(ggrfile);
		}

		public ColorD GetColor(double index, double maximum)
		{
			double z = index / maximum;
			GradientColour(z,Grad,out double r, out double g, out double b, out double a);
			return ColorD.FromArgb(a,r,g,b);
		}

		Gradient Grad = null;

		static void GradientColour(double z, Gradient grad,out double r, out double g, out double b, out double a)
		{
			r = g = b = a = 0;
			if (grad == null) {
				return;
			}

			z = Math.Min(1.0,Math.Max(0.0,z));
			var seg = GetSegmentAt(grad,z);

			double middle,factor = 0;
			double seglen = seg.right - seg.left;
			if (seglen < double.Epsilon) {
				middle = 0.5;
				z = 0.5;
			} else {
				middle = (seg.middle - seg.left) / seglen;
				z = (z - seg.left) / seglen;
			}

			switch(seg.type)
			{
			case GradType.Linear:
				factor = CalcLinearFactor(middle,z);
				break;
			case GradType.Curved:
				if (middle < double.Epsilon) {
					middle = double.Epsilon;
				}
				factor = Math.Pow(z, Math.Log(0.5) / Math.Log(middle));
				break;
			case GradType.Sine:
				z = CalcLinearFactor(middle,z);
				factor = (Math.Sin((-Math.PI/2.0) + Math.PI*z) + 1.0)/2.0;
				break;
			case GradType.SphereInc:
				z = CalcLinearFactor(middle,z) - 1.0;
				factor = Math.Sqrt(1.0 - z*z);
				break;
			case GradType.SphereDec:
				z = CalcLinearFactor(middle,z);
				factor = 1.0 - Math.Sqrt(1.0 - z*z);
				break;
			default:
				throw new ArgumentException("Corrupt gradient");
			}

			a = seg.a0 + (seg.a1 - seg.a0) * factor;

			if (seg.color == GradColor.RGB)
			{
				r = seg.r0 + (seg.r1 - seg.r0) * factor;
				g = seg.g0 + (seg.g1 - seg.g0) * factor;
				b = seg.b0 + (seg.b1 - seg.b0) * factor;
			}
			else
			{
				double h0,s0,v0;
				ColorHelpers.RGBtoHSV(seg.r0,seg.g0,seg.b0,out h0,out s0, out v0);
				double h1,s1,v1;
				ColorHelpers.RGBtoHSV(seg.r1,seg.g1,seg.b1,out h1,out s1, out v1);
				s0 = s0 + (s1 - s0) * factor;
				v0 = v0 + (v1 - v0) * factor;

				switch(seg.color)
				{
				case GradColor.HSVccw:
					if (h0 < h1) {
						h0 = h0 + (h1 - h0) * factor;
					} else {
						h0 = h0 + (1.0 - (h0 - h1)) * factor;
						if (h0 > 1.0) {
							h0 -= 1.0;
						}
					}
					break;
				case GradColor.HSVcw:
					if (h1 < h0) {
						h0 = h0 - (h0 - h1) * factor;
					} else {
						h0 = h0 - (1.0 - (h1 - h0)) * factor;
						if (h0 < 0.0) {
							h0 += 1.0;
						}
					}
					break;
				default:
					throw new ArgumentException("unknown colour model");
				}
				ColorHelpers.HSVtoRGB(h0,s0,v0,out r,out g,out b);
			}
		}

		static double CalcLinearFactor(double middle, double z)
		{
			if (z <= middle) {
				return middle < double.Epsilon ? 0.0 : 0.5 * z / middle;
			} else {
				z -= middle;
				middle = 1.0 - middle;
				return middle < double.Epsilon ? 1.0 : 0.5 + 0.5 * z / middle;
			}
		}

		static GradSegment GetSegmentAt(Gradient grad, double z)
		{
			foreach(var seg in grad.segments)
			{
				if (z >= seg.left && z <= seg.right) {
					return seg;
				}
			}
			throw new ArgumentOutOfRangeException("no matching segment for "+z);
		}

		static Gradient LoadGradient(string name)
		{
			using (var fs = File.Open(name,FileMode.Open,FileAccess.Read,FileShare.Read))
			using (var sr = new StreamReader(fs))
			{
				string line = sr.ReadLine();
				if(!line.StartsWith("GIMP Gradient")) {
					throw new FileLoadException("file does not seem to be a GIMP gradient");
				}
				return LoadGrad(sr,name);
			}
		}

		static Gradient LoadGrad(StreamReader sr,string name)
		{
			Gradient g = new Gradient();

			string line = sr.ReadLine();
			if (line.StartsWith("Name:")) {
				g.name = line.Substring(5).Trim();
			} else {
				g.name = name;
			}

			line = sr.ReadLine();
			if (!int.TryParse(line,out int numsegments) || numsegments < 1 || numsegments > MaxSegments) {
				throw new ArgumentOutOfRangeException("invalid number of segments");
			}

			var segList = new List<GradSegment>();
			for(int i=0; i<numsegments; i++)
			{
				line = sr.ReadLine();
				var seg = ParseSeg(line);
				segList.Add(seg);
			}
			g.segments = segList.ToArray();

			return g;
		}

		static GradSegment ParseSeg(string line)
		{
			int element = 0;
			StringBuilder chunk = new StringBuilder();
			double num = double.NaN;
			GradSegment seg = new GradSegment();
			line += " ";

			foreach(char c in line)
			{
				if (!char.IsWhiteSpace(c)) {
					chunk.Append(c);
					continue;
				}

				double.TryParse(chunk.ToString(),out num);
				if (double.IsInfinity(num) || double.IsNaN(num)) {
					throw new ArgumentOutOfRangeException("unexpected value found");
				}

				switch(element)
				{
				case 00: seg.left = num; break;
				case 01: seg.middle = num; break;
				case 02: seg.right = num; break;
				case 03: seg.r0 = num; break;
				case 04: seg.g0 = num; break;
				case 05: seg.b0 = num; break;
				case 06: seg.a0 = num; break;
				case 07: seg.r1 = num; break;
				case 08: seg.g1 = num; break;
				case 09: seg.b1 = num; break;
				case 10: seg.a1 = num; break;
				case 11: seg.type = (GradType)((int)num); break;
				case 12: seg.color = (GradColor)((int)num); break;
				}
				chunk.Clear();
				element++;
			}

			if (seg.color == GradColor.HSVshort || seg.color == GradColor.HSVlong)
			{
				double h0,s0,v0,h1,s1,v1;
				ColorHelpers.RGBtoHSV(seg.r0,seg.g0,seg.b0,out h0,out s0,out v0);
				ColorHelpers.RGBtoHSV(seg.r1,seg.g1,seg.b1,out h1,out s1,out v1);
				seg.color = GradHSVType(seg.color,h0,h1);
			}
			if (!Enum.IsDefined(typeof(GradColor),seg.color)) {
				throw new ArgumentOutOfRangeException("unknown colour model");
			}
			return seg;
		}
		
		static GradColor GradHSVType(GradColor color,double x, double y)
		{
			double min = Math.Min(x,y);
			double max = Math.Max(x,y);

			double midlen = max - min;
			double rndlen = min + (1.0 - max);

			GradColor shorter;
			if (rndlen < midlen) {
				shorter = (max == y ? GradColor.HSVcw : GradColor.HSVccw);
			} else {
				shorter = (max == y ? GradColor.HSVccw : GradColor.HSVcw);
			}
			if (color == GradColor.HSVlong) {
				return shorter == GradColor.HSVcw ? GradColor.HSVccw : GradColor.HSVcw;
			} else if (color == GradColor.HSVshort) {
				return shorter;
			} else {
				return color;
			}
		}

		const int MaxSegments = 4096;

		enum GradType
		{
			Linear = 0,
			Curved,
			Sine,
			SphereInc,
			SphereDec
		}

		enum GradColor
		{
			RGB=0,     /* normal RGB */
			HSVccw,    /* counterclockwise hue */
			HSVcw,     /* clockwise hue */
			HSVshort,  /* shorter of cw & ccw hue */
			HSVlong    /* longer of cw & ccw hue */
		}

		class GradSegment
		{
			public double left = 0.0, middle = 0.5, right = 1.0;
			public double r0 = 0.0, g0 = 0.0, b0 = 0.0, a0 = 0.0;
			public double r1 = 1.0, g1 = 1.0, b1 = 1.0, a1 = 1.0;
			public GradType type = GradType.Linear;
			public GradColor color = GradColor.RGB;
		}

		class Gradient
		{
			public string name;
			public GradSegment[] segments;
		}
	}
}
