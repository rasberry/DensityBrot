using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using DensityBrot;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace Test
{
	[TestClass]
	public class UnitTests
	{
		//[TestMethod]
		public void PebbleLockTestOne()
		{
			Trace.WriteLine("PebbleLockTestOne");
			var list = new List<Task>();
			for(int i=0; i<8; i++)
			{
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
			//Trace.WriteLine("starting "+n+" "+System.Threading.Thread.CurrentThread.ManagedThreadId);
			for(int i=0; i<1000;i++) {
				var key = new KeyValuePair<int,int>(1,2);
				using (var pb = new PebbleLock<KeyValuePair<int,int>>(key)) {
					//we're using the same key so only one thread should ever be able to
					// affect the static var
					//Trace.WriteLine("adding "+n+" @"+i+" "+System.Threading.Thread.CurrentThread.ManagedThreadId);
					PebbleLockTestOneTaskBeef += n;
					Assert.AreEqual(n,PebbleLockTestOneTaskBeef);
					Thread.Sleep(1); //give scheduler a chance
					//Trace.WriteLine("subtracting "+n+" @"+i+" "+System.Threading.Thread.CurrentThread.ManagedThreadId);
					PebbleLockTestOneTaskBeef -= n;
					Assert.AreEqual(0,PebbleLockTestOneTaskBeef);
				}
			}
		}

		//[TestMethod]
		public void PebbleLockTestTwo()
		{
			Trace.WriteLine("PebbleLockTestTwo");
			var list = new List<Task>();
			PebbleLockTestTwoTaskBeef = new int[1000];
			for(int n=0; n<8; n++)
			{
				var t = new Task(PebbleLockTestTwoTask,n);
				t.Start();
				list.Add(t);
			}
			Task.WaitAll(list.ToArray());
		}

		static int[] PebbleLockTestTwoTaskBeef;
		void PebbleLockTestTwoTask(object o)
		{
			int n = (int)o;
			for(int i=0; i<1000;i++) {
				var key = new KeyValuePair<int,int>(i,0);
				using (var pb = new PebbleLock<KeyValuePair<int,int>>(key)) {
					//we're using the same key so only one thread should ever be able to
					// affect the same bucket
					//Trace.WriteLine("adding "+n+" to "+i+" "+System.Threading.Thread.CurrentThread.ManagedThreadId);
					PebbleLockTestTwoTaskBeef[i] += n;
					Assert.AreEqual(n,PebbleLockTestTwoTaskBeef[i]);
					Thread.Sleep(1); //give scheduler a chance
					//Trace.WriteLine("subtracting "+n+" to "+i+" "+System.Threading.Thread.CurrentThread.ManagedThreadId);
					PebbleLockTestTwoTaskBeef[i] -= n;
					Assert.AreEqual(0,PebbleLockTestTwoTaskBeef[i]);
				}
			}
		}

		[TestMethod]
		public void PebbleLockTestThree()
		{
			Trace.WriteLine("PebbleLockTestThree");
			try {
				var pb = new PebbleLock<int>(0);
			} catch(Exception e) {
				Assert.IsInstanceOfType(e,typeof(InvalidOperationException));
			} finally {
				//need to reset the static vars for next test
				ConstructorInfo constructor = typeof(PebbleLock<int>).GetConstructor(BindingFlags.Static | BindingFlags.NonPublic,null, new Type[0], null);
				constructor.Invoke(null, null);
				Trace.WriteLine("PebbleLock<int> Reset");
			}
		}

		[TestMethod]
		public void PebbleLockTestFour()
		{
			Trace.WriteLine("PebbleLockTestFour");
			try {
				using (var pb = new PebbleLock<int>(0)) {
					PebbleLockTestOneTaskBeef += 1;
					Assert.AreEqual(1,PebbleLockTestOneTaskBeef);
					Thread.Sleep(1); //give scheduler a chance
					PebbleLockTestOneTaskBeef -= 1;
					Assert.AreEqual(0,PebbleLockTestOneTaskBeef);
				}
			} catch {
				Assert.Fail();
			}
		}
	}
}
