using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	#if false
	public class DoubleArray2
	{
		public DoubleArray2(int w,int h)
		{
			Width = w;
			Height = h;
			string fn = Path.Combine(Environment.CurrentDirectory,
				"ArrayTemp"+DateTime.Now.ToString("yyyyMMddHHmmss"));
			data = new MMDataStructures.Array<double>(w*h,fn);
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public double this[int x,int y] {
			get {
				long index = y * Height + x;
				return data[index];
			}
			set {
				long index = y * Height + x;
				data[index] = value;
			}
		}

		public void IncrementByOne(int x, int y) {
			long index = y * Height + x;
			data[index] += 1;
		}

		MMDataStructures.Array<double> data;
	}
	#endif

	public class DoubleArray : IDisposable
	{
		public DoubleArray(int w,int h)
		{
			Width = w;
			Height = h;
			tempFileName = Path.Combine(Environment.CurrentDirectory,
				"ArrayTemp"+DateTime.Now.ToString("yyyyMMddHHmmss"));
			store = File.Open(tempFileName,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.Read);
			cache = new LRUCache<long, double>(8 * 1024 * 1024);
			cache.ItemRemoved += Cache_ItemRemoved;
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public double this[int x,int y] {
			get {
				long index = y * Height + x;
				double val;
				//TODO need to lock or a getoradd method
				if (cache.TryGet(index,out val)) {
					return val;
				} else {
					val = Read(index);
					cache.AddOrUpdate(index,val);
					return val;
				}
			}
			set {
				long index = y * Height + x;
				cache.AddOrUpdate(index,value);
			}
		}

		static byte[] bufBuff = new byte[sizeof(double)];
		private double Read(long index)
		{
			bufBuff.Initialize();
			lock (fileLock) {
				store.Seek(index * SizeOfDouble,SeekOrigin.Begin);
				store.Read(bufBuff,0,SizeOfDouble);
			}
			return BitConverter.ToDouble(bufBuff,0);
		}

		private void Write(long index,double val)
		{
			byte[] buff = BitConverter.GetBytes(val);
			lock (fileLock) {
				store.Seek(index * SizeOfDouble,SeekOrigin.Begin);
				store.Write(buff,0,SizeOfDouble);
			}
		}

		public void IncrementByOne(int x, int y) {
			this[x,y] += 1;
		}

		private void Cache_ItemRemoved(object sender, LRUCache<long, double>.ItemRemovedArgs e)
		{
			Write(e.Key,e.Value);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				if (store != null) {
					store.Close();
				}
				if (File.Exists(tempFileName)) {
					File.Delete(tempFileName);
				}
			}
		}

		FileStream store;
		int SizeOfDouble = sizeof(double);
		object fileLock = new object();
		LRUCache<long,double> cache;
		string tempFileName = null;
	}
}
