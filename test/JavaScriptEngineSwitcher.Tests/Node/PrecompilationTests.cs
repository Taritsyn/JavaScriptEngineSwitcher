#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1
namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class PrecompilationTests : PrecompilationTestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}
	}
}
#endif