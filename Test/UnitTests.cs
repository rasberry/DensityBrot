using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using DensityBrot;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
	public class UnitTests
	{
		[TestMethod]
		public void PebbleLockTestOne()
		{
			Console.WriteLine("PebbleLockTestOne");
			var list = new List<Task>();
			for(int i=0; i<8; i++)
			{
				Console.WriteLine("making "+i);
				var t = new Task(PebbleLockTestOneTask,i);
				t.Start();
				list.Add(t);
			}
			Task.WaitAll(list.ToArray());
		}

		static int PebbleLockTestOneTaskBeef = 0;
		void PebbleLockTestOneTask(object o)
		{
			int n = (int)o;
			for(int i=0; i<1000;i++) {
				var key = new KeyValuePair<int,int>(1,2);
				using (var pb = new PebbleLock<KeyValuePair<int,int>>(key)) {
					//we're using the same key so only one thread should ever be able to affect the static var
					PebbleLockTestOneTaskBeef += n;
					Assert.AreEqual(n,PebbleLockTestOneTaskBeef);
					Thread.Sleep(1); //give scheduler a chance
					PebbleLockTestOneTaskBeef -= n;
					Assert.AreEqual(0,PebbleLockTestOneTaskBeef);
				}
			}
		}
	}
}
