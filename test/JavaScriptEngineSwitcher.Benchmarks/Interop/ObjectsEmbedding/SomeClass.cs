using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace JavaScriptEngineSwitcher.Benchmarks.Interop.ObjectsEmbedding
{
	public class SomeClass : SomeClassBase
	{
		public Point Field5;

		public SomeOtherClass Property5 { get; set; }


		public SomeClass()
		{
			Field5 = new Point();

			Property5 = new SomeOtherClass();
		}


		public int DoSomething(bool arg1, int arg2, double arg3, string arg4)
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