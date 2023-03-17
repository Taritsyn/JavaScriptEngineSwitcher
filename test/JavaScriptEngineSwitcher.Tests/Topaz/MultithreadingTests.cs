#if NET6_0_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Topaz
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "TopazJsEngine"; }
		}
	}
}
#endif