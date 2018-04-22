using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DensityBrot
{
	public class Render
	{
		DoubleArray matrix;

		public void RenderToCanvas(ICanvas canvas, FractalConfig config)
		{
			int width = canvas.Width;
			int height = canvas.Height;

			if (matrix == null || matrix.Width != width || matrix.Height != height) {
				matrix = new DoubleArray(width,height);
			} else {
				for(int y=0; y<height; y++) {
					for(int x=0; x<width; x++) {
						matrix[x,y] = 0;
					}
				}
			}

			var taskList = new List<Task>(height*width);

			Console.WriteLine("spooling tasks "+height);
			for(int y=0; y<height; y++) {
				RenderRow(config,y,width,height,matrix);
				Logger.PrintInfo("Rendering Row "+y);
			}

			double min = double.MaxValue,max = double.MinValue;
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = matrix[x,y];
					if (d > 0) { d = Math.Log10(d); }
					if (d < min) { min = d; }
					if (d > max) { max = d; }
				}
				Logger.PrintInfo("Finding total range ["+y+"]");
			}
			double range = Math.Abs(max - min);
			double mult = 255.0/range;

			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = matrix[x,y];
					if (d > 0) { d = Math.Log10(d); }
					Color c;
					if (d <= 0) {
						c = Color.Black;
					} else {
						double q = d*mult - min;
						int w = (int)q;
						c = Color.FromArgb(w,w,w);
					}
					try {
						canvas.SetPixel(x,y,c);
					} catch {
						Console.WriteLine("!! Trying to set x="+x+" y="+y+" c="+c+" w="+width+" h="+height);
						throw;
					}
				}
				Logger.PrintInfo("Normalizing Colors ["+y+"]");
			}
		}

		//public Task RenderOrbitAsync(ICanvas canvas, FracConfig conf, int x, int y, Color highlight)
		//{
		//	return Task.Run(() => {
		//		RenderOrbitToBitmap(canvas,conf,x,y,highlight);
		//	});
		//}

		static void RenderOrbitToBitmap(ICanvas canvas, FractalConfig conf, int x, int y, Color highlight)
		{
			int wth = canvas.Width;
			int hth = canvas.Height;

			Complex z,c;
			InitZC(conf,x,y,wth,hth,out z,out c);

			Complex[] points = new Complex[conf.IterMax];
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape);

			for (int iter = 0; iter < escapeiter; iter++)
			{
				Complex f = points[iter];
				int bx = WorldToWin(f.Real, conf.Resolution, wth, conf.OffsetX);
				int by = WorldToWin(f.Imaginary, conf.Resolution, hth, conf.OffsetY);
				if (bx > 0 && bx < wth && by > 0 && by < hth) {
					canvas.SetPixel(bx,by,highlight);
				}
			}

			canvas.SetPixel(x,y,Color.Blue);
		}

		static void RenderRow(FractalConfig conf, int y, int wth, int hth, DoubleArray data)
		{
			for(int x = 0; x<wth; x++)
			{
				RenderPart(conf,x,y,wth,hth,data);
			}
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
				
		static Complex[] pointBuffer = null;
		static void RenderPart(FractalConfig conf, int x, int y, int wth, int hth, DoubleArray data)
		{
			//http://www.physics.emory.edu/faculty/weeks//software/mandel.c
			//int hxres = data.GetLength(0);
			//int hyres = data.GetLength(1);

			//double xoff = -0.8, yoff = -0.5;
			//Complex res = new Complex((double)wth,(double)hth);

			Complex z,c;
			InitZC(conf,x,y,wth,hth,out z,out c);

			if (pointBuffer == null) {
				pointBuffer = new Complex[conf.IterMax];
			} else {
				pointBuffer.Initialize();
			}
			Complex[] points = pointBuffer;
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape);

			for(int iter = 0; iter < escapeiter; iter++)
			{
				Complex f = points[iter];
				int bx = WorldToWin(f.Real,conf.Resolution,wth,conf.OffsetX);
				int by = WorldToWin(f.Imaginary,conf.Resolution,hth,conf.OffsetY);
				if (bx > 0 && bx < wth && by > 0 && by < hth) {
					data.IncrementByOne(bx,by);
				}
			}

			//for(iter = 0; iter < conf.IterMax; iter++) {
			//
			//	//z = Complex.Pow(z*z,c*4)+c;
			//	z = z*z + c;
			//	int bx = WorldToWin(z.Real,conf.Scale,hxres,conf.OffsetX);
			//	int by = WorldToWin(z.Imaginary,conf.Scale,hyres,conf.OffsetY);
			//	if (bx > 0 && bx < wth && by > 0 && by < hth) {
			//		InterlockedAdd(ref data[bx,by],1);
			//	}
			//
			//	var dist = z.Magnitude;
			//	if (dist > conf.Escape || double.IsNaN(dist) || double.IsInfinity(dist)) { dist = -1; break; }
			//}
			////smooth coloring
			//double index = iter;
			//if (iter < itermax)
			//{
			//	//double zn = Math.Sqrt(z.Real*z.Real+z.Imaginary*z.Imaginary);
			//	double zn = z.Magnitude;
			//	double nu = Math.Log(Math.Log(zn,2),2);
			//	index = iter + 1.0 - nu;
			//}
			//data[hx,hy] = index;
		}

		static int FillOrbit(Complex[] points, int itermax, Complex z, Complex c, double escape)
		{
			int iter;
			for(iter = 0; iter < itermax; iter++) {
				z = z*z + c;

				points[iter] = z;

				var dist = z.Magnitude;
				if (dist > escape || double.IsNaN(dist) || double.IsInfinity(dist)) { dist = -1; break; }
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

		static double InterlockedAdd(ref double location1, double value)
		{
			double newCurrentValue = location1; // non-volatile read, so may be stale
			while (true)
			{
				double currentValue = newCurrentValue;
				double newValue = currentValue + value;
				newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
				if (newCurrentValue == currentValue) {
					return newValue;
				}
			}
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
