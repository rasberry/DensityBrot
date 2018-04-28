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
			Debug.Listeners.Add(new ConsoleTraceListener());

			try {
				MainMain(args);
			} catch(Exception e) {
				Logger.PrintError(e.ToString());
			}
		}

		static void MainMain(string[] args)
		{
			if (!ProcessArgs(args)) { return; }
			Logger.ShowVerbose = ShowVerbose;

			switch(Mode)
			{
			case ProcessMode.Fractal: CreateFractal(); break;
			case ProcessMode.CreateOrbits: ProduceOrbits(); break;
			case ProcessMode.TestColorMap: CreateColorMapTest(); break;
			}
		}

		static void CreateFractal()
		{
			var conf = new FractalConfig {
				Escape = FractalEscape,
				Plane = Planes.XY,
				Resolution = Resolution,
				X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
				IterMax = FractalMaxIter,
				OffsetX = Width/2,
				OffsetY = Height/2
			};

			DensityMatrix matrix = null;
			try
			{
				if (CreateMatrix) {
					matrix = DoCreateMatrix(conf);
				}
				if (CreateImage) {
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
			DensityMatrix matrix = new DensityMatrix(Width, Height);
			var builder = new FractalBuilder(matrix, conf);
			Logger.PrintInfo("building matrix");
			builder.Build();
			string n = EnsureEndsWith(FileName, ".dm");
			Logger.PrintInfo("saving matrix file [" + n + "]");
			matrix.SaveToFile(n);
			return matrix;
		}

		static DensityMatrix DoCreateImage(DensityMatrix matrix)
		{
			if (matrix == null)
			{
				string a = EnsureEndsWith(FileName, ".dm");
				Logger.PrintInfo("loading matrix file [" + a + "]");
				matrix = new DensityMatrix(a);
				Width = matrix.Width;
				Height = matrix.Height;
			}

			Logger.PrintInfo("matrix = [" + matrix.Width + "x" + matrix.Height + " " + matrix.Maximum + "]");
			IColorMap cm = new FullRangeRGBColorMap();
			var img = new MagicCanvas(Width, Height);
			Logger.PrintInfo("building image");
			double lm = Math.Log(matrix.Maximum);
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					double li = Math.Log(matrix[x,y]);
					Color c = cm.GetColor(li, lm);
					img.SetPixel(x, y, c);
				}
			}
			string n = EnsureEndsWith(FileName, ".png");
			Logger.PrintInfo("saving image file [" + n + "]");
			img.SavePng(n);
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
			IColorMap cmap;
			string name;
			if (!String.IsNullOrWhiteSpace(GgrFile)) {
				cmap = ColorHelpers.GetGimpColorMap(GgrFile);
				name = Path.GetFileNameWithoutExtension(GgrFile);
			} else {
				cmap = ColorHelpers.GetColorMap(MapColors);
				name = MapColors.ToString();
			}

			using (var mi = new MagickImage(MagickColors.Transparent,Width,Height))
			{
				mi.ColorType = ColorType.TrueColorAlpha;
				mi.Alpha(AlphaOption.Transparent);
				mi.ColorAlpha(MagickColors.Transparent);

				var d = new Drawables();
				
				for(int x=0; x<Width; x++)
				{
					Color c = cmap.GetColor(x,Width);
					d.FillColor(c);
					d.Line(x,0,x,Height-1);
				}
				d.Draw(mi);
				var fs = File.Open("ColorMapTest-"+name+".png",FileMode.Create,FileAccess.Write,FileShare.Read);
				mi.Write(fs,MagickFormat.Png32);
			}
		}

		enum ProcessMode
		{
			Fractal = 0,
			CreateOrbits = 1,
			TestColorMap = 2
		}

		static ProcessMode Mode = ProcessMode.CreateOrbits;
		static int Width = -1;
		static int Height = -1;
		static string FileName = null;
		static double Resolution = 200;
		static bool ShowVerbose = false;
		static bool CreateMatrix = false;
		static bool CreateImage = false;
		static double FractalEscape = 4;
		static int FractalMaxIter = 1000;
		static SomeColorMaps MapColors = SomeColorMaps.Gray;
		static string GgrFile = null;

		static bool ProcessArgs(string[] args)
		{
			bool showHelp = false;
			bool noChecks = false;
			for(int a=0; a<args.Length; a++)
			{
				string c = args[a];
				
				//regular options
				if (c == "--help" || c == "-h")
				{
					showHelp = true;
					noChecks = true;
				}
				else if (c == "-v")
				{
					ShowVerbose = true;
				}
				else if (c == "-o")
				{
					Mode = ProcessMode.CreateOrbits;
				}
				else if (c == "-i")
				{
					CreateImage = true;
				}
				else if (c == "-m")
				{
					CreateMatrix = true;
				}
				else if (c == "-d" && (a += 2) < args.Length)
				{
					string sw = args[a - 1];
					string sh = args[a - 0];
					if (!int.TryParse(sw, out Width))
					{
						Logger.PrintError("Invalid width " + sw);
						showHelp = true;
					}
					if (!int.TryParse(sh, out Height))
					{
						Logger.PrintError("Invalid height " + sh);
						showHelp = true;
					}
				}
				else if (c == "-r" && ++a < args.Length)
				{
					string res = args[a];
					if (!double.TryParse(res,out Resolution)) {
						Logger.PrintError("Invalid resolution "+res);
						showHelp = true;
					}
				}
				else if (c == "-cm" && ++a < args.Length)
				{
					if (!Enum.TryParse(args[a],true,out MapColors)) {
						Logger.PrintError("Invalid color map "+args[a]);
						showHelp = true;
					}
				}
				else if (c == "-ggr" && ++a < args.Length)
				{
					GgrFile = args[a];
				}
				else if (c == "-testcm")
				{
					Mode = ProcessMode.TestColorMap;
				}

				// fractal options
				else if (c == "-fe" && ++a < args.Length)
				{
					string esc = args[a];
					if (!double.TryParse(esc,out FractalEscape) || FractalEscape <= 0.0) {
						Logger.PrintError("Invalid escape value "+esc);
						showHelp = true;
					}
				}
				else if (c == "-fi" && ++a < args.Length)
				{
					string iter = args[a];
					if (!int.TryParse(iter,out FractalMaxIter) || FractalMaxIter <= 0) {
						Logger.PrintError("Invalid number of maximum iterations "+iter);
						showHelp = true;
					}
				}

				// filename
				else
				{
					FileName = c;
				}
			}

			if (!noChecks)
			{
				if (Mode == ProcessMode.Fractal)
				{
					if (!CreateImage && !CreateMatrix) {
						CreateMatrix = true; //default mode
					}
					//sanity checks
					if (String.IsNullOrWhiteSpace(FileName)) {
						Logger.PrintError("Missing filename / prefix");
						showHelp = true;
					}
					if (CreateMatrix && Width < 1 && Height < 1) {
						Width = Height = (int)Math.Ceiling(2 * FractalEscape * Resolution);
					}
					if (CreateMatrix && Resolution < double.Epsilon) {
						Logger.PrintError("Resolution must be greater than zero");
						showHelp = true;
					}
				}
				else if (Mode == ProcessMode.TestColorMap)
				{
					if (Width < 1 && Height < 1) {
						Width = 1024; Height= 256;
					}
				}

				if (CreateMatrix && (Width < 1 || Height < 1)) {
					Logger.PrintError("output image size is invalid");
					showHelp = true;
				}
			}

			if (showHelp) {
				var sb = new StringBuilder();
				var mapNames = Enum.GetNames(typeof(SomeColorMaps));
				foreach(string mn in mapNames) {
					sb.Append("  ").Append(mn).AppendLine();
				}

				Logger.PrintLine("\n"+nameof(DensityBrot)+" [options] (filename / prefix)"
					+"\nOptions:"
					+"\n --help / -h                       Show this help"
					+"\n -v                                Print additional progress messages"
					+"\n -m [filaneme]                     Create a density matrix file (this is the default)"
					+"\n -i [filename]                     Read density matrix file and output an image"
					+"\n                                     if -m is also specified both files will be created"
					+"\n -d (width) (height)               Size of image output images in pixels"
					+"\n -r (resolution)                   Scale factor (Default: 200. 400 = 2x bigger)"
					+"\n -o                                Output orbits instead of dentisy plot"
					+"\n                                    (Warning: produces one image per coordinate)"
					+"\n -cm (name)                        Use buit-in color map model (Gray is default)"
					+"\n -ggr (ggr file)                   Use a GIMP ggr colormap file for the color map model"
					+"\n -testcm                           Tests a colormap by saving a colormap image instead of a fractal"
					+"\n\nColor Maps:"
					+"\n"+sb.ToString()
					+"\nFractal Controls:"
					+"\n -fe (number)                      escape value (default 4.0)"
					+"\n -fi (number)                      maximum number of iterations (default 1000)"
				);
			}
			return !showHelp;
		}
	}
}
