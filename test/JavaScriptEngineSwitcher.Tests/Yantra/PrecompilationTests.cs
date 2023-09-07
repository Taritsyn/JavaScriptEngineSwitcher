#if NET471 || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class PrecompilationTests : PrecompilationTestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}
	}
}
#endif