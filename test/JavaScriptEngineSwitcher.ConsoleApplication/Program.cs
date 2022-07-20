using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Jering.Javascript.NodeJS;

using OriginalStepMode = Jint.Runtime.Debugger.StepMode;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
using JavaScriptEngineSwitcher.Node;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Vroom;

namespace JavaScriptEngineSwitcher.ConsoleApplication
{
    public class Program
    {
		static Program()
		{
			StaticNodeJSService.Configure<OutOfProcessNodeJSServiceOptions>(
				options => options.Concurrency = Concurrency.MultiProcess);

			JsEngineSwitcher.Current.EngineFactories
				.AddChakraCore()
				.AddJint()
				.AddJurassic()
				.AddMsie(options => options.EngineMode = JsEngineMode.ChakraIeJsRt)
				.AddNiL()
				.AddNode()
				.AddV8()
				.AddVroom()
				;
			JsEngineSwitcher.Current.DefaultEngineName = ChakraCoreJsEngine.EngineName;
		}

		public static void Main(string[] args)
		{
			//TestEngines();
			//TestPerformance();

			CustomTest();
		}

		private static void TestEngines()
		{
			TestEngine(ChakraCoreJsEngine.EngineName);
			TestEngine(JintJsEngine.EngineName);
			TestEngine(JurassicJsEngine.EngineName);
			TestEngine(MsieJsEngine.EngineName);
			TestEngine(NiLJsEngine.EngineName);
			TestEngine(V8JsEngine.EngineName);
			TestEngine(VroomJsEngine.EngineName);
		}

		private static void TestEngine(string engineName)
	    {
//			const string code = @"var str = '';

//loop1:
//for (var i = 0; i < 5; i++) {
//  if (i === 1) {
//    continue loop1;
//  }
//  str = str + i;
//}";

			//			const string code = @"'use strict';

			//a = 1;";
			//string code = "'use strict'; " + System.IO.File.ReadAllText(@"C:\Projects\BundleTransformer\src\BundleTransformer.Packer\Resources\packer-combined.js");
			//			const string code = @"'use strict';

			//var mathFunctions = {
			//	cube: function (num) {
			//		var result;

			//		if (num !== 0) {
			//			function square(n) {
			//				return n * n;
			//			}

			//			result = square(num) * num;
			//		}
			//		else {
			//			result = 0;
			//		}

			//		return result;
			//	}
			//};";

			//			const string code = @"var obj = Object.setPrototypeOf || ('__proto__'in{}?function(t, r, i) {
			//	try {
			//		i = e(31)(Function.call, n(Object.prototype, '__proto__').set, 2), i(t, []), r = !(t instanceof Array)
			//	} catch (s) {
			//		r = !0
			//	}
			//	return function(e, t) {
			//		return a(e, t), r ? e.__proto__ = t : i(e, t), e
			//	}
			//}({}, !1) : void 0)";

			//const string code = "function base64_encode(a){var b=\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=\";var c,o2,o3,h1,h2,h3,h4,bits,i=0,enc='';do{c=a.charCodeAt(i++);o2=a.charCodeAt(i++);o3=a.charCodeAt(i++);bits=c<<16|o2<<8|o3;h1=bits>>18&0x3f;h2=bits>>12&0x3f;h3=bits>>6&0x3f;h4=bits&0x3f;enc+=b.charAt(h1)+b.charAt(h2)+b.charAt(h3)+b.charAt(h4)}while(i<a.length);switch(a.length%3){case 1:enc=enc.slice(0,-2)+'==';break;case 2:enc=enc.slice(0,-1)+'=';break}return enc}@";
			//Console.WriteLine("|" + JsErrorHelpers.GetSourceFragmentFromLine(code, 145) + "|");

			//const string code = @"var a = 1;
			//var b = 2;
			//var c = 3;
			//c @= 1;
			//c = b + a;
			//c -= 0.5;";

			//const string code = @"var arr = [];

			////for (var i = 0; i < 10000; i++) {
			////	arr.push('Current date: ' + new Date());
			////}";

			//const string code = @"var MyLib = {};
			//MyLib.ns1 = {};
			//MyLib.ns2 = {};

			//MyLib.ns1.CustomError = function(message) {
			//  this.name = 'MyLib.ns1.CustomError';
			//  this.message = message;

			//			if (Error.captureStackTrace)
			//			{
			//				Error.captureStackTrace(this, this.constructor);
			//			}
			//			else
			//			{
			//				this.stack = (new Error()).stack;
			//			}

			//		}

			//MyLib.ns1.CustomError.prototype = Object.create(Error.prototype);
			//MyLib.ns1.CustomError.prototype.constructor = MyLib.ns1.CustomError;

			//MyLib.ns2.MyClass = function(){
			//};

			//MyLib.ns2.MyClass.prototype.invoke = function () {
			//	throw /*new MyLib.ns1.CustomError(*/'123'/*)*/;
			//};

			//var obj = new MyLib.ns2.MyClass();
			//obj.invoke();
			//";

			//const string code = @"function fibonacci(n) {
			//	if (n === 1) {
			//		return 1;
			//	}
			//	else if (n === 2) {
			//		return 1;
			//	}
			//	else {
			//		return fibonacci(n - 1)/* + fibonacci(n - 2)*/;
			//	}
			//}

			//(function (fibonacci) {
			//	var a = 5;
			//	var b = 11;
			//	var c = fibonacci(b) - fibonacci(a);
			//})(fibonacci);";

			//			const string code = @"f();
			//function e() {
			//	return true;
			//}

			//function f(){
			//	var b = 12+15;
			//	if (b > 20) {
			//		n();
			//	}
			//}

			//var a = null;
			//var b = 2;
			//var c = b/a;
			//f();
			////throw new Error('Ура!!!'); //c(12,3);";

			//			const string code = @"function declensionOfNumerals(number, titles) {
			//	var result,
			//		titleIndex,
			//		cases = [2, 0, 1, 1, 1, 2],
			//		caseIndex
			//		;

			//	if (number % 100 > 4 && number % 100 < 20) {
			//		titleIndex = 2;
			//	}
			//	else {
			//		caseIndex = number % 1O < 5 ? number % 10 : 5;
			//		titleIndex = cases[caseIndex];
			//	}

			//	result = titles[titleIndex];

			//	return result;
			//}

			//function declinationOfSeconds(number) {
			//	return declensionOfNumerals(number, ['секунда', 'секунды', 'секунд']);
			//}";

			//const string code = @"function f()
			//{
			//	f();
			//};
			//f();";

			//const string code = @"while (true);";

			//const string code = @"function f()
			//{
			//	f();
			//};

			//function c()
			//{
			//	f();
			//};

			//function b()
			//{
			//	c();
			//};";

			//const string code = "var a = 1, b = true, c = '', f = {}; if (a > 0 && b === true && c && f) {  }";

			//			const string code = @"var successfulWork = new Promise(function(resolve, reject) {
			//	resolve(""Success!"");
			//});
			//
			//var unsuccessfulWork = new Promise(function (resolve, reject) {
			//	reject(""Fail!"");
			//});
			//
			//function resolveCallback(result) {
			//	console.WriteLine('Resolved: ' + result);
			//}
			//
			//function rejectCallback(reason) {
			//	console.WriteLine('Rejected: ' + reason);
			//}
			//
			//successfulWork.then(resolveCallback, rejectCallback);
			//unsuccessfulWork.then(resolveCallback, rejectCallback);";

			Console.WriteLine("------------------------------------------------------------");
			Console.WriteLine(engineName);
			Console.WriteLine("------------------------------------------------------------");

			using (IJsEngine engine = JsEngineSwitcher.Current.CreateEngine(engineName))
			{
				try
				{
					//engine.EmbedHostType("console", typeof(Console));
					//engine.ExecuteFile("C:/temp/doc01.js");
					//engine.ExecuteResource("JavaScriptEngineSwitcher.ConsoleApplication.doc01.js", typeof(Program).Assembly);
					//engine.ExecuteResource("doc01.js", typeof(Program));
					//engine.Execute(code, "declinationOfSeconds.js");
					//engine.CallFunction("b");
					//engine.Evaluate("e();");
					//engine.Evaluate("f();");
					//engine.Evaluate("123@45", "C:/temp/doc01.js");
					//engine.CallFunction("f");
					//Console.WriteLine(code);

					//engine.ExecuteFile(@"C:\Projects\JavaScriptEngineSwitcher\test\JavaScriptEngineSwitcher.ConsoleApplication\russian-translit.js");
					//string result = engine.CallFunction<string>("transliterate", "Что нужно знать о JavaScript Engine Switcher 3.0",
					//	"yandex-maps");
					//string result = engine.Evaluate<string>("transliterate('Что нужно знать о JavaScript Engine Switcher 3.0', 'yandex-maps');");

					//engine.Execute(code);

					//var someFunc = new Func<int>(() => 42);
					//engine.EmbedHostObject("embeddedFunc", someFunc);

					var animal = new Duck();

					engine.EmbedHostObject("animal", animal);
					string result = engine.Evaluate<string>(@"animal.Cry();", "new.js");

					Console.WriteLine(result);
				}
				catch (JsException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(JsErrorHelpers.GenerateErrorDetails(e));
					Console.WriteLine();
				}
				catch (COMException e)
				{
					Console.WriteLine(e.ToString());
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			}

			//const string sleepyСode = @"function sleep(millisecondsTimeout) {
			//	var totalMilliseconds = new Date().getTime() + millisecondsTimeout;

			//	while (new Date() < totalMilliseconds)
			//	{ }
			//}

			//waitHandle.Set();
			//sleep(5000);";

			//const string input = "!0";
			//const bool targetOutput = true;

			//// Act
			//bool supportsScriptInterruption;
			//Exception currentException = null;
			//bool output;

			//using (var jsEngine = JsEngineSwitcher.Current.CreateEngine(engineName))
			//{
			//	supportsScriptInterruption = jsEngine.SupportsScriptInterruption;
			//	if (supportsScriptInterruption)
			//	{
			//		using (var waitHandle = new ManualResetEvent(false))
			//		{
			//			ThreadPool.QueueUserWorkItem(state =>
			//			{
			//				waitHandle.WaitOne();
			//				jsEngine.Interrupt();
			//			});

			//			jsEngine.EmbedHostObject("waitHandle", waitHandle);

			//			try
			//			{
			//				jsEngine.Execute(sleepyСode);
			//			}
			//			catch (Exception e)
			//			{
			//				currentException = e;
			//			}
			//		}
			//	}

			//	output = jsEngine.Evaluate<bool>(input);
			//}

			//			Console.WriteLine("------------------------------------------------------------");
			//			Console.WriteLine(engineName);
			//			Console.WriteLine("------------------------------------------------------------");

			//			using (IJsEngine engine = JsEngineSwitcher.Current.CreateEngine(engineName))
			//			{
			//				try
			//				{
			//					engine.ExecuteFile("C:/temp/packer-combined.min.js");
			//					engine.Evaluate(@"var packer = new Packer();
			//packer.pack('123', true, true);");
			//					//engine.Evaluate("e();");
			//					//engine.Evaluate("f();");
			//					//engine.Evaluate("123@45", "C:/temp/doc01.js");
			//					//engine.CallFunction("f");
			//					//Console.WriteLine(code);
			//				}
			//				catch (JsRuntimeException e)
			//				{
			//					Console.WriteLine("|" + e.Message + "|");
			//					//Console.WriteLine(JsErrorHelpers.Format(e));
			//					Console.WriteLine();
			//				}
			//			}

			//Console.ReadLine();
		}

		private static void TestDebug(string engineName)
		{
			using (IJsEngine engine = JsEngineSwitcher.Current.CreateEngine(engineName))
			{
				try
				{
					engine.ExecuteFile(@"C:\Projects\JavaScriptEngineSwitcher\test\JavaScriptEngineSwitcher.ConsoleApplication\factorial.js");
					//engine.ExecuteResource("factorial.js", typeof(Program));
					engine.Evaluate<int>(@"factorial(3);");
					engine.Evaluate<int>(@"factorial(4);");
					int result = engine.Evaluate<int>(@"factorial(5);");
					Console.WriteLine("result = {0}", result);
				}
				catch (JsRuntimeException e)
				{
					Console.WriteLine(JsErrorHelpers.GenerateErrorDetails(e));
					Console.WriteLine();
				}

				Console.ReadLine();
			}
		}

		private static void TestPerformance()
		{
			TestPerformance(ChakraCoreJsEngine.EngineName);
			TestPerformance(JintJsEngine.EngineName);
			TestPerformance(JurassicJsEngine.EngineName);
			TestPerformance(MsieJsEngine.EngineName);
			//TestPerformance(NiLJsEngine.EngineName);
			//TestPerformance(V8JsEngine.EngineName);
			//TestPerformance(VroomJsEngine.EngineName);
		}

		private static void TestPerformance(string engineName)
		{
			Console.WriteLine("------------------------------------------------------------");
			Console.WriteLine(engineName);
			Console.WriteLine("------------------------------------------------------------");

			const int sleepTime = 1000;
			const int numberOfIterations = 50;

			using (var warmJsEngine = JsEngineSwitcher.Current.CreateEngine(engineName))
			{
				warmJsEngine.Execute("1 + 1;");
			}

			GC.Collect();
			Thread.Sleep(sleepTime);

			const string resourceName = "JavaScriptEngineSwitcher.ConsoleApplication.Resources.declinationOfSeconds.js";
			Assembly assembly = typeof(Program).Assembly;
			const string functionName = "declinationOfSeconds";

			var withoutPrecompilationTimeBefore = DateTime.Now;
			var withoutPrecompilationMemoryBefore = GC.GetTotalMemory(false) / 1024;

			for (int i = 0; i < numberOfIterations; i++)
			{
				using (var innerJsEngine = JsEngineSwitcher.Current.CreateEngine(engineName))
				{
					innerJsEngine.ExecuteResource(resourceName, assembly);
					innerJsEngine.CallFunction<string>(functionName, i);
				}
			}

			var withoutPrecompilationTimeAfter = DateTime.Now;
			var withoutPrecompilationMemoryAfter = GC.GetTotalMemory(false) / 1024;

			Console.WriteLine("Without pre-compilation");
			Console.WriteLine("time: {0} ms", (withoutPrecompilationTimeAfter - withoutPrecompilationTimeBefore).TotalMilliseconds);
			Console.WriteLine("memory: {0} kb", (withoutPrecompilationMemoryAfter - withoutPrecompilationMemoryBefore));
			Console.WriteLine();

			GC.Collect();
			Thread.Sleep(sleepTime);

			var withPrecompilationTimeBefore = DateTime.Now;
			var withPrecompilationMemoryBefore = GC.GetTotalMemory(false) / 1024;

			bool supportsScriptCompilation = false;
			IPrecompiledScript compiledCode = null;

			using (var jsEngine = JsEngineSwitcher.Current.CreateEngine(engineName))
			{
				supportsScriptCompilation = jsEngine.SupportsScriptPrecompilation;
				if (supportsScriptCompilation)
				{
					compiledCode = jsEngine.PrecompileResource(resourceName, assembly);
				}
			}

			if (supportsScriptCompilation)
			{
				for (int i = 0; i < numberOfIterations; i++)
				{
					using (var innerJsEngine = JsEngineSwitcher.Current.CreateEngine(engineName))
					{
						innerJsEngine.Execute(compiledCode);
						innerJsEngine.CallFunction<string>(functionName, i);
					}
				}

				var withPrecompilationTimeAfter = DateTime.Now;
				var withPrecompilationMemoryAfter = GC.GetTotalMemory(false) / 1024;

				Console.WriteLine("With pre-compilation");
				Console.WriteLine("time: {0} ms", (withPrecompilationTimeAfter - withPrecompilationTimeBefore).TotalMilliseconds);
				Console.WriteLine("memory: {0} kb", (withPrecompilationMemoryAfter - withPrecompilationMemoryBefore));
				Console.WriteLine();
			}
			else
			{
				Console.WriteLine("Not supported.");
			}

			GC.Collect();
			Thread.Sleep(sleepTime);
		}

		private static void CustomTest()
		{
			// Arrange
			const string input = @"function foo(x, y) {
	var z = x + y;
	if (z > 20) {
		bar();
	}
}

(function (foo) {
	var a = 8;
	var b = 15;

	foo(a, b);
})(foo);";
			string targetOutput = "ReferenceError: bar is not defined" + Environment.NewLine +
				"   at foo (functions.js:4:3) -> 		bar();" + Environment.NewLine +
				"   at functions.js:12:2" + Environment.NewLine +
				"   at functions.js:13:3"
				;

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = JsEngineSwitcher.Current.CreateEngine("NodeJsEngine"))
			{
				try
				{
					jsEngine.Execute(input, "functions.js");
				}
				catch (JsRuntimeException e)
				{
					Console.WriteLine(e.Message);
				}
			}

			//// Arrange
			//const string input = @"
			//             function test()
			//             {
			//                 const localConst = 'test';
			//                 debugger;
			//             }
			//             test();";
			//var settings = new JintSettings
			//{
			//	EnableDebugging = true,
			//	DebuggerStatementHandlingMode = JsDebuggerStatementHandlingMode.Clr,
			//	DebuggerBreakCallback = (sender, info) =>
			//	{
			//		return OriginalStepMode.None;
			//	},
			//	DebuggerStepCallback = (sender, info) =>
			//	{
			//		return OriginalStepMode.Into;
			//	}
			//};

			//// Act
			//using (var jsEngine = new JintJsEngine(settings))
			//{
			//	try
			//	{
			//		jsEngine.Execute(input);
			//	}
			//	catch (JsRuntimeException e)
			//	{
			//		Console.WriteLine(e);
			//	}
			//}

			//Console.ReadLine();


			//// Arrange
			//const string input = @"function factorial(value) {
			//	if (value <= 0) {
			//		throw new Error(""The value must be greater than or equal to zero."");
			//	}

			//	return value !== 1 ? value * factorial(value - 1) : 1;
			//}

			//factorial(5);
			//factorial(-1);
			//factorial(0);";

			//// Act
			//using (var jsEngine = new JintJsEngine())
			//{
			//	try
			//	{
			//		jsEngine.Execute(input, "factorial.js");
			//	}
			//	catch (JsRuntimeException e)
			//	{
			//		Console.WriteLine(e);
			//	}
			//}

			//Console.ReadLine();


			//			// Arrange
			//			const string input = @"function factorial(value) {
			//	if (value <= 0) {
			//		throw new Error(""The value must be greater than or equal to zero."");
			//	}

			//	return value !== 1 ? value * factorial(value - 1) : 1;
			//}

			//factorial(5);
			//factorial(-1);
			//factorial(0);";

			//			JsRuntimeException exception = null;

			//			// Act
			//			using (var jsEngine = new JintJsEngine())
			//			{
			//				try
			//				{
			//					jsEngine.Execute(input, "factorial.js");
			//				}
			//				catch (JsRuntimeException e)
			//				{
			//					exception = e;
			//				}
			//			}

			//			Console.ReadLine();

			//			const string input = @"var $variable1 = 611;
			//var _variable2 = 711;
			//var variable3 = 678;

			//$variable1 + -variable2 - variable3;";

			//			JsRuntimeException exception = null;

			//			// Act
			//			using (var jsEngine = new ChakraCoreJsEngine())
			//			{
			//				try
			//				{
			//					int result = jsEngine.Evaluate<int>(input, "variables.js");
			//					Console.WriteLine("result = {0}", result);
			//				}
			//				catch (JsRuntimeException e)
			//				{
			//					Console.WriteLine(input.Length);
			//					Console.WriteLine("ERROR");
			//					Console.WriteLine();
			//					Console.WriteLine(e.SourceFragment.Length);
			//					Console.WriteLine("|" + e.SourceFragment + "|");
			//				}
			//			}

			//double output;

			//using (var engine = new NodeJsEngine())
			//{
			//	output = engine.Evaluate<double>("5 * 3 + 7/9");
			//}

			//Console.WriteLine(output);
			//Console.ReadLine();

			//// Arrange
			//const string directoryPath = "Files/recursive-evaluation/runtime-error";
			//const string input = "evaluateFile('index').calculateResult();";

			//// Act
			//JsRuntimeException exception = null;

			//using (var jsEngine = new JintJsEngine())
			//{
			//	try
			//	{
			//		Func<string, object> evaluateFile = path => {
			//			string absolutePath = Path.Combine(directoryPath, $"{path}.js");
			//			string code = File.ReadAllText(absolutePath);
			//			object result = jsEngine.Evaluate(code, absolutePath);

			//			return result;
			//		};

			//		jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
			//		double output = jsEngine.Evaluate<double>(input);
			//	}
			//	catch (JsRuntimeException e)
			//	{
			//		exception = e;
			//	}
			//}


			//			// Arrange
			//			const string input = @"function sum(a, b) {
			//	return a + b + c;
			//}";

			//			//JsTimeoutException exception = null;

			//			// Act
			//			using (var jsEngine = new NodeJsEngine(
			//				new NodeSettings
			//				{
			//					//TimeoutInterval = TimeSpan.FromMilliseconds(30)
			//				}
			//			))
			//			{
			//				try
			//				{
			//					jsEngine.Execute(input, "sum.js");
			//					int result = jsEngine.CallFunction<int>("sum", 1, 2);

			//					Console.WriteLine("result = {0}", result);
			//				}
			//				catch (JsRuntimeException e)
			//				{
			//					Console.WriteLine(e.ToString());
			//				}
			//			}

			//			Console.ReadLine();

			//// Arrange
			//const string input = @"while (true);";

			////JsTimeoutException exception = null;

			//// Act
			//using (var jsEngine = new NodeJsEngine(
			//	new NodeSettings
			//	{
			//		TimeoutInterval = TimeSpan.FromMilliseconds(30)
			//	}
			//))
			//{
			//	try
			//	{
			//		jsEngine.Execute(input, "infinite-loop.js");
			//	}
			//	catch (JsTimeoutException e)
			//	{
			//		Console.WriteLine(e.ToString());
			//	}
			//}

			//Console.ReadLine();

			//			// Arrange
			//			const string input = @"function factorial(value) {
			//	if (value <= 0) {
			//		throw new Error(""The value must be greater than or equal to zero."");
			//	}

			//	return value !== 1 ? value * factorial(value - 1) : 1;
			//}

			//factorial(5);
			//factorial(-1);
			//factorial(0);";

			//			JsRuntimeException exception = null;

			//			// Act
			//			using (var jsEngine = JsEngineSwitcher.Current.CreateEngine(NodeJsEngine.EngineName))
			//			{
			//				try
			//				{
			//					jsEngine.Execute(input, "factorial.js");
			//				}
			//				catch (JsRuntimeException e)
			//				{
			//					Console.WriteLine(e.ToString());
			//				}
			//			}

			//			// Arrange
			//			const string input = @"var $variable1 = 611;
			//var _variable2 = 711;
			//var @variable3 = 678;

			//$variable1 + _variable2 - @variable3;";

			//			JsCompilationException exception = null;

			//			// Act
			//			using (var jsEngine = JsEngineSwitcher.Current.CreateEngine(NodeJsEngine.EngineName))
			//			{
			//				try
			//				{
			//					int result = jsEngine.Evaluate<int>(input, "variables.js");
			//				}
			//				catch (JsCompilationException e)
			//				{
			//					exception = e;
			//				}
			//			}

			//var engine1 = JsEngineSwitcher.Current.CreateEngine(NodeJsEngine.EngineName);
			//var engine2 = JsEngineSwitcher.Current.CreateEngine(NodeJsEngine.EngineName);

			//engine1.SetVariableValue("a", 2);
			//engine1.SetVariableValue("b", 5);

			//engine2.SetVariableValue("a", 21);
			//engine2.SetVariableValue("b", 52);

			//Console.WriteLine(engine1.GetVariableValue("a"));
			//Console.WriteLine(engine1.GetVariableValue("b"));

			//Console.WriteLine(engine2.GetVariableValue("a"));
			//Console.WriteLine(engine2.GetVariableValue("b"));

			//Console.WriteLine(engine1.Evaluate<string>(@"require('path').basename('C:\\temp\\myfile.html');"));

			//engine1.Dispose();
			//engine2.Dispose();
		}
	}
}