namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class Es2015Tests : Es2015TestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}
	}
}