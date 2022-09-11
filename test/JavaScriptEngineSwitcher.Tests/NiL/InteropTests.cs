#if NET471 || NETCOREAPP3_1_OR_GREATER
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


		#region Embedding of objects

		#region Delegates

		[Fact]
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