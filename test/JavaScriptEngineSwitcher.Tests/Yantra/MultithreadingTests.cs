#if NET48 || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}
	}
}
#endif