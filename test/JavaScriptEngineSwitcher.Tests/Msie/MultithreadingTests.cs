namespace JavaScriptEngineSwitcher.Tests.Msie
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "MsieJsEngine"; }
		}
	}
}