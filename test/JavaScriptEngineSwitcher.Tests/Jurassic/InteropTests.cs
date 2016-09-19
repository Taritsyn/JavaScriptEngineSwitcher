namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JurassicJsEngine"; }
		}

		#region Embedding of types

		#region Types with constants

		public override void EmbeddingOfBuiltinReferenceTypeWithConstantsIsCorrect()
		{ }

		public override void EmbeddingOfCustomValueTypeWithConstantsIsCorrect()
		{ }

		public override void EmbeddingOfCustomReferenceTypeWithConstantIsCorrect()
		{ }

		#endregion

		#endregion
	}
}