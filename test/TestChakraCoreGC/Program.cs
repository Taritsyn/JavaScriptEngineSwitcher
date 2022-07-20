using System;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;

namespace TestChakraCoreGC
{
	class Program
	{
		public class Native
		{
			public SomeStruct SomeObject { get; set; } = new SomeStruct();
		}

		public struct SomeStruct
		{
			public string doSomething(double i) => "SomeValue@" + i;
		}

		static void Main(string[] args)
		{
			using (var newEngine = new ChakraCoreJsEngine())
			{
				newEngine.EmbedHostObject("native", new Native());
				newEngine.EmbedHostObject("log", new Action<string>(Console.WriteLine));
				//newEngine.EmbedHostType("nativeType", typeof(Native));
				newEngine.Execute(@"//var native = new nativeType();
//log(Object.getOwnPropertyNames(log).join(', '));
var count = 0;
var propertyNames = '';

for (var propertyName in log) {
	propertyNames += propertyName + ',';
	count++;
}

log(propertyNames + ' -=- ' + count);

function testFunction(i) {
	log('Ура!');
	return native.SomeObject.doSomething(i);
}");

				for (int i = 0; i < 5/*1000*/; i++)
				{
					var result = newEngine.CallFunction("testFunction", new object[] { (double)i });
					Console.WriteLine($"Called test #{i}, result was {result}");

					newEngine.CollectGarbage();
				}

				Console.WriteLine();
			}
		}
	}
}