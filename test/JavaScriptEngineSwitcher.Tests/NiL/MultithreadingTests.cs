#if !NET452
namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}
	}
}
#endif