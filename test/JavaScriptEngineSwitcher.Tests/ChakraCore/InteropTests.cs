namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	using Core;

	public class InteropTests : InteropTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("ChakraCoreJsEngine");

			return jsEngine;
		}
	}
}