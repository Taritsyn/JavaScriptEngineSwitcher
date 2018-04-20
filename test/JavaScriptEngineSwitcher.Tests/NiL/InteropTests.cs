using Xunit;

namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}


		#region Embedding of types

		#region Creating of instances

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedBuiltinValueTypeIsCorrect()
		{ }

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedBuiltinReferenceTypeIsCorrect()
		{ }

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedCustomValueTypeIsCorrect()
		{ }

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedCustomReferenceTypeIsCorrect()
		{ }

		#endregion

		#region Types with constants

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithConstantsIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomValueTypeWithConstantsIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithConstantIsCorrect()
		{ }

		#endregion

		#region Types with fields

		[Fact]
		public override void EmbeddingOfBuiltinValueTypeWithFieldIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithFieldIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomValueTypeWithFieldIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithFieldIsCorrect()
		{ }

		#endregion

		#region Types with properties

		[Fact]
		public override void EmbeddingOfBuiltinValueTypeWithPropertyIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithPropertyIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomValueTypeWithPropertyIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithPropertyIsCorrect()
		{ }

		#endregion

		#region Types with methods

		[Fact]
		public override void EmbeddingOfBuiltinValueTypeWithMethodIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithMethodsIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomValueTypeWithMethodIsCorrect()
		{ }

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithMethodIsCorrect()
		{ }

		#endregion

		#region Removal

		[Fact]
		public override void RemovingOfEmbeddedCustomReferenceTypeIsCorrect()
		{ }

		#endregion

		#endregion
	}
}