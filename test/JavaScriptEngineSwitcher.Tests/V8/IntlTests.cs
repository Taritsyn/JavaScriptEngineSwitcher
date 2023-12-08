namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class IntlTests : IntlTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}
	}
}