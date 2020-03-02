#if !NETCOREAPP1_0
using Xunit;

namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
		}
	}
}
#endif