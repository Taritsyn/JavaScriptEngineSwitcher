namespace JavaScriptEngineSwitcher.Tests.Msie
{
	using Core;

	public class InteropTests : InteropTestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("MsieJsEngine");

			return jsEngine;
		}
	}
}