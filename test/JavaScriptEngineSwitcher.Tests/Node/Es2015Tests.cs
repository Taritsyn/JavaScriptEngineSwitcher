#if NET48 || NETCOREAPP3_1_OR_GREATER
namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class Es2015Tests : Es2015TestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}
	}
}
#endif