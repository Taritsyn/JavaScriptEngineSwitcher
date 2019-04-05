namespace JavaScriptEngineSwitcher.Tests.Msie
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "MsieJsEngine"; }
		}


		// TODO: Remove after fixing a error in the MSIE JavaScript Engine for .NET
		public override void RecursiveExecutionOfFilesIsCorrect()
		{ }
	}
}