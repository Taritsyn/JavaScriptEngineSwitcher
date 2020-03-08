#if !NETCOREAPP1_0
using System;

using Xunit;

namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JurassicJsEngine"; }
		}

		#region Embedding of objects

		#region Objects with properties

		public override void EmbeddingOfInstanceOfAnonymousTypeWithPropertiesIsCorrect()
		{ }

		#endregion

		#region Delegates

		public override void CallingOfEmbeddedDelegateWithMissingParameter()
		{
			// Arrange
			var sumFunc = new Func<int, int, int>((a, b) => a + b);

			const string input = "sum(678)";
			const int targetOutput = 678;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("sum", sumFunc);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#endregion

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
#endif