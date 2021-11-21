using System;
using System.Drawing;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
#if NET461 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.V8;
#endif

using JavaScriptEngineSwitcher.Benchmarks.Interop.TypesEmbedding;

namespace JavaScriptEngineSwitcher.Benchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared)]
	public class HostTypesEmbeddingBenchmark
	{
		private static void EmbedAndUseHostTypes(Func<IJsEngine> createJsEngine)
		{
			// Arrange
			var someType = typeof(SomeClass);
			var pointType = typeof(Point);
			var someOtherType = typeof(SomeOtherClass);

			const string input = @"(function(SomeClass, Point, SomeOtherClass, undefined) {
	var arg1, arg2, arg3, arg4, interimResult, result;

	SomeClass.Field1 = false;
	SomeClass.Field2 = 678;
	SomeClass.Field3 = 2.20;
	SomeClass.Field4 = 'QWERTY';
	SomeClass.Field5 = new Point(2, 4);

	SomeClass.Property1 = true;
	SomeClass.Property2 = 711;
	SomeClass.Property3 = 5.5;
	SomeClass.Property4 = 'ЙЦУКЕН';
	SomeClass.Property5 = new SomeOtherClass(true, 611, 69.82, 'ASDF',
		false, 555, 79.99, 'ФЫВА');

	arg1 = SomeClass.Field1 || SomeClass.Property1;
	arg2 = SomeClass.Field2 + SomeClass.Property2 + SomeClass.Field5.X;
	arg3 = SomeClass.Field3 + SomeClass.Property3 + SomeClass.Field5.Y;
	arg4 = SomeClass.Field4 + SomeClass.Property4;

	interimResult = SomeClass.DoSomething(arg1, arg2, arg3, arg4);

	arg1 = SomeClass.Property5.Field1 && SomeClass.Property5.Property1;
	arg2 = interimResult - SomeClass.Property5.Field2 - SomeClass.Property5.Property2;
	arg3 = SomeClass.Property5.Field3 / SomeClass.Property5.Property3;
	arg4 = SomeClass.Property5.Field4 + SomeClass.Property5.Property4;

	result = SomeOtherClass.DoSomething(arg1, arg2, arg3, arg4);

	return result;
}(SomeClass, Point, SomeOtherClass));";
			const string targetOutput = "RmFsc2V8MjkyMHwwLjg3Mjg1OTEwNzM4ODQyNHxBU0RG0KTQq9CS0JA=";

			// Act
			string output;

			using (var jsEngine = createJsEngine())
			{
				jsEngine.EmbedHostType("SomeClass", someType);
				jsEngine.EmbedHostType("Point", pointType);
				jsEngine.EmbedHostType("SomeOtherClass", someOtherType);

				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Benchmark]
		public void ChakraCore()
		{
			Func<IJsEngine> createJsEngine = () => new ChakraCoreJsEngine();
			EmbedAndUseHostTypes(createJsEngine);
		}

		[Benchmark]
		public void Jint()
		{
			Func<IJsEngine> createJsEngine = () => new JintJsEngine();
			EmbedAndUseHostTypes(createJsEngine);
		}

		[Benchmark]
		public void Jurassic()
		{
			Func<IJsEngine> createJsEngine = () => new JurassicJsEngine();
			EmbedAndUseHostTypes(createJsEngine);
		}
#if NET461

		[Benchmark]
		public void MsieClassic()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic
			});
			EmbedAndUseHostTypes(createJsEngine);
		}

		[Benchmark]
		public void MsieChakraActiveScript()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraActiveScript
			});
			EmbedAndUseHostTypes(createJsEngine);
		}
#endif
		[Benchmark]
		public void MsieChakraIeJsRt()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraIeJsRt
			});
			EmbedAndUseHostTypes(createJsEngine);
		}

		[Benchmark]
		public void MsieChakraEdgeJsRt()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraEdgeJsRt
			});
			EmbedAndUseHostTypes(createJsEngine);
		}

		[Benchmark]
		public void NiL()
		{
			Func<IJsEngine> createJsEngine = () => new NiLJsEngine();
			EmbedAndUseHostTypes(createJsEngine);
		}
#if NET461 || NETCOREAPP3_1_OR_GREATER

		[Benchmark]
		public void V8()
		{
			Func<IJsEngine> createJsEngine = () => new V8JsEngine();
			EmbedAndUseHostTypes(createJsEngine);
		}
#endif
	}
}