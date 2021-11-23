#if !NET452
namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class PrecompilationTests : PrecompilationTestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}
	}
}
#endif