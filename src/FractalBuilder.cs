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
		public FractalBuilder(IDensityMatrix[] matrix, FractalConfig config = null)
		{
			Matrix = matrix;
			this.config = config ?? FractalConfig.Default;
		}

		public IDensityMatrix[] Matrix { get; set; }
		FractalConfig config;

		public void Build()
		{
			Stopwatch sw = Stopwatch.StartNew();

			//TODO maybe have an option for image size bounds ?
			// double sampleRadW = config.Resolution / Options.Width;
			// double sampleRadH = config.Resolution / Options.Height;
			// double sampleRadWhalf = sampleRadW / 2;
			// double sampleRadHhalf = sampleRadH / 2;
			//RectangleD escapeBounds = new RectangleD(
			//	- config.Escape + config.OffsetX - 2 * sampleRadW,
			//	- config.Escape + config.OffsetY - 2 * sampleRadH,
			//	2 * config.Escape + 2 * sampleRadW,
			//	2 * config.Escape + 2 * sampleRadH
			//);

			//var rnd = new UniqueRandom(config.SamplesPerPoint * 2);
			var po = new ParallelOptions { MaxDegreeOfParallelism = Matrix.Length };
			long total = Options.Height * Options.Width * config.SamplesPerPoint;
			using (var progress = Logger.CreateProgress(total))
			{
				Parallel.For(0,Matrix.Length,po,(i) => {
					var rnd = new Random(i); //does not seem to be thread safe so putting inside thread
					//var rnd = new RandomWrapper();
					//var rnd = new UniqueRandom();
					//var rnd = new MersenneTwister();
					var mat = Matrix[i];
					int start = i * Options.Height / Matrix.Length;
					int end = (1 + i) * Options.Height / Matrix.Length;
					for(int y = start; y<end; y++) {
						for(int x = 0; x<Options.Width; x++) {
							for(int s = 0; s<config.SamplesPerPoint; s++) {
								double nx = 1.0 * rnd.NextDouble() - 0.5;
								double ny = 1.0 * rnd.NextDouble() - 0.5;
								RenderPart(config,x + nx,y + ny,Options.Width,Options.Height,mat);
								progress.Update("Matrix");
							}
						}
					}
				});
			}

			Logger.PrintInfo("Build took "+sw.ElapsedMilliseconds);
		}

		static void InitZC(FractalConfig conf, double x, double y, int wth, int hth, out Complex z, out Complex c)
		{
			double cx = WinToWorld(x, conf.Resolution, wth, conf.OffsetX);
			double cy = WinToWorld(y, conf.Resolution, hth, conf.OffsetY);

			switch(conf.Plane)
			{
			case Planes.ZrZi: default:
				c = new Complex(cx,cy);
				z = new Complex(conf.Zr,conf.Zi); break;
			case Planes.ZrCr:
				c = new Complex(conf.Cr,cy);
				z = new Complex(conf.Zr,cx); break;
			case Planes.ZrCi:
				c = new Complex(cx,conf.Ci);
				z = new Complex(conf.Zr,cy); break;
			case Planes.ZiCr:
				c = new Complex(conf.Cr,cx);
				z = new Complex(cy,conf.Zi); break;
			case Planes.ZiCi:
				c = new Complex(cx,conf.Ci);
				z = new Complex(cy,conf.Zi); break;
			case Planes.CrCi:
				c = new Complex(conf.Cr,conf.Ci);
				z = new Complex(cx,cy); break;
			}
		}

		static void RenderPart(FractalConfig conf, double x, double y, int wth, int hth, IDensityMatrix data)
		{
			//http://www.physics.emory.edu/faculty/weeks/software/mandel.c

			Complex z,c;
			InitZC(conf,x,y,wth,hth,out z,out c);
			Complex[] points = new Complex[conf.IterMax];
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape,out bool didesc);
			bool hide = conf.HideEscaped && didesc || conf.HideContained && !didesc;
			if (hide) { return; }

			for(int iter = 0; iter < escapeiter; iter++)
			{
				Complex f = points[iter];
				int bx = WorldToWin(f.Real,conf.Resolution,wth,conf.OffsetX);
				int by = WorldToWin(f.Imaginary,conf.Resolution,hth,conf.OffsetY);
				if (bx >= 0 && bx < wth && by >= 0 && by < hth) {
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
					break;
				}
			}
			return iter;
		}

		static double WinToWorld(double v, double magnify, int res, double offset)
		{
			return magnify / res * v - (magnify / 2 - offset);
		}
		static int WorldToWin(double v, double magnify, int res, double offset)
		{
			return (int)((v + magnify / 2 + offset) * res / magnify);
		}

		struct RectangleD
		{
			public RectangleD(double x,double y,double width,double height)
			{
				X = x; Y = y; Width = width; Height = height;
			}

			public double X;
			public double Y;
			public double Width;
			public double Height;

			public double Bottom { get { return Y + Height; }}
			public double Top { get { return Y; }}
			public double Left { get { return X; }}
			public double Right { get { return X + Width; }}

			public bool Contains(double x, double y)
			{
				return X <= x && x < X + Width && Y <= y && y < Y + Height;
			}
		}
	}
}
