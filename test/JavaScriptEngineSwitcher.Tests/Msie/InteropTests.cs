namespace JavaScriptEngineSwitcher.Tests.Msie
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "MsieJsEngine"; }
		}


		#region Embedding of objects

		#region Delegates

#if !NETCOREAPP
		public override void EmbeddedInstanceOfDelegateHasFunctionPrototype()
		{ }
#endif

		#endregion

		#region Recursive calls

		// TODO: Remove after fixing a error in the MSIE JavaScript Engine for .NET
		public override void RecursiveExecutionOfFilesIsCorrect()
		{ }

		// TODO: Remove after fixing a error in the MSIE JavaScript Engine for .NET
		public override void RecursiveEvaluationOfFilesIsCorrect()
		{ }

		#endregion

		#endregion
	}
}