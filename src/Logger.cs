using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IProgress : IDisposable
	{
		void Update(string message);
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

		public static IProgress CreateProgress(long total)
		{
			return new Progress(total);
		}

		class Progress : IProgress
		{
			public Progress(long max)
			{
				Count = 0;
				Max = max;
			}

			long Max;
			long Count;
			Stopwatch progressTimer = Stopwatch.StartNew();
			bool DidPrint = false;
			object sliceLock = new object();
			public void Update(string message)
			{
				Interlocked.Increment(ref Count);
				if (progressTimer.ElapsedMilliseconds < 500) { return; } //i'm assuming this is thread safe
				lock(sliceLock) {
					if (progressTimer.ElapsedMilliseconds >= 500) {
						progressTimer.Restart();
						double amount = (double)Count / Max;
						string txt = ("\r"+(amount * 100).ToString("##.0")+"% "+message);
						Console.Write(txt + new String(' ',Console.BufferWidth - txt.Length));
					}
				}
				DidPrint = true;
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
