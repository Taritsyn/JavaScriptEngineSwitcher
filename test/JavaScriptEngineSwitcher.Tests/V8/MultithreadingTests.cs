#if NETFULL || NETCOREAPP3_1 || NET5_0
namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}
	}
}
#endif