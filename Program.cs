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
			case Options.ProcessMode.DensityBrot: CreateDensityBrot(); break;
			case Options.ProcessMode.CreateOrbits: ProduceOrbits(); break;
			case Options.ProcessMode.TestColorMap: CreateColorMapTest(); break;
			case Options.ProcessMode.NebulaBrot: CreateNebulaBrot(); break;
			}
		}

		static void CreateDensityBrot()
		{
			var conf = new FractalConfig {
				Escape = Options.FractalEscape,
				Plane = Planes.XY,
				Resolution = Options.Resolution,
				X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
				IterMax = Options.FractalMaxIter,
				OffsetX = 0.0,
				OffsetY = 0.0,
				HideEscaped = Options.HideEscaped,
				HideContained = Options.HideContained,
				SamplesPerPoint = 10
			};

			DensityMatrix matrix = null;
			try
			{
				if (Options.CreateMatrix) {
					matrix = DoCreateMatrix(conf);
				} else {
					matrix = LoadMatrix();
				}

				if (Options.CreateImage) {
					DoCreateImage(matrix);
				}
			}
			finally
			{
				if (matrix != null) {
					matrix.Dispose();
				}
			}
		}

		static DensityMatrix DoCreateMatrix(FractalConfig conf, string name = null)
		{
			DensityMatrix matrix = new DensityMatrix(Options.Width, Options.Height);
			var builder = new FractalBuilder(matrix, conf);
			string n = EnsureEndsWith(name ?? Options.FileName, ".dm");
			Logger.PrintInfo("building matrix [" + n + "]");
			builder.Build();
			Logger.PrintInfo("saving matrix file [" + n + "]");
			matrix.SaveToFile(n);
			return matrix;
		}

		static void DoCreateImage(IDensityMatrix matrix)
		{
			Logger.PrintInfo("matrix = [" + matrix.Width + "x" + matrix.Height + " " + matrix.Maximum + "]");
			IColorMap cm = GetColorMap(out string _);
			using (var img = new MagicCanvas(Options.Width, Options.Height))
			{
				Logger.PrintInfo("building image");
				PaintImageData(matrix, cm, img);

				SaveCanvas(img);
			}
		}

		static void SaveCanvas(ICanvas img)
		{
			string n = EnsureEndsWith(Options.FileName, ".png");
			Logger.PrintInfo("saving image file [" + n + "]");
			img.SavePng(n);
		}

		static void DrawToImageComponent(ICanvas img, IDensityMatrix matrix, ColorComponent comp)
		{
			Logger.PrintInfo("matrix = [" + matrix.Width + "x" + matrix.Height + " " + matrix.Maximum + "]");
			IColorMap cm = new GrayColorMap();
			Logger.PrintInfo("drawing component '"+comp+"'");
			PaintImageData(matrix, cm, img, comp);
		}

		private static DensityMatrix LoadMatrix(string name = null)
		{
			DensityMatrix matrix;
			string a = EnsureEndsWith(name ?? Options.FileName, ".dm");
			Logger.PrintInfo("loading matrix file [" + a + "]");
			matrix = new DensityMatrix(a);
			Options.Width = matrix.Width;
			Options.Height = matrix.Height;
			return matrix;
		}

		static void PaintImageData(IDensityMatrix matrix, IColorMap cm, ICanvas img, ColorComponent comp = ColorComponent.None)
		{
			double lm = matrix.Maximum;
			double ln = double.MaxValue;

			for (int y = 0; y < Options.Height; y++) {
				for (int x = 0; x < Options.Width; x++) {
					double li = matrix[x, y];
					if (li > 0.0 && li < ln) { ln = li; }
				}
			}

			Debug.WriteLine("ln = "+ln);

			for (int y = 0; y < Options.Height; y++) {
				for (int x = 0; x < Options.Width; x++) {
					double li = matrix[x, y];
					ColorD c = cm.GetColor(li - ln, lm - ln);
					if (comp != ColorComponent.None) {
						img.SetPixelComponent(x,y,comp,c.GetComponent(comp));
					} else {
						img.SetPixel(x, y, c);
					}
				}
			}
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

		static void CreateNebulaBrot()
		{
			using (var matrixR = CreateNebulaBrotMatrix("-r",Options.NebulaRIter,ColorComponent.R))
			using (var matrixG = CreateNebulaBrotMatrix("-g",Options.NebulaGIter,ColorComponent.G))
			using (var matrixB = CreateNebulaBrotMatrix("-b",Options.NebulaBIter,ColorComponent.B))
			{
				if (!Options.CreateImage) { return; }
				using (var canvas = new MagicCanvas(Options.Width,Options.Height,true))
				{
					DrawToImageComponent(canvas,matrixR,ColorComponent.R);
					DrawToImageComponent(canvas,matrixG,ColorComponent.G);
					DrawToImageComponent(canvas,matrixB,ColorComponent.B);

					SaveCanvas(canvas);
				}
			}
		}

		static IDensityMatrix CreateNebulaBrotMatrix(string suffix, int iters, ColorComponent comp)
		{
			var conf = new FractalConfig {
				Escape = Options.FractalEscape,
				Plane = Planes.XY,
				Resolution = Options.Resolution,
				X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
				IterMax = iters,
				//OffsetX = Options.Width/2,
				//OffsetY = Options.Height/2,
				HideEscaped = Options.HideEscaped,
				HideContained = Options.HideContained
			};

			DensityMatrix matrix = null;
			string mname = Path.Combine(
				Path.GetDirectoryName(Options.FileName),
				Path.GetFileNameWithoutExtension(Options.FileName) + suffix + ".dm"
			);

			if (Options.CreateMatrix) {
				matrix = DoCreateMatrix(conf, mname);
			} else {
				matrix = LoadMatrix(mname);
			}
			return matrix;
		}
	}
}
