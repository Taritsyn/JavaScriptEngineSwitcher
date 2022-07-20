using System;

using JavaScriptEngineSwitcher.V8;

namespace TestNetCore31
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var engine = new V8JsEngine())
			{
				int result = engine.Evaluate<int>("1 + 1");
				Console.WriteLine(result);
			}
		}
	}
}
