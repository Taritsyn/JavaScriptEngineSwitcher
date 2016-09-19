namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}
	}
}