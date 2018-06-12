﻿using ImageMagick;
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
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
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

			Logger.PrintLine("Starting "+Options.Mode);
			Stopwatch sw = null;
			if (Logger.ShowVerbose) {
				sw = Stopwatch.StartNew();
			}
			switch(Options.Mode)
			{
			case Options.ProcessMode.DensityBrot:  CreateDensityBrot(); break;
			case Options.ProcessMode.CreateOrbits: ProduceOrbits(); break;
			case Options.ProcessMode.TestColorMap: CreateColorMapTest(); break;
			case Options.ProcessMode.NebulaBrot:   CreateNebulaBrot(); break;
			}
			if (Logger.ShowVerbose) {
				Logger.PrintInfo("Run took "+(sw.Elapsed.TotalSeconds).ToString("N1")+" seconds");
			}
		}

		static void CreateDensityBrot()
		{
			var conf = Options.ConfigFromOptions();
			IDensityMatrix matrix = null;
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

		static IDensityMatrix DoCreateMatrix(FractalConfig conf, string name = null)
		{
			IDensityMatrix[] matrix = new DensityMatrix[Options.ThreadCount];
			Logger.PrintInfo("Number of threads: "+Options.ThreadCount);
			try {
				for(int t=0; t < Options.ThreadCount; t++) {
					matrix[t] = new DensityMatrix(Options.Width, Options.Height);
				}
				var builder = new FractalBuilder(matrix, conf);
				string n = EnsureEndsWith(name ?? Options.FileName, ".dm");
				Logger.PrintInfo("building matrix [" + n + "]");
				builder.Build();
				Logger.PrintInfo("saving matrix file [" + n + "]");

				// if there's only 1 matrix we can just return it as is
				var merged = matrix.Length == 1
					? matrix[0]
					: MergeMatrix(matrix)
				;
				FileHelpers.SaveToFile(n,merged,conf);
				return merged;
			}
			finally {
				//matrix 'merged' is being disposed outside of this function so don't
				// dispose it here. just skip the matrix.Length == 1 case
				if (matrix != null && matrix.Length > 1) {
					foreach(var m in matrix) {
						if (m != null) {
							m.Dispose();
						}
					}
				}
			}
		}

		static IDensityMatrix MergeMatrix(IDensityMatrix[] matrix)
		{
			long total = matrix.Length * Options.Width * Options.Height;
			var merged = new DensityMatrix(Options.Width, Options.Height);
			using (var progress = Logger.CreateProgress(total))
			{
				foreach(var m in matrix) {
					for(int y=0; y<Options.Height; y++) {
						for(int x=0; x<Options.Width; x++) {
							merged[x,y] += m[x,y];
							progress.Update("Merge");
						}
					}
				}
			}
			return merged;
		}

		static void DoCreateImage(IDensityMatrix matrix)
		{
			Logger.PrintLine("Creating Image");
			Logger.PrintInfo("matrix = [" + matrix.Width + "x" + matrix.Height + " " + matrix.Maximum + "]");
			IColorMap cm = GetColorMap(out string _);
			using (var img = new MagicCanvas(Options.Width, Options.Height))
			{
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

		private static IDensityMatrix LoadMatrix(string name = null)
		{
			IDensityMatrix matrix;
			FractalConfig conf;
			string a = EnsureEndsWith(name ?? Options.FileName, ".dm");
			Logger.PrintInfo("loading matrix file [" + a + "]");
			FileHelpers.LoadFromFile(a,out matrix,out conf);
			Options.Width = matrix.Width;
			Options.Height = matrix.Height;
			Options.OptionsFromConfig(conf);
			return matrix;
		}

		static void PaintImageData(IDensityMatrix matrix, IColorMap cm, ICanvas img, ColorComponent comp = ColorComponent.None)
		{
			double lm = matrix.Maximum;
			double ln = double.MaxValue;

			// find minimum
			long totalm = Options.Height * Options.Width;
			using (var progress = Logger.CreateProgress(totalm))
			{
				for (int y = 1; y < Options.Height - 1; y++) {
					for (int x = 1; x < Options.Width - 1; x++) {
						double li = matrix[x, y];
						if (li > 0.0 && li < ln) { ln = li; }
						progress.Update("Minimum");
					}
				}
				Debug.WriteLine("ln = "+ln);
			}

			//chop at most frequent value
			//double hmax = 0.0;
			//int cmax = 0;
			//var histogram = new Dictionary<double,int>();
			//for (int y = 1; y < Options.Height - 1; y++) {
			//	for (int x = 1; x < Options.Width - 1; x++) {
			//		double li = matrix[x, y];
			//		if (li < double.Epsilon || double.IsInfinity(li) || double.IsNaN(li)) { continue; }
			//		if (!histogram.TryGetValue(li,out int val)) {
			//			val = 1;
			//		} else {
			//			val++;
			//		}
			//		if (val > cmax) {
			//			cmax = val;
			//			hmax = li;
			//		}
			//		histogram[li] = val;
			//	}
			//}
			// Debug.WriteLine("hmax = "+hmax+" cmax = "+cmax);

			//find minimum method using average of blocks
			//double ln = double.MaxValue;
			//int aspect = 32;
			//int aw = (Options.Width - 1) / aspect;
			//int ah = (Options.Height - 1) / aspect;
			//for (int ay = 1; ay < ah; ay++) {
			//	for (int ax = 1; ax < aw; ax++) {
			//		int ys = ay * aspect; int ye = ys + aspect - 1;
			//		int xs = ax * aspect; int xe = xs + aspect - 1;
			//		double avg = 0.0;
			//		for(int y = ys; y < ye; y++) {
			//			for(int x = xs; x < xe; x++) {
			//				if (x >= 0 && x < Options.Width && y >=0 && y < Options.Height) {
			//					double li = matrix[x, y];
			//					avg += li;
			//				}
			//			}
			//		}
			//		avg /= (aspect * aspect);
			//		if (avg > 0.0 && avg < ln) { ln = avg; }
			//	}
			//}
			//Debug.WriteLine("ln = "+ln);

			//find average method
			//double total = 0.0;
			//long count = 0;
			//for (int y = 1; y < Options.Height - 1; y++) {
			//	for (int x = 1; x < Options.Width - 1; x++) {
			//		double li = matrix[x, y];
			//		if (li >= 0.0 && !double.IsInfinity(li) && !double.IsNaN(li)) {
			//			total += li;
			//			count++;
			//		}
			//	}
			//}
			//double ln = total / count;
			//Debug.WriteLine("ln = "+ln);

			long total = Options.Height * Options.Width;
			using (var progress = Logger.CreateProgress(total))
			{
				double spp = Options.FractalSamples;
				for (int y = 0; y < Options.Height; y++) {
					for (int x = 0; x < Options.Width; x++) {
						double li = matrix[x, y];
						ColorD c = cm.GetColor((li - ln)/spp, (lm - ln)/spp);
						if (comp != ColorComponent.None) {
							img.SetPixelComponent(x,y,comp,c.GetComponent(comp));
						} else {
							img.SetPixel(x, y, c);
						}
						progress.Update("Image");
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
			var conf = Options.ConfigFromOptions();
			conf.IterMax = iters;

			IDensityMatrix matrix = null;
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
