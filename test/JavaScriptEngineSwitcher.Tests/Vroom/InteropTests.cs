namespace JavaScriptEngineSwitcher.Tests.Vroom
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "VroomJsEngine"; }
		}


		#region Embedding of objects

		#region Objects with fields

		public override void EmbeddingOfInstanceOfCustomValueTypeWithFieldsIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithFieldsIsCorrect()
		{ }

		#endregion

		#region Objects with methods

		public override void EmbeddingOfInstanceOfBuiltinReferenceTypeWithMethodIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithMethodIsCorrect()
		{ }

		#endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateWithoutParametersIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithOneParameterIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithTwoParametersIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithoutResultIsCorrect()
		{ }

		public override void EmbeddedInstanceOfDelegateHasFunctionPrototype()
		{ }

		#endregion

		#region Recursive calls

		public override void RecursiveExecutionOfFilesIsCorrect()
		{ }

		public override void RecursiveEvaluationOfFilesIsCorrect()
		{ }

		#endregion

		#endregion


		#region Embedding of types

		#region Types with fields

		public override void EmbeddingOfBuiltinValueTypeWithFieldIsCorrect()
		{ }

		public override void EmbeddingOfCustomReferenceTypeWithFieldIsCorrect()
		{ }

		#endregion

		#region Types with methods

		public override void EmbeddingOfBuiltinReferenceTypeWithMethodsIsCorrect()
		{ }

		#endregion

		#endregion
	}
}