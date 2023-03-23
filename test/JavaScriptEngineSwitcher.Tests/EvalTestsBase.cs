using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class EvalTestsBase : TestsBase
	{
		[Fact]
		public virtual void UsageOfEvalFunction()
		{
			// Arrange
			const string input = "eval('2*2');";
			const int targetOutput = 4;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void UsageOfFunctionConstructor()
		{
			// Arrange
			const string input = "new Function('return 2*2;')();";
			const int targetOutput = 4;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}
	}
}