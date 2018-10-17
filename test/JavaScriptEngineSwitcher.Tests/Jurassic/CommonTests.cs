#if !NETCOREAPP1_0
using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "JurassicJsEngine"; }
		}


		#region Mapping errors

		[Fact]
		public override void MappingRuntimeErrorDuringEvaluationOfExpressionIsCorrect()
		{
			// Arrange
			const string input = @"var $variable1 = 611;
var _variable2 = 711;
var @variable3 = 678;

$variable1 + _variable2 - variable3;";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					int result = jsEngine.Evaluate<int>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.NotEmpty(exception.Message);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
		}

		[Fact]
		public override void MappingRuntimeErrorDuringExecutionOfCodeIsCorrect()
		{
			// Arrange
			const string input = @"function factorial(value) {
	if (value <= 0) {
		throw new Error(""The value must be greater than or equal to zero."");
	}

	return value !== 1 ? value * factorial(value - 1) : 1;
}

factorial(5);
factorial(@);
factorial(0);";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.NotEmpty(exception.Message);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
		}

		#endregion
	}
}
#endif