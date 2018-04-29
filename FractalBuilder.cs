using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public class FractalBuilder
	{
		public FractalBuilder(IDensityMatrix matrix, FractalConfig config = null)
		{
			Matrix = matrix;
			this.config = config ?? FractalConfig.Default;
		}

		public IDensityMatrix Matrix { get; set; }
		public int Width { get { return Matrix.Width; }}
		public int Height  { get { return Matrix.Height; }}
		FractalConfig config;

		public void Build()
		{
			var sw = Stopwatch.StartNew();
			double sampleRadW = config.Resolution / Options.Width;
			double sampleRadH = config.Resolution / Options.Height;
			double sampleRadWhalf = sampleRadW / 2;
			double sampleRadHhalf = sampleRadH / 2;
			
			for(int y = 0; y<Height; y++) {
				for(int x = 0; x<Width; x++) {
					var rnd = new UniqueRandom(config.SamplesPerPoint * 2);
					for(int s = 0; s<config.SamplesPerPoint; s++) {
						double nx = sampleRadW * rnd.NextDouble() - sampleRadWhalf;
						double ny = sampleRadH * rnd.NextDouble() - sampleRadHhalf;
						RenderPart(config,x,y,Width,Height,Matrix,new Complex(nx,ny));
					}
				}
				if (sw.ElapsedMilliseconds > 1000) {
					sw.Restart();
					Logger.PrintInfo("progress "+y+" out of "+Height);
				}
			}
			Logger.PrintInfo("Build took "+sw.ElapsedMilliseconds);
		}

		static void InitZC(FractalConfig conf, int x, int y, int wth, int hth, Complex pert, out Complex z, out Complex c)
		{
			double cx = WinToWorld(x, conf.Resolution, wth, conf.OffsetX);
			double cy = WinToWorld(y, conf.Resolution, hth, conf.OffsetY);

			switch(conf.Plane)
			{
			case Planes.XY: default:
				c = new Complex(cx,cy) + pert;
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
		
		static void RenderPart(FractalConfig conf, int x, int y, int wth, int hth, IDensityMatrix data, Complex pert)
		{
			//http://www.physics.emory.edu/faculty/weeks/software/mandel.c

			Complex z,c;
			InitZC(conf,x,y,wth,hth,pert,out z,out c);
			Complex[] points = new Complex[conf.IterMax];
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape,out bool didesc);
			bool hide = conf.HideEscaped && didesc || conf.HideContained && !didesc;
			if (hide) { return; }

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

				//TODO pass in a rectangle instead of using escape
				//  need to add -res/width to the less than sides because image has a top and left black pixel line
				//var dist = z.Magnitude;
				if (z.Real < -escape || z.Real > escape || z.Imaginary < -escape || z.Imaginary > escape) {
				//if (dist > escape || double.IsNaN(dist) || double.IsInfinity(dist)) {
					didEscape = true;
					//dist = -1;
					break;
				}
			}
			return iter;
		}

		static double WinToWorld(int v, double magnify, int res, double offset)
		{
			return magnify / res * v - (magnify / 2 - offset);
			// return (v - offset) / magnify;

		}
		static int WorldToWin(double v, double magnify, int res, double offset)
		{
			return (int)((v + magnify / 2 + offset) * res / magnify);
			//return (int)Math.Round(v * magnify) + offset;
		}

		class UniqueRandom
		{
			public UniqueRandom(int count)
			{
				this.count = count;
				list = new List<int>(count);
				for(int i = 0; i<count; i++) {
					list.Add(i);
				}
			}

			List<int> list;
			Random rnd = new Random(1301);
			int count;

			public double NextDouble()
			{
				int len = list.Count;
				int nexti = rnd.Next(0,len);
				int tmp = list[nexti];
				list[nexti] = list[len-1];
				list.RemoveAt(len - 1);
				return (double)tmp / count;
			}
		}
	}
}
