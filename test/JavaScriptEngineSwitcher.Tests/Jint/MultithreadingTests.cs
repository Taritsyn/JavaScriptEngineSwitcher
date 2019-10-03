#if !NETCOREAPP1_0
namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
		}
	}
}
#endif