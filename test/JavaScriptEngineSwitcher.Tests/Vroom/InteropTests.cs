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

		#endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateWithoutParametersIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithOneParameterIsCorrect()
		{ }

		public override void EmbeddingOfInstanceOfDelegateWithTwoParametersIsCorrect()
		{ }

		#endregion

		#endregion


		#region Embedding of types

		#region Types with fields

		public override void EmbeddingOfBuiltinValueTypeWithFieldIsCorrect()
		{ }

		#endregion

		#region Types with methods

		public override void EmbeddingOfBuiltinReferenceTypeWithMethodsIsCorrect()
		{ }

		#endregion

		#endregion
	}
}