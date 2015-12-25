namespace JavaScriptEngineSwitcher.Tests.V8
{
	using NUnit.Framework;

	using Core;

	[TestFixture]
	public class CommonTests : CommonTestsBase
	{
		[TestFixtureSetUp]
		public override void SetUp()
		{
			_jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("V8JsEngine");
		}
	}
}