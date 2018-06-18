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
		public void MersenneTwisterTest10k()
		{
			var mt = new MersenneTwister();
			ulong v = 0;
			for(int i=0; i<10000;i++) {
				v = mt.Extract();
				Trace.WriteLine("["+i+"] = "+v);
			}
			Assert.IsTrue(9981545732273789042 == v);
		}

		//[TestMethod]
		public void MersenneTwisterTest10kSeed0()
		{
			var mt = new MersenneTwister(0);
			ulong v = 0;
			for(int i=0; i<10000;i++) {
				v = mt.Extract();
				Trace.WriteLine("["+i+"] = "+v);
			}
			Assert.IsTrue(16335088777103562557 == v);
		}

		//[TestMethod]
		public void MersenneTwisterTest10kSeedN0()
		{
			var mt = new MersenneTwister(~0uL);
			ulong v = 0;
			for(int i=0; i<10000;i++) {
				v = mt.Extract();
				Trace.WriteLine("["+i+"] = "+v);
			}
			Assert.IsTrue(898929940823410802 == v);
		}

		[TestMethod]
		public void UniqueRandomTest1()
		{
			var ur = new UniqueRandom();
			double v = 0;
			for(int i=0; i<10000;i++) {
				v = ur.NextDouble();
				Trace.WriteLine("["+i+"] = "+v);
			}
		}
	}
}
