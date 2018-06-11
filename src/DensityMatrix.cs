using HugeStructures.TitanicArray;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IDensityMatrix : ISaveable, IDisposable
	{
		int Width { get; }
		int Height { get; }
		long this[int x,int y] { get; set; }
		void Touch(int x,int y);
		long Maximum { get; }
	}

	public class DensityMatrix : IDensityMatrix
	{
		private DensityMatrix()
		{
		}
		public DensityMatrix(int width, int height)
		{
			Width = width;
			Height = height;
			data = CreateArray(MakeConfig(Width,Height));
		}

		public long this[int x, int y] {
			get {
				long offset = (long)y * Width + x;
				//using(var pb = new PebbleLock<long>(offset)) {
					return data[offset];
				//}
			}
			set {
				long offset = (long)y * Width + x;
				if (value > maximum) {
					lock(maxLock) {
						if (value > maximum) {
							maximum = value;
						}
					}
				}
				//using (var pb = new PebbleLock<long>(offset)) {
					data[offset] = value;
				//}
			}
		}

		public long ToStream(Stream writeStream)
		{
			return WriteToStream(writeStream);
		}

		public static IDensityMatrix FromStream(Stream readStream)
		{
			var m = new DensityMatrix();
			m.InitFromStream(readStream);
			return m;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool dispose)
		{
			if (dispose) {
				data.Dispose();
			}
		}

		public long Maximum { get { return maximum; }}
		public void Touch(int x,int y) { this[x,y]++; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		static string GetDefaultFileName()
		{
			return Path.Combine(
				Environment.CurrentDirectory,"_dm-"+new Random().Next() + ".tmp"
			);
		}

		static ITitanicArrayConfig<long> MakeConfig(int width, int height)
		{
			return new TitanicArrayConfig<long> {
				BackingStoreFileName = GetDefaultFileName(),
				Capacity = (long)width * height,
				IsTemporary = true,
				DataSerializer = new LongSerializer()
			};
		}

		static ITitanicArray<long> CreateArray(ITitanicArrayConfig<long> config)
		{
			return new TitanicMMFArray<long>(config);
		}

		void InitFromStream(Stream readStream)
		{
			using(var gz = new GZipStream(readStream,CompressionMode.Decompress))
			{
				ReadHeader(gz,out int w,out int h, out long m);
				data = CreateArray(MakeConfig(w,h));
				Width = w; Height = h; maximum = m;

				byte[] buff = new byte[sizeof(long)];
				for(long i=0; i<data.Length; i++) {
					gz.Read(buff,0,sizeof(long));
					long item = BitConverter.ToInt64(buff,0);
					data[i] = item;
				}
			}
		}
		long WriteToStream(Stream writeStream)
		{
			long len = 0;
			using(var gz = new GZipStream(writeStream,CompressionLevel.Optimal))
			{
				len += WriteHeader(gz);

				for(long i=0; i<data.Length; i++) {
					long item = data[i];
					byte[] bs = BitConverter.GetBytes(item);
					gz.Write(bs,0,sizeof(long));
				}
			}
			return len + data.Length;
		}

		long WriteHeader(Stream s)
		{
			var list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(Width));
			list.AddRange(BitConverter.GetBytes(Height));
			list.AddRange(BitConverter.GetBytes(Maximum));
			var header = list.ToArray();

			s.Write(header,0,header.Length);
			return header.Length;
		}

		void ReadHeader(Stream s,out int width, out int height, out long maximum)
		{
			int len = 2 * sizeof(int) + 1 * sizeof(long);
			byte[] buff = new byte[len];
			s.Read(buff,0,len);

			int off = 0;
			width = BitConverter.ToInt32(buff,off);
			height = BitConverter.ToInt32(buff,(off += sizeof(int)));
			maximum = BitConverter.ToInt64(buff,(off += sizeof(int)));
		}

		class LongSerializer : HugeStructures.IDataSerializer<long>
		{
			public long Deserialize(byte[] bytes)
			{
				if (bytes == null) {
					throw new ArgumentNullException();
				}
				return BitConverter.ToInt64(bytes,0);
			}

			public byte[] Serialize(long item)
			{
				return BitConverter.GetBytes(item);
			}
		}

		ITitanicArray<long> data;

		//need explicit backing field for thead safety
		long maximum;
		object maxLock = new object();
	}
}
