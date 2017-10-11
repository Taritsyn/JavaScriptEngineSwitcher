#if !NETCOREAPP1_0 && !NETCOREAPP2_0
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
#endif