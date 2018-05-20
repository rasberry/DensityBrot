using System;
using System.IO;

namespace DensityBrot
{
	public static class FileHelpers
	{
		public static void SaveToFile(string name, IDensityMatrix matrix,FractalConfig conf)
		{
			using(var fs = File.Open(name,FileMode.Create,FileAccess.Write,FileShare.Read))
			{
				long off = 0;
				off += DoWrite(fs,BitConverter.GetBytes('D'));
				off += DoWrite(fs,BitConverter.GetBytes('M'));
				var sconf = new MemoryStream();
				off += conf.ToStream(sconf);
				off += sizeof(long); //also need to skip the offset value itself
				DoWrite(fs,BitConverter.GetBytes(off));
				sconf.Seek(0,SeekOrigin.Begin);
				sconf.CopyTo(fs);
				matrix.ToStream(fs);
			}
		}

		public static void LoadFromFile(string name,out IDensityMatrix matrix, out FractalConfig conf)
		{
			using(var fs = File.Open(name,FileMode.Open,FileAccess.Read,FileShare.Read))
			{
				DoRead(fs,out char d);
				DoRead(fs,out char m);
				DoRead(fs,out long off);
				conf = FractalConfig.FromStream(fs);
				fs.Seek(off,SeekOrigin.Begin);
				matrix = DensityMatrix.FromStream(fs);
			}
		}

		public static int DoWrite(this Stream s, byte[] buff)
		{
			s.Write(buff,0,buff.Length);
			return buff.Length;
		}
		public static int DoRead<T>(this Stream s, out T val)
		{
			int len = FileHelpers.SizeOf<T>();
			byte[] buff = new byte[len];
			int read = s.Read(buff,0,len);
			FileHelpers.BytesToValue(buff,out val);
			return read;
		}

		public static void BytesToValue<T>(byte[] data, out T val)
		{
			var tc = Type.GetTypeCode(typeof(T));
			switch(tc)
			{
			case TypeCode.Boolean:
				val = (T)(object)BitConverter.ToBoolean(data,0);
				break;
			case TypeCode.Char:
				val = (T)(object)BitConverter.ToChar(data,0);
				break;
			case TypeCode.Double:
				val = (T)(object)BitConverter.ToDouble(data,0);
				break;
			case TypeCode.Single:
				val = (T)(object)BitConverter.ToSingle(data,0);
				break;
			case TypeCode.Int16:
				val = (T)(object)BitConverter.ToInt16(data,0);
				break;
			case TypeCode.Int32:
				val = (T)(object)BitConverter.ToInt32(data,0);
				break;
			case TypeCode.Int64:
				val = (T)(object)BitConverter.ToInt64(data,0);
				break;
			case TypeCode.UInt16:
				val = (T)(object)BitConverter.ToUInt16(data,0);
				break;
			case TypeCode.UInt32:
				val = (T)(object)BitConverter.ToUInt32(data,0);
				break;
			case TypeCode.UInt64:
				val = (T)(object)BitConverter.ToUInt64(data,0);
				break;
			case TypeCode.String:
				val = (T)(object)BitConverter.ToString(data,0);
				break;
			default:
				val = default(T);
				throw new NotSupportedException(typeof(T).FullName+" is not supported");
			}
		}
		public static int SizeOf<T>()
		{
			var tc = Type.GetTypeCode(typeof(T));
			switch(tc)
			{
			case TypeCode.Boolean: return sizeof(bool);
			case TypeCode.Char:    return sizeof(char);
			case TypeCode.Double:  return sizeof(double);
			case TypeCode.Single:  return sizeof(float);
			case TypeCode.Int16:   return sizeof(short);
			case TypeCode.Int32:   return sizeof(int);
			case TypeCode.Int64:   return sizeof(long);
			case TypeCode.UInt16:  return sizeof(ushort);
			case TypeCode.UInt32:  return sizeof(uint);
			case TypeCode.UInt64:  return sizeof(ulong);
			default:
				throw new NotSupportedException(typeof(T).FullName+" is not supported");
			}
		}
	}
}