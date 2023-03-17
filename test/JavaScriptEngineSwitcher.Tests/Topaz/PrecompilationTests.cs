#if NET6_0_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Topaz
{
	public class PrecompilationTests : PrecompilationTestsBase
	{
		protected override string EngineName
		{
			get { return "TopazJsEngine"; }
		}
	}
}
#endif