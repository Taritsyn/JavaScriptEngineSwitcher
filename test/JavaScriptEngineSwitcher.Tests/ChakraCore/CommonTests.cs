namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}
	}
}