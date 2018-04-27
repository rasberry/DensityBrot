using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IFractalBuilder
	{
		IDensityMatrix Matrix { get; }
		int Width { get; }
		int Height { get; }
		void Build();
	}

	public class FractalBuilder : IFractalBuilder
	{
		public FractalBuilder(IDensityMatrix matrix, FractalConfig config = null)
		{
			Matrix = matrix;
			this.config = config ?? FractalConfig.Default;
		}

		public IDensityMatrix Matrix { get; set; }
		public int Width { get { return Matrix.Width; }}
		public int Height  { get { return Matrix.Height; }}

		public FractalConfig config;

		public void Build()
		{
			var sw = Stopwatch.StartNew();
			
			for(int y = 0; y<Height; y++) {
				for(int x = 0; x<Width; x++) {
					RenderPart(config,x,y,Width,Height,Matrix);
				}
				if (sw.ElapsedMilliseconds > 1000) {
					sw.Restart();
					Logger.PrintInfo("progress "+y+" out of "+Height);
				}
			}
			Logger.PrintInfo("Build took "+sw.ElapsedMilliseconds);
		}

		static void InitZC(FractalConfig conf, int x, int y, int wth, int hth, out Complex z, out Complex c)
		{
			double cx = WinToWorld(x, conf.Resolution, wth, conf.OffsetX);
			double cy = WinToWorld(y, conf.Resolution, hth, conf.OffsetY);

			switch(conf.Plane)
			{
			case Planes.XY: default:
				c = new Complex(cx,cy);
				z = new Complex(conf.X,conf.Y); break;
			case Planes.XW:
				c = new Complex(conf.W,cy);
				z = new Complex(conf.X,cx); break;
			case Planes.XZ:
				c = new Complex(cx,conf.Z);
				z = new Complex(conf.X,cy); break;
			case Planes.YW:
				c = new Complex(conf.W,cx);
				z = new Complex(cy,conf.Y); break;
			case Planes.YZ:
				c = new Complex(cx,conf.Z);
				z = new Complex(cy,conf.Y); break;
			case Planes.WZ:
				c = new Complex(conf.W,conf.Z);
				z = new Complex(cx,cy); break;
			}
		}
		
		static void RenderPart(FractalConfig conf, int x, int y, int wth, int hth, IDensityMatrix data)
		{
			//http://www.physics.emory.edu/faculty/weeks/software/mandel.c

			Complex z,c;
			InitZC(conf,x,y,wth,hth,out z,out c);
			Complex[] points = new Complex[conf.IterMax];;
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape,out bool didesc);
			bool draw = conf.HideEscaped && didesc || conf.HideContained && !didesc;
			//if (!draw) { return; }

			for(int iter = 0; iter < escapeiter; iter++)
			{
				Complex f = points[iter];
				int bx = WorldToWin(f.Real,conf.Resolution,wth,conf.OffsetX);
				int by = WorldToWin(f.Imaginary,conf.Resolution,hth,conf.OffsetY);
				if (bx > 0 && bx < wth && by > 0 && by < hth) {
					data.Touch(bx,by);
				}
			}
		}

		static int FillOrbit(Complex[] points, int itermax, Complex z, Complex c, double escape, out bool didEscape)
		{
			didEscape = false;
			int iter;
			for(iter = 0; iter < itermax; iter++) {
				z = z*z + c;

				points[iter] = z;

				var dist = z.Magnitude;
				if (dist > escape || double.IsNaN(dist) || double.IsInfinity(dist)) {
					didEscape = true;
					dist = -1;
					break;
				}
			}
			return iter;
		}

		static double WinToWorld(int v, double magnify, int res, int offset)
		{
			//return (v + offset)/(double)res / magnify;
			//return (((double)v) / ((double)res) + offset) / magnify;
			return (v - offset) / magnify;

		}
		static int WorldToWin(double v, double magnify, int res, int offset)
		{
			//return (int)Math.Round(res * magnify * v) - offset;
			//return (int)Math.Round((double)res * (magnify * v - offset));
			return (int)Math.Round(v * magnify) + offset;
		}
	}
}
