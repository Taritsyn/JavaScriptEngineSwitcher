namespace JavaScriptEngineSwitcher.Tests.Msie
{
	using Core;

	public class Es5Tests : Es5TestsBase
	{
		protected override IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("MsieJsEngine");

			return jsEngine;
		}
	}
}