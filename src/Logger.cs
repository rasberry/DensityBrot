using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public static class Logger
	{
		public static bool ShowVerbose { get; set; }

		public static void PrintLine(string message)
		{
			Console.WriteLine(message);
		}
		public static void PrintError(string message)
		{
			Console.Error.WriteLine("E: "+message);
		}
		public static void PrintInfo(string message)
		{
			if (ShowVerbose) {
				Console.WriteLine("I: "+message);
			}
		}
	}
}
