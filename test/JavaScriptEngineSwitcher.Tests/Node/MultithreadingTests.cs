#if !NET452
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