using System;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
#if NET461 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.NiL;
#endif
#if NET6_0_OR_GREATER
using JavaScriptEngineSwitcher.Topaz;
#endif
#if NET461 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.V8;
#endif

using JavaScriptEngineSwitcher.Benchmarks.Interop.ObjectsEmbedding;

namespace JavaScriptEngineSwitcher.Benchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared)]
	public class HostObjectsEmbeddingBenchmark
	{
		private static void EmbedAndUseHostObjects(Func<IJsEngine> createJsEngine)
		{
			// Arrange
			var someObj = new SomeClass();
			var logBuilder = new StringBuilder();
			Action<string> log = (string value) =>
			{
				logBuilder.AppendLine(value);
			};

			const string input = @"(function(someObj, log, undefined) {
	var arg1, arg2, arg3, arg4, interimResult, result;

	log('-= Start code execution =-');

	someObj.Field1 = false;
	someObj.Field2 = 678;
	someObj.Field3 = 2.20;
	someObj.Field4 = 'QWERTY';
	someObj.Field5.X = 2;
	someObj.Field5.Y = 4;

	someObj.Property1 = true;
	someObj.Property2 = 711;
	someObj.Property3 = 5.5;
	someObj.Property4 = 'ЙЦУКЕН';
	someObj.Property5.Field1 = true;
	someObj.Property5.Field2 = 611;
	someObj.Property5.Field3 = 69.82;
	someObj.Property5.Field4 = 'ASDF';
	someObj.Property5.Property1 = false;
	someObj.Property5.Property2 = 555;
	someObj.Property5.Property3 = 79.99;
	someObj.Property5.Property4 = 'ФЫВА';

	arg1 = someObj.Field1 || someObj.Property1;
	arg2 = someObj.Field2 + someObj.Property2 + someObj.Field5.X;
	arg3 = someObj.Field3 + someObj.Property3 + someObj.Field5.Y;
	arg4 = someObj.Field4 + someObj.Property4;

	interimResult = someObj.DoSomething(arg1, arg2, arg3, arg4);

	arg1 = someObj.Property5.Field1 && someObj.Property5.Property1;
	arg2 = interimResult - someObj.Property5.Field2 - someObj.Property5.Property2;
	arg3 = someObj.Property5.Field3 / someObj.Property5.Property3;
	arg4 = someObj.Property5.Field4 + someObj.Property5.Property4;

	result = someObj.Property5.DoSomething(arg1, arg2, arg3, arg4);

	log('-= End of code execution =-');

	return result;
}(someObj, log));";
			const string targetOutput = "RmFsc2V8MjkxNHwwLjg3Mjg1OTEwNzM4ODQyNHxBU0RG0KTQq9CS0JA=";
			string targetLogOutput = "-= Start code execution =-" + Environment.NewLine +
				"-= End of code execution =-" + Environment.NewLine;

			// Act
			string output;
			string logOutput;

			using (var jsEngine = createJsEngine())
			{
				jsEngine.EmbedHostObject("someObj", someObj);
				jsEngine.EmbedHostObject("log", log);

				output = jsEngine.Evaluate<string>(input);

				logOutput = logBuilder.ToString();
				logBuilder.Clear();
			}

			// Assert
			Assert.Equal(targetOutput, output);
			Assert.Equal(targetLogOutput, logOutput);
		}

		[Benchmark]
		public void ChakraCore()
		{
			Func<IJsEngine> createJsEngine = () => new ChakraCoreJsEngine();
			EmbedAndUseHostObjects(createJsEngine);
		}

		[Benchmark]
		public void Jint()
		{
			Func<IJsEngine> createJsEngine = () => new JintJsEngine();
			EmbedAndUseHostObjects(createJsEngine);
		}

		[Benchmark]
		public void Jurassic()
		{
			Func<IJsEngine> createJsEngine = () => new JurassicJsEngine();
			EmbedAndUseHostObjects(createJsEngine);
		}
#if NET461

		[Benchmark]
		public void MsieClassic()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic
			});
			EmbedAndUseHostObjects(createJsEngine);
		}

		[Benchmark]
		public void MsieChakraActiveScript()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraActiveScript
			});
			EmbedAndUseHostObjects(createJsEngine);
		}
#endif
		[Benchmark]
		public void MsieChakraIeJsRt()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraIeJsRt
			});
			EmbedAndUseHostObjects(createJsEngine);
		}

		[Benchmark]
		public void MsieChakraEdgeJsRt()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraEdgeJsRt
			});
			EmbedAndUseHostObjects(createJsEngine);
		}
#if NET461 || NETCOREAPP3_1_OR_GREATER

		[Benchmark]
		public void NiL()
		{
			Func<IJsEngine> createJsEngine = () => new NiLJsEngine();
			EmbedAndUseHostObjects(createJsEngine);
		}
#endif
#if NET6_0_OR_GREATER

		[Benchmark]
		public void Topaz()
		{
			Func<IJsEngine> createJsEngine = () => new TopazJsEngine();
			EmbedAndUseHostObjects(createJsEngine);
		}
#endif
#if NET461 || NETCOREAPP3_1_OR_GREATER

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void V8(bool disableDynamicBinding)
		{
			Func<IJsEngine> createJsEngine = () => new V8JsEngine(
				new V8Settings { DisableDynamicBinding = disableDynamicBinding }
			);
			EmbedAndUseHostObjects(createJsEngine);
		}
#endif
	}
}