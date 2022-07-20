using System;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
#if NET46
using JavaScriptEngineSwitcher.V8;
#endif

namespace TestBenchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared)]
	public class InteropBenchmark
	{
		public class Native
		{
			public SomeClass SomeObject { get; set; } = new SomeClass();
		}

		public class SomeClass
		{
			public string doSomething(double i) => "SomeValue@" + i;
		}


		private static void CallFunctionAndNestedProperties(Func<IJsEngine> createJsEngine)
		{
			using (var newEngine = createJsEngine())
			{

				newEngine.EmbedHostObject("native", new Native());
				newEngine.Execute(@"function testFunction(i) {
	return native.SomeObject.doSomething(i);
}");

				for (int i = 0; i < 1000/*00*/; i++)
				{
					var result = newEngine.CallFunction("testFunction", new object[] { (double)i });
					//if (i % 10000 == 0) Console.WriteLine($"Called test #{i}, result was {result}");
				}
			}
		}

		[Benchmark]
		public void ChakraCore()
		{
			Func<IJsEngine> createJsEngine = () => new ChakraCoreJsEngine();
			CallFunctionAndNestedProperties(createJsEngine);
		}
//#if NET46

//		[Benchmark]
//		public void V8()
//		{
//			Func<IJsEngine> createJsEngine = () => new V8JsEngine();
//			CallFunctionAndNestedProperties(createJsEngine);
//		}
//#endif
	}
}