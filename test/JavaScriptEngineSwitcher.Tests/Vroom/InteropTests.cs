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

		public override void EmbeddingOfInstanceOfCustomValueTypeWithFields()
		{ }

		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithFields()
		{ }

		#endregion

		#region Objects with methods

		public override void EmbeddingOfInstanceOfBuiltinReferenceTypeWithMethod()
		{ }

		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithMethod()
		{ }

		#endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateWithoutParameters()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithOneParameter()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithTwoParameters()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithoutResult()
		{ }

		public override void EmbeddedInstanceOfDelegateHasFunctionPrototype()
		{ }

		public override void CallingOfEmbeddedDelegateWithMissingParameter()
		{ }

		public override void CallingOfEmbeddedDelegateWithExtraParameter()
		{ }

		#endregion

		#region Recursive calls

		public override void RecursiveExecutionOfFiles()
		{ }

		public override void RecursiveEvaluationOfFiles()
		{ }

		#endregion

		#endregion


		#region Embedding of types

		#region Types with fields

		public override void EmbeddingOfBuiltinValueTypeWithField()
		{ }

		public override void EmbeddingOfCustomReferenceTypeWithField()
		{ }

		#endregion

		#region Types with methods

		public override void EmbeddingOfBuiltinReferenceTypeWithMethods()
		{ }

		#endregion

		#endregion
	}
}