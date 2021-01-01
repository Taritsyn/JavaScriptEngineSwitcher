#if !NET452
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