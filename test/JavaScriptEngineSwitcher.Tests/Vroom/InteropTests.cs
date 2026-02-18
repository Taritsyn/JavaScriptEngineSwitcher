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

		public override void EmbeddingOfInstanceOfCustomValueTypeWithReadonlyField()
		{ }

		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithFields()
		{ }

		#endregion

		#region Objects with methods

		public override void EmbeddingOfInstanceOfBuiltinReferenceTypeWithMethod()
		{ }

		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithMethod()
		{ }

		public override void EmbeddingOfInstanceOfAssemblyTypeAndCallingOfItsCreateInstanceMethod()
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

		public override void EmbeddingOfInstanceOfDelegateAndCheckingItsPrototype()
		{ }

		public override void EmbeddingOfInstanceOfDelegateAndCallingItWithMissingParameter()
		{ }

		public override void EmbeddingOfInstanceOfDelegateAndCallingItWithExtraParameter()
		{ }

		public override void EmbeddingOfInstanceOfDelegateAndGettingItsMethodProperty()
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

		public override void EmbeddingOfCustomReferenceTypeWithReadonlyFields()
		{ }

		#endregion

		#region Types with methods

		public override void EmbeddingOfBuiltinReferenceTypeWithMethods()
		{ }

		public override void EmbeddingOfTypeAndCallingOfItsGetTypeMethod()
		{ }

		public override void EmbeddingOfAssemblyTypeAndCallingOfItsLoadMethod()
		{ }

		#endregion

		#endregion
	}
}