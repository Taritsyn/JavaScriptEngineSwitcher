namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}
	}
}