using ImageMagick;
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
	class Program
	{
		static void Main(string[] args)
		{
			MagickNET.SetTempDirectory(Environment.CurrentDirectory);
			#if DEBUG
			Debug.Listeners.Add(new ConsoleTraceListener());
			#endif

			try {
				MainMain(args);
			} catch(Exception e) {
				#if DEBUG
				string err = e.ToString();
				#else
				string err = e.Message;
				#endif
				Logger.PrintError(err);
			}
		}

		static void MainMain(string[] args)
		{
			if (!Options.ProcessArgs(args)) { return; }
			Logger.ShowVerbose = Options.ShowVerbose;

			switch(Options.Mode)
			{
			case Options.ProcessMode.Fractal: CreateFractal(); break;
			case Options.ProcessMode.CreateOrbits: ProduceOrbits(); break;
			case Options.ProcessMode.TestColorMap: CreateColorMapTest(); break;
			}
		}

		static void CreateFractal()
		{
			var conf = new FractalConfig {
				Escape = Options.FractalEscape,
				Plane = Planes.XY,
				Resolution = Options.Resolution,
				X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
				IterMax = Options.FractalMaxIter,
				OffsetX = Options.Width/2,
				OffsetY = Options.Height/2
			};

			DensityMatrix matrix = null;
			try
			{
				if (Options.CreateMatrix) {
					matrix = DoCreateMatrix(conf);
				}
				if (Options.CreateImage) {
					matrix = DoCreateImage(matrix);
				}
			}
			finally
			{
				if (matrix != null) {
					matrix.Dispose();
				}
			}
		}

		static DensityMatrix DoCreateMatrix(FractalConfig conf)
		{
			DensityMatrix matrix = new DensityMatrix(Options.Width, Options.Height);
			var builder = new FractalBuilder(matrix, conf);
			Logger.PrintInfo("building matrix");
			builder.Build();
			string n = EnsureEndsWith(Options.FileName, ".dm");
			Logger.PrintInfo("saving matrix file [" + n + "]");
			matrix.SaveToFile(n);
			return matrix;
		}

		static DensityMatrix DoCreateImage(DensityMatrix matrix)
		{
			if (matrix == null)
			{
				string a = EnsureEndsWith(Options.FileName, ".dm");
				Logger.PrintInfo("loading matrix file [" + a + "]");
				matrix = new DensityMatrix(a);
				Options.Width = matrix.Width;
				Options.Height = matrix.Height;
			}

			Logger.PrintInfo("matrix = [" + matrix.Width + "x" + matrix.Height + " " + matrix.Maximum + "]");
			IColorMap cm = GetColorMap(out string _);
			using (var img = new MagicCanvas(Options.Width, Options.Height))
			{
				Logger.PrintInfo("building image");
				double lm = Math.Log(matrix.Maximum);
				for (int y = 0; y < Options.Height; y++)
				{
					for (int x = 0; x < Options.Width; x++)
					{
						double li = Math.Log(matrix[x,y]);
						ColorD c = cm.GetColor(li, lm);
						img.SetPixel(x, y, c);
					}
				}
				string n = EnsureEndsWith(Options.FileName, ".png");
				Logger.PrintInfo("saving image file [" + n + "]");
				img.SavePng(n);
			}
			return matrix;
		}

		static string EnsureEndsWith(string name,string ext)
		{
			if (!name.EndsWith(ext,StringComparison.OrdinalIgnoreCase)) {
				name += ext;
			}
			return name;
		}

		static void ProduceOrbits()
		{
			//TODO implement
		}

		static void CreateColorMapTest()
		{
			IColorMap cmap = GetColorMap(out string name);
			using (var img = new MagicCanvas(Options.Width,Options.Height))
			{
				for(int x=0; x<Options.Width; x++)
				{
					ColorD c = cmap.GetColor(x,Options.Width);
					img.DrawLine(c,x,0,x,Options.Height-1);
				}
				img.SavePng("ColorMapTest-"+name+".png");
			}
		}

		static IColorMap GetColorMap(out string name)
		{
			IColorMap cmap;
			if (!String.IsNullOrWhiteSpace(Options.GgrFile)) {
				cmap = ColorHelpers.GetGimpColorMap(Options.GgrFile);
				name = Path.GetFileNameWithoutExtension(Options.GgrFile);
			} else {
				cmap = ColorHelpers.GetColorMap(Options.MapColors);
				name = Options.MapColors.ToString();
			}
			return cmap;
		}
	}
}
