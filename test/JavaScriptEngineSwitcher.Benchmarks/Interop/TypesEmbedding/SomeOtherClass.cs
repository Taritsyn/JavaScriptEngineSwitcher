using System;
using System.Globalization;
using System.Text;

namespace JavaScriptEngineSwitcher.Benchmarks.Interop.TypesEmbedding
{
	public class SomeOtherClass
	{
		public bool Field1;
		public int Field2;
		public double Field3;
		public string Field4;

		public bool Property1 { get; set; }
		public int Property2 { get; set; }
		public double Property3 { get; set; }
		public string Property4 { get; set; }


		public SomeOtherClass(bool field1, int field2, double field3, string field4,
			bool property1, int property2, double property3, string property4)
		{
			Field1 = field1;
			Field2 = field2;
			Field3 = field3;
			Field4 = field4;

			Property1 = property1;
			Property2 = property2;
			Property3 = property3;
			Property4 = property4;
		}


		public static string DoSomething(bool arg1, int arg2, double arg3, string arg4)
		{
			string rawResult = arg1.ToString(CultureInfo.InvariantCulture) + "|" +
				arg2.ToString(CultureInfo.InvariantCulture) + "|" +
				Math.Round(arg3, 15).ToString(CultureInfo.InvariantCulture) + "|" +
				arg4
				;
			string result = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawResult));

			return result;
		}
	}
}