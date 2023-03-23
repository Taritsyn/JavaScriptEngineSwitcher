#if NETFRAMEWORK || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class EvalTests : EvalTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}
	}
}
#endif