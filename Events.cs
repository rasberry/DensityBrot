using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	public class RenderFinishedEventArgs : EventArgs
	{
		public RenderFinishedEventArgs(Bitmap img, double seconds)
		{
			TotalSeconds = seconds;
			Image = img;
		}

		public double TotalSeconds { get; private set; }
		public Bitmap Image { get; private set; }
	}

	public class ConfigChangedEventArgs : EventArgs
	{
		public ConfigChangedEventArgs(FractalConfig conf)
		{
			Config = conf;
		}
		public FractalConfig Config { get; private set; }
	}

	public class ScrollChangedEventArgs : EventArgs
	{
		public ScrollChangedEventArgs(Point offset)
		{
			Offset = offset;
		}
		public Point Offset { get; private set; }
	}

	public class CanvasSizeChangedEventArgs : EventArgs
	{
		public CanvasSizeChangedEventArgs(Size size)
		{
			Size = size;
		}
		public Size Size { get; private set; }
	}
}
