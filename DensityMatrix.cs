using HugeStructures.TitanicArray;
using System;
using System.Collections.Generic;
using System.IO;
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
	}

	public class DensityMatrix : IDensityMatrix, IDisposable
	{
		public DensityMatrix(int width, int height, string backingFile = null)
		{
			Width = width;
			Height = height;

			var config = new TitanicArrayConfig<long> {
				BackingStoreFileName = backingFile ?? GetDefaultFileName(),
				Capacity = (long)Width * Height,
				IsTemporary = false,
				DataSerializer = new LongSerializer()
			};
			//data = new TitanicFileArray<long>(config);
			data = new TitanicMMFArray<long>(config);
			//data = new TitanicIMArray<long>(config);
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
				Environment.CurrentDirectory,
				"DM"+DateTime.Now.ToString("yyyyMMddHHmmss")+".dm"
			);
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
	}
}
