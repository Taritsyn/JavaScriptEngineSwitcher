namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	using Core;

	public class Es5Tests : Es5TestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("ChakraCoreJsEngine");

			return jsEngine;
		}
	}
}