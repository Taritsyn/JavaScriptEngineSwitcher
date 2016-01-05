namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	using Core;

	public class InteropTests : InteropTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("JurassicJsEngine");

			return jsEngine;
		}
	}
}