namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class EvalTests : EvalTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}
	}
}