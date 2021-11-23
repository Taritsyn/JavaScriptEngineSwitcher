#if !NET452
using System;

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

		#region Types with methods

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithMethodsIsCorrect()
		{
			// Arrange
			Type mathType = typeof(Math);

			const string input1 = "Math2.Max(5.37, 5.56)";
			const double targetOutput1 = 5.56;

			const string input2 = "Math2.Log10(23)";
			const double targetOutput2 = 1.36172783601759;

			// Act
			double output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Math2", mathType);
				output1 = Math.Round(jsEngine.Evaluate<double>(input1), 2);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 14);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#endregion
	}
}
#endif