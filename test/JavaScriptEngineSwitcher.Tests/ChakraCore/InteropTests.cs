namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}
	}
}