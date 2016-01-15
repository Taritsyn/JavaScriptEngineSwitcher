namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	using Core;

	public class CommonTests : CommonTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("ChakraCoreJsEngine");

			return jsEngine;
		}
	}
}