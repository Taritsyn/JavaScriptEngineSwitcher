#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1 || NET5_0
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