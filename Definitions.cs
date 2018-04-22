using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public enum Planes { XY, XW, XZ, YW, YZ, WZ }

	public class FractalConfig
	{
		public double X;
		public double Y;
		public double W;
		public double Z;
		public Planes Plane;
		public double Resolution;
		public double Escape;
		public int IterMax;
		public int OffsetX;
		public int OffsetY;
		public bool HideEscaped;
		public bool HideContained;

		public static FractalConfig Default { get {
			return new FractalConfig {
				X = 0.0,
				Y = 0.0,
				W = 0.0,
				Z = 0.0,
				Plane = Planes.XY,
				Resolution = 20.0,
				Escape = 4.0,
				IterMax = 100,
				HideContained = false,
				HideEscaped = false
			};
		}}
	}

	public class RenderConfig
	{
		public IColorMap ColorMap;
	}
}
