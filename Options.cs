using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public static class Options
	{
		public enum ProcessMode
		{
			DensityBrot = 0,
			CreateOrbits = 1,
			TestColorMap = 2,
			NebulaBrot = 3
		}

		public static ProcessMode Mode = ProcessMode.DensityBrot;
		public static int Width = -1;
		public static int Height = -1;
		public static string FileName = null;
		public static double Resolution = 4.0;
		public static bool ShowVerbose = false;
		public static bool CreateMatrix = false;
		public static bool CreateImage = false;

		//fractal options
		public static double FractalEscape = 2.0;
		public static int FractalMaxIter = 1000;
		public static int FractalSamples = 1;

		//color map options
		public static SomeColorMaps MapColors = SomeColorMaps.Gray;
		public static string GgrFile = null;
		public static bool HideEscaped = false;
		public static bool HideContained = false;
		public static int NebulaRIter = 5000;
		public static int NebulaGIter = 500;
		public static int NebulaBIter = 50;
		

		public static bool ProcessArgs(string[] args)
		{
			bool showHelp = false;
			bool skipChecks = false;
			for(int a=0; a<args.Length; a++)
			{
				string c = args[a];
				
				//regular options
				if (c == "--help" || c == "-h")
				{
					showHelp = true;
					skipChecks = true;
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

				//color map options
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
				else if (c == "-nb")
				{
					Mode = ProcessMode.NebulaBrot;
					if (a+3 < args.Length) {
						bool good = 
							   int.TryParse(args[a+1],out int NebulaRIter)
							&& int.TryParse(args[a+2],out int NebulaGIter)
							&& int.TryParse(args[a+3],out int NebulaBIter)
						;
						if (good) {
							a+=3;
						}
					}
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
				else if (c == "-fs" && ++a < args.Length)
				{
					string samp = args[a];
					if (!int.TryParse(samp,out FractalSamples) || FractalSamples <= 0) {
						Logger.PrintError("Invalid number of samples "+samp);
						showHelp = true;
					}
				}
				else if (c == "-he")
				{
					HideEscaped = true;
				}
				else if (c == "-hc")
				{
					HideContained = true;
				}

				// filename
				else
				{
					FileName = c;
				}
			}

			if (!skipChecks)
			{
				if (Mode == ProcessMode.DensityBrot || Mode == ProcessMode.NebulaBrot)
				{
					if (!CreateImage && !CreateMatrix) {
						CreateMatrix = true; //default mode
					}
					//sanity checks
					if (CreateMatrix && String.IsNullOrWhiteSpace(FileName)) {
						FileName = "DB-"+DateTime.Now.ToString("yyyyMMddHHmmss");
					}
					if (CreateImage && String.IsNullOrWhiteSpace(FileName)) {
						Logger.PrintError("Missing filename / prefix");
						showHelp = true;
					}
					if (CreateMatrix && Width < 1 && Height < 1) {
						//Width = Height = (int)Math.Ceiling(2 * FractalEscape * Resolution);
						Width = Height = 800;
					}
					if (CreateMatrix && Resolution < double.Epsilon) {
						Logger.PrintError("Resolution must be greater than zero");
						showHelp = true;
					}
				}
				else if (Mode == ProcessMode.TestColorMap)
				{
					if (Width < 1 && Height < 1) {
						Width = 1024; Height = 256;
					}
				}

				if (CreateMatrix && (Width < 1 || Height < 1)) {
					Logger.PrintError("output image [" + Width + "," + Height + "] size is invalid");
					showHelp = true;
				}
			}

			if (showHelp) {
				var sb = new StringBuilder();
				var mapNames = Enum.GetNames(typeof(SomeColorMaps));
				foreach(string mn in mapNames) {
					sb.Append("  ").Append(mn).AppendLine();
				}

				Logger.PrintLine("\n"+nameof(DensityBrot)+" [options] (filename)"
					+"\nOptions:"
					+"\n --help / -h                       Show this help"
					+"\n -v                                Print additional progress messages"
					+"\n -m                                Create a density matrix file (this is the default)"
					+"\n -i                                Read density matrix file and output an image"
					+"\n                                     if -m is also specified both files will be created"
					+"\n -d (width) (height)               Size of image output images in pixels"
					+"\n -r (resolution)                   Scale factor (Default: 4.0; 2.0 = 2x bigger)"
					+"\n -o                                Output orbits instead of dentisy plot"
					+"\n                                    (Warning: produces one image per coordinate)"
					+"\n\nColor Options:"
					+"\n -nb [r-iter g-iter b-iter]        Use nebulabrot coloring (default is 5000 500 50)"
					+"\n                                    (Warning: this creates / requires 3 density matrix files)"
					+"\n -cm (name)                        Use buit-in color map model (Gray is default)"
					+"\n -ggr (ggr file)                   Use a GIMP ggr colormap file for the color map model"
					+"\n -testcm                           Tests a colormap by saving a colormap image instead of a fractal"
					+"\n\nColor Maps:"
					+"\n"+sb.ToString()
					+"\nFractal Controls:"
					+"\n -fe (number)                      escape value (default 4.0)"
					+"\n -fi (number)                      maximum number of iterations (default 1000)"
					+"\n -fs (number)                      number of samples per pixel (defualt 1)"
					+"\n -he                               hide escaped orbits"
					+"\n -hc                               hide contained orbits"
				);
			}
			return !showHelp;
		}
	}
}
