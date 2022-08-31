#if NET471 || NETCOREAPP3_1_OR_GREATER
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