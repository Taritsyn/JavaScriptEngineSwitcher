#if !NETCOREAPP1_0
namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "JurassicJsEngine"; }
		}
	}
}
#endif