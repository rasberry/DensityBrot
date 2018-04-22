using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

			if (ShouldCreateOrbits) {
				ProduceOrbits();
			} else {
				//var ren = new Render();
				//var bmp = new MagicCanvas(Width,Height);
				var conf = new FractalConfig {
					Escape = 4.0,
					Plane = Planes.XY,
					Resolution = Resolution,
					X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
					IterMax = 1000,
					OffsetX = Width/2,
					OffsetY = Height/2
				};

				using (var matrix = new DensityMatrix(Width,Height))
				{
					var builder = new FractalBuilder(matrix,conf);
					builder.Build();
				}

				//Logger.PrintInfo("Rendering Image ["+Width+"x"+Height+"] @"+Resolution);
				//ren.RenderToCanvas(bmp,conf);
				//Logger.PrintInfo("Saving "+FileName+".png");
				//bmp.SavePng(FileName+".png");
			}
		}

		static void ProduceOrbits() {
			//TODO implement
		}

		static bool ShouldCreateOrbits = false;
		static int Width = -1;
		static int Height = -1;
		static string FileName = null;
		static double Resolution = 200;
		static bool ShowVerbose = false;

		static bool ProcessArgs(string[] args)
		{
			bool showHelp = false;
			bool noChecks = false;
			for(int a=0; a<args.Length; a++)
			{
				string c = args[a];
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
					ShouldCreateOrbits = true;
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
				else
				{
					FileName = c;
				}
			}

			if (!noChecks)
			{
				//sanity checks
				if (String.IsNullOrWhiteSpace(FileName)) {
					Logger.PrintError("Missing filename / prefix");
					showHelp = true;
				}
				if (Width < 1 || Height < 1) {
					Logger.PrintError("output image size is invalid");
					showHelp = true;
				}
				if (Resolution < double.Epsilon) {
					Logger.PrintError("Resolution must be greater than zero");
					showHelp = true;
				}
			}

			if (showHelp) {
				Logger.PrintLine("\n"+nameof(DensityBrot)+" [options] (filename / prefix)"
					+"\nOptions:"
					+"\n --help / -h                       Show this help"
					+"\n -v                                Print additional progress messages"
					+"\n -d (width) (height)               Size of image output images in pixels"
					+"\n -r (resolution)                   Scale factor (Default: 200. 400 = 2x bigger)"
					+"\n -o                                Output orbits instead of dentisy plot"
					+"\n                                    (Warning: produces one image per coordinate)"
				);
			}
			return !showHelp;
		}
	}
}
