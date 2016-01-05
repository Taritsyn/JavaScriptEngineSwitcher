namespace JavaScriptEngineSwitcher.Tests.V8
{
	using Core;

	public class InteropTests : InteropTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("V8JsEngine");

			return jsEngine;
		}
	}
}