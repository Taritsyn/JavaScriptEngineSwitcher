namespace JavaScriptEngineSwitcher.Tests.Vroom
{
	public class MultithreadingTests : MultithreadingTestsBase
	{
		protected override string EngineName
		{
			get { return "VroomJsEngine"; }
		}


		public override void RecursiveExecutionOfFilesIsCorrect()
		{ }

		public override void RecursiveEvaluationOfFilesIsCorrect()
		{ }
	}
}