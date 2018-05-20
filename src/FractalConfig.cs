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
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(X));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(Y));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(W));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(Z));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes((int)Plane));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(Resolution));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(Escape));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(IterMax));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(OffsetX));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(OffsetY));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(HideEscaped));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(HideContained));
			len += FileHelpers.DoWrite(writeStream,BitConverter.GetBytes(SamplesPerPoint));
			return len;
		}

		public static FractalConfig FromStream(Stream readStream)
		{
			int iPlane;
			var fc = new FractalConfig();
			FileHelpers.DoRead(readStream,out fc.X);
			FileHelpers.DoRead(readStream,out fc.Y);
			FileHelpers.DoRead(readStream,out fc.W);
			FileHelpers.DoRead(readStream,out fc.Z);
			FileHelpers.DoRead(readStream,out iPlane);
			FileHelpers.DoRead(readStream,out fc.Resolution);
			FileHelpers.DoRead(readStream,out fc.Escape);
			FileHelpers.DoRead(readStream,out fc.IterMax);
			FileHelpers.DoRead(readStream,out fc.OffsetX);
			FileHelpers.DoRead(readStream,out fc.OffsetY);
			FileHelpers.DoRead(readStream,out fc.HideContained);
			FileHelpers.DoRead(readStream,out fc.HideContained);
			FileHelpers.DoRead(readStream,out fc.SamplesPerPoint);
			fc.Plane = (Planes)iPlane;
			return fc;
		}
	}
}
