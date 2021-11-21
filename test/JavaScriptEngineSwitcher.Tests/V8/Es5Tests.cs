#if NETFRAMEWORK || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}
	}
}
#endif