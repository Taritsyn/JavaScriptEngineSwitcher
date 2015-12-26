namespace JavaScriptEngineSwitcher.Tests.Jint
{
	using Core;

	public class CommonTests : CommonTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("JintJsEngine");

			return jsEngine;
		}
	}
}