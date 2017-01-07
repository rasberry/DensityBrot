﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
	public static class Helpers
	{
		public static Bitmap GetOneByOne()
		{
			byte[] obopng = {
				0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
				0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
				0x08, 0x04, 0x00, 0x00, 0x00, 0xB5, 0x1C, 0x0C, 0x02, 0x00, 0x00, 0x00,
				0x0B, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0xFA, 0xCF, 0x00, 0x00,
				0x02, 0x07, 0x01, 0x02, 0x9A, 0x1C, 0x31, 0x71, 0x00, 0x00, 0x00, 0x00,
				0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
			};
			var ms = new MemoryStream(obopng);
			Bitmap bmp = new Bitmap(ms);
			return bmp;
		}

		public static bool AreColorsEqual(Color a, Color b)
		{
			return a.ToArgb() == b.ToArgb();
		}
	}
}