namespace JavaScriptEngineSwitcher.Tests.Jint
{
	using Core;

	public class InteropTests : InteropTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("JintJsEngine");

			return jsEngine;
		}
	}
}