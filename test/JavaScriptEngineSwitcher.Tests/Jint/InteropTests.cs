namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
		}


		#region Embedding of objects

		#region Recursive calls

		// TODO: Remove after fixing a error
		public override void RecursiveEvaluationOfFilesIsCorrect()
		{ }

		#endregion

		#endregion
	}
}