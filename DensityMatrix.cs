using HugeStructures.TitanicArray;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public interface IDensityMatrix
	{
		int Width { get; }
		int Height { get; }
		long this[int x,int y] { get; set; }
		void Touch(int x,int y);
		long Maximum { get; }
		void SaveToFile(string name = null);
	}

	public class DensityMatrix : IDensityMatrix, IDisposable
	{
		public DensityMatrix(int width, int height)
		{
			Width = width;
			Height = height;
			CreateArray(MakeConfig(Width,Height));
		}

		public DensityMatrix(string matrixFile)
		{
			this.matrixFile = matrixFile;
			if (matrixFile != null && File.Exists(matrixFile)) {
				LoadDataFromFile(matrixFile);
			}
		}

		public long this[int x, int y] {
			get {
				long offset = (long)y * Width + x;
				return data[offset];
			}
			set {
				long offset = (long)y * Width + x;
				if (value > Maximum) { Maximum = value; }
				data[offset] = value;
			}
		}

		public void SaveToFile(string name = null)
		{
			SaveDataToFile(data,name ?? matrixFile);
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

		public long Maximum { get; private set; }
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

		void CreateArray(ITitanicArrayConfig<long> config)
		{
			//return new TitanicFileArray<long>(config);
			data = new TitanicMMFArray<long>(config);
		}

		void LoadDataFromFile(string name)
		{
			using(var fs = File.Open(name,FileMode.Open,FileAccess.Read,FileShare.Read))
			using(var gz = new GZipStream(fs,CompressionLevel.Optimal))
			{
				ReadHeader(gz,out int w,out int h, out long m);
				CreateArray(MakeConfig(w,h));
				Maximum = m;

				byte[] buff = new byte[sizeof(long)];
				for(long i=0; i<data.Length; i++) {
					gz.Read(buff,0,sizeof(long));
					long item = BitConverter.ToInt64(buff,0);
					data[i] = item;
				}
			}
		}
		void SaveDataToFile(ITitanicArray<long> data, string name)
		{
			using(var fs = File.Open(name,FileMode.Create,FileAccess.Write,FileShare.Read))
			using(var gz = new GZipStream(fs,CompressionMode.Decompress))
			{
				WriteHeader(gz);

				for(long i=0; i<data.Length; i++) {
					long item = data[i];
					byte[] bs = BitConverter.GetBytes(item);
					gz.Write(bs,0,sizeof(long));
				}
			}
		}

		void WriteHeader(Stream s)
		{
			var list = new List<byte>();
			list.AddRange(BitConverter.GetBytes('D'));
			list.AddRange(BitConverter.GetBytes('M'));
			list.AddRange(BitConverter.GetBytes(Width));
			list.AddRange(BitConverter.GetBytes(Height));
			list.AddRange(BitConverter.GetBytes(Maximum));
			var header = list.ToArray();

			s.Write(header,0,header.Length);
		}

		void ReadHeader(Stream s,out int width, out int height, out long maximum)
		{
			int len = 2 * sizeof(char) + 2 * sizeof(int) + 1 * sizeof(long);
			byte[] buff = new byte[len];
			s.Read(buff,0,len);

			int off = 2 * sizeof(char);
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
		string matrixFile;
	}
}
