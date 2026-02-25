#if NIL_JS
namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class EvalTests : EvalTestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}
	}
}
#endif