using System;
using System.IO;

namespace DensityBrot
{
	public class FractalConfig : ISaveable
	{
		public double X,Y,W,Z;
		public Planes Plane;
		public double Resolution;
		public double Escape;
		public int IterMax;
		public double OffsetX;
		public double OffsetY;
		public bool HideEscaped;
		public bool HideContained;
		public int SamplesPerPoint;

		public static FractalConfig Default { get {
			return new FractalConfig {
				X = 0.0,Y = 0.0,
				W = 0.0,Z = 0.0,
				Plane = Planes.XY,
				Resolution = 20.0,
				Escape = 4.0,
				IterMax = 100,
				HideContained = false,
				HideEscaped = false,
				SamplesPerPoint = 2
			};
		}}

		public long ToStream(Stream writeStream)
		{
			long len = 0;
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(X));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(Y));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(W));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(Z));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes((int)Plane));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(Resolution));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(Escape));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(IterMax));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(OffsetX));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(OffsetY));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(HideEscaped));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(HideContained));
			len += Helpers.DoWrite(writeStream,BitConverter.GetBytes(SamplesPerPoint));
			return len;
		}

		public static FractalConfig FromStream(Stream readStream)
		{
			int iPlane;
			var fc = new FractalConfig();
			Helpers.DoRead(readStream,out fc.X);
			Helpers.DoRead(readStream,out fc.Y);
			Helpers.DoRead(readStream,out fc.W);
			Helpers.DoRead(readStream,out fc.Z);
			Helpers.DoRead(readStream,out iPlane);
			Helpers.DoRead(readStream,out fc.Resolution);
			Helpers.DoRead(readStream,out fc.Escape);
			Helpers.DoRead(readStream,out fc.IterMax);
			Helpers.DoRead(readStream,out fc.OffsetX);
			Helpers.DoRead(readStream,out fc.OffsetY);
			Helpers.DoRead(readStream,out fc.HideContained);
			Helpers.DoRead(readStream,out fc.HideContained);
			Helpers.DoRead(readStream,out fc.SamplesPerPoint);
			fc.Plane = (Planes)iPlane;
			return fc;
		}
	}
}
