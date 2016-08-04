namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}
	}
}