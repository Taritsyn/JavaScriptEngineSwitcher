#if NET48 || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class EvalTests : EvalTestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}
	}
}
#endif