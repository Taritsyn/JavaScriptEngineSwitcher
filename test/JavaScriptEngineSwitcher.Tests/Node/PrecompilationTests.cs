#if NET48 || NETCOREAPP3_1_OR_GREATER
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