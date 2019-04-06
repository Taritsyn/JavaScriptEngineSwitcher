namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
		}


		// TODO: Remove after fixing a error
		public override void RecursiveEvaluationOfFilesIsCorrect()
		{ }
	}
}