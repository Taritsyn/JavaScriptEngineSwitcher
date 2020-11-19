#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1 || NET5_0
namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}
	}
}
#endif