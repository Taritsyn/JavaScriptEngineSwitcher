#if NET471 || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class EvalTests : EvalTestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}
	}
}
#endif