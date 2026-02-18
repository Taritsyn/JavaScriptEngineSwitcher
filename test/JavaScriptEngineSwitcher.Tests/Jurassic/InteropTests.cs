using System;

using Xunit;

using JavaScriptEngineSwitcher.Tests.Interop;

namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JurassicJsEngine"; }
		}

		#region Embedding of objects

		#region Objects with fields

		public override void EmbeddingOfInstanceOfCustomValueTypeWithReadonlyField()
		{
			// Arrange
			var age = new Age(1979);
			const string updateCode = "age.Year = 1982;";

			const string input = "age.Year";
			const int targetOutput = 1982;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("age", age);
				jsEngine.Execute(updateCode);

				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Objects with properties

		public override void EmbeddingOfInstanceOfAnonymousTypeWithProperties()
		{ }

		#endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateAndCallingItWithMissingParameter()
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

		public override void EmbeddingOfBuiltinReferenceTypeWithConstants()
		{ }

		public override void EmbeddingOfCustomValueTypeWithConstants()
		{ }

		public override void EmbeddingOfCustomReferenceTypeWithConstant()
		{ }

		#endregion

		#endregion
	}
}