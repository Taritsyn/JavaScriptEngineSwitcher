#if NET471 || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}
	}
}
#endif