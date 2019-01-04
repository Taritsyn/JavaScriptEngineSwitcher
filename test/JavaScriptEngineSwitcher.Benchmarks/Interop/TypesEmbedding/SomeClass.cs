using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace JavaScriptEngineSwitcher.Benchmarks.Interop.TypesEmbedding
{
	public static class SomeClass
	{
		public static bool Field1;
		public static int Field2;
		public static double Field3;
		public static string Field4;
		public static Point Field5;

		public static bool Property1 { get; set; }
		public static int Property2 { get; set; }
		public static double Property3 { get; set; }
		public static string Property4 { get; set; }
		public static SomeOtherClass Property5 { get; set; }


		public static int DoSomething(bool arg1, int arg2, double arg3, string arg4)
		{
			int result = Convert.ToInt32(arg1) +
				arg2 +
				(int)Math.Round(arg3) +
				Encoding.UTF8.GetBytes(arg4).Sum(x => x);
				;

			return result;
		}
	}
}