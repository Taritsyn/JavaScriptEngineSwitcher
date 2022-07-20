using System;
using System.Runtime.InteropServices;
using System.Text;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;

namespace TestChakraCore
{
	class Program
    {
		static void Main(string[] args)
        {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			const string libraryCode = @"function declensionOfNumerals(number, titles) {
	if (number < 0) {
		throw new Error('123');
	}

	var result,
		titleIndex,
		cases = [2, 0, 1, 1, 1, 2],
		caseIndex
		;

	if (number % 100 > 4 && number % 100 < 20) {
		titleIndex = 2;
	}
	else {
		caseIndex = number % 10 < 5 ? number % 10 : 5;
		titleIndex = cases[caseIndex];
	}

	result = titles[titleIndex];

	return result;
}

function declinationOfSeconds(number) {
	return declensionOfNumerals(number, ['секунда', 'секунды', 'секунд']);
}";
			const string functionName = "declinationOfSeconds";
			const int itemCount = 4;

			int[] inputSeconds = new int[itemCount] { 0, 1, 42, 600 };
			string[] outputStrings = new string[itemCount];

			// Act
			IPrecompiledScript compiledCode = null;

			using (var jsEngine = new ChakraCoreJsEngine())
			{
				compiledCode = jsEngine.Precompile(libraryCode, "declinationOfSeconds.js");
				jsEngine.HasVariable("переменная");

				jsEngine.Execute(compiledCode);
				outputStrings[0] = jsEngine.CallFunction<string>(functionName, inputSeconds[0]);
			}

			for (int itemIndex = 1; itemIndex < itemCount; itemIndex++)
			{
				using (var firstJsEngine = new ChakraCoreJsEngine())
				{
					firstJsEngine.Execute(compiledCode);
					outputStrings[itemIndex] = firstJsEngine.CallFunction<string>(functionName, inputSeconds[itemIndex]);
				}
			}

			for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
			{
				Console.WriteLine("{0} {1}", inputSeconds[itemIndex], outputStrings[itemIndex]);
			}

			//			Console.WriteLine();
			//			Console.WriteLine("====================================================================");
			//			Console.WriteLine();

			//			using (var fourthJsEngine = new ChakraCoreJsEngine())
			//			{
			//				const string input4 = "Math.E + Math.PI";
			//				double output4 = (double)fourthJsEngine.ParseAndEvaluate(input4, "constants.js");

			//				Console.WriteLine("{0} = {1}", input4, output4);
			//			}

			//			Console.WriteLine();
			//			Console.WriteLine("====================================================================");
			//			Console.WriteLine();

			//			using (var fifthJsEngine = new ChakraCoreJsEngine())
			//			{
			//				const string input5 = @"var randomNumber = Math.random();
			//randomNumber = Math.log(randomNumber);
			//randomNumber = Math.sin(randomNumber);";
			//				double output5 = (double)fifthJsEngine.ParseAndEvaluate(input5, "random.js");

			//				Console.WriteLine("{0} = {1}", input5, output5);
			//			}

			//			Console.WriteLine();
			//			Console.WriteLine("====================================================================");
			//			Console.WriteLine();

			//			using (var sixthJsEngine = new ChakraCoreJsEngine())
			//			{
			//				const string functionCode = @"function guid() {
			//  function s4() {
			//    return Math.floor((1 + Math.random()) * 0x10000)
			//      .toString(16)
			//      .substring(1);
			//  }
			//  return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
			//}";
			//				const string input6 = @"guid()";

			//				sixthJsEngine.Execute(functionCode);
			//				string output6 = sixthJsEngine.Evaluate<string>(input6, "random.js");

			//				Console.WriteLine("{0} = {1}", input6, output6);
			//			}

			Console.WriteLine();
			Console.WriteLine("====================================================================");
			Console.WriteLine();

			// Arrange
			var uri = new Uri("https://github.com/Taritsyn/MsieJavaScriptEngine");

			const string input21 = "uri.Scheme";
			const string input22 = "uri.Host";
			const string input23 = "uri.PathAndQuery";

			// Act
			string output21;
			string output22;
			string output23;

			using (var jsEngine = new ChakraCoreJsEngine())
			{
				jsEngine.EmbedHostObject("uri", uri);

				output21 = jsEngine.Evaluate<string>(input21);
				output22 = jsEngine.Evaluate<string>(input22);
				output23 = jsEngine.Evaluate<string>(input23);
			}

			// Assert
			Console.WriteLine("{0} = {1}", input21, output21);
			Console.WriteLine("{0} = {1}", input22, output22);
			Console.WriteLine("{0} = {1}", input23, output23);

			Console.WriteLine();
			Console.WriteLine("====================================================================");
			Console.WriteLine();
		}
    }
}