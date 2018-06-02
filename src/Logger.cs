using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IProgress : IDisposable
	{
		void Update(string message,double amount);
	}

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

		public static IProgress CreateProgress()
		{
			return new Progress();
		}

		class Progress : IProgress
		{
			Stopwatch progressTimer = Stopwatch.StartNew();
			bool DidPrint = false;
			public void Update(string message, double amount)
			{
				if (progressTimer.ElapsedMilliseconds < 500) { return; }
				DidPrint = true;
				string txt = ("\r"+(amount * 100).ToString("##.0")+"% "+message);
				Console.Write(txt + new String(' ',Console.BufferWidth - txt.Length));
				progressTimer.Restart();
			}

			public void Dispose()
			{
				if (DidPrint) {
					Console.WriteLine();
				}
			}
		}
	}
}
