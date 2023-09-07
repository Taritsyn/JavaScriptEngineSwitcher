#if NET471 || NETCOREAPP3_1_OR_GREATER
using Xunit;

namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}
	}
}
#endif