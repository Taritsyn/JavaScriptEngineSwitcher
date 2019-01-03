using System;
using System.Globalization;
using System.Text;

namespace JavaScriptEngineSwitcher.Benchmarks.Interop.ObjectsEmbedding
{
	public class SomeOtherClass : SomeClassBase
	{
		public string DoSomething(bool arg1, int arg2, double arg3, string arg4)
		{
			string rawResult = arg1.ToString(CultureInfo.InvariantCulture) + "|" +
				arg2.ToString(CultureInfo.InvariantCulture) + "|" +
				arg3.ToString(CultureInfo.InvariantCulture) + "|" +
				arg4
				;
			string result = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawResult));

			return result;
		}
	}
}