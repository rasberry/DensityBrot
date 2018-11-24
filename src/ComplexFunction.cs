using System;
using System.Numerics;
using System.Reflection;

namespace DensityBrot
{
	public static class ComplexFunction
	{
		public static Complex IterateComplexFunction(Complex z, Complex c)
		{
			z = z * z + c;
			return z;
		}

		static void Test()
		{
			//System.Reflection.Emit.MethodBuilder mb;
			//mb.ReturnType = typeof(Complex);

		}
	}
}
