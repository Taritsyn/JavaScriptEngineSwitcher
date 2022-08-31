#if NET471 || NETCOREAPP3_1_OR_GREATER
using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}

		#region Error handling

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringEvaluationOfExpressionIsCorrect()
		{
			// Arrange
			const string input = @"var $variable1 = 611;
var _variable2 = 711;
var @variable3 = 678;

$variable1 + _variable2 - @variable3;";

			JsCompilationException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					int result = jsEngine.Evaluate<int>(input, "variables.js");
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Compilation error", exception.Category);
			Assert.Equal("Invalid variable definition", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(5, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringEvaluationOfExpressionIsCorrect()
		{
			// Arrange
			const string input = @"var $variable1 = 611;
var _variable2 = 711;
var variable3 = 678;

$variable1 + -variable2 - variable3;";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					int result = jsEngine.Evaluate<int>(input, "variables.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Variable \"variable2\" is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(5, exception.LineNumber);
			Assert.Equal(15, exception.ColumnNumber);
			Assert.Equal("$variable1 + -variable2 - variable3;", exception.SourceFragment);
			Assert.Empty(exception.CallStack);
		}

		[Fact]
		public void MappingCompilationErrorDuringExecutionOfCodeIsCorrect()
		{
			// Arrange
			const string input = @"function factorial(value) {
	if (value <= 0) {
		throw new Error(""The value must be greater than or equal to zero."");
	}

	return value !== 1 ? value * factorial(value - 1) : 1;
}

factorial(5);
factorial(2%);
factorial(0);";

			JsCompilationException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(input, "factorial.js");
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Compilation error", exception.Category);
			Assert.Equal("Unexpected token", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(13, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringExecutionOfCodeIsCorrect()
		{
			// Arrange
			const string input = @"function factorial(value) {
	if (value <= 0) {
		throw new Error(""The value must be greater than or equal to zero."");
	}

	return value !== 1 ? value * factorial(value - 1) : 1;
}

factorial(5);
factorial(-1);
factorial(0);";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(input, "factorial.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("The value must be greater than or equal to zero.", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(3, exception.ColumnNumber);
			Assert.Equal(
				"		throw new Error(\"The value must be greater than or equal to zero.\");",
				exception.SourceFragment
			);
			Assert.Empty(exception.CallStack);
		}

		#endregion

		#region Generation of error messages

		[Fact]
		public void GenerationOfCompilationErrorMessageIsCorrect()
		{
			// Arrange
			const string input = @"var arr = [];
var obj = {};
var foo = 'Browser's bar';";
			string targetOutput = "SyntaxError: Unexpected token 's'" + Environment.NewLine +
				"   at 3:20"
				;

			JsCompilationException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(input, "variables.js");
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			Assert.NotNull(exception);
			Assert.Equal(targetOutput, exception.Message);
		}

		[Fact]
		public void GenerationOfRuntimeErrorMessageIsCorrect()
		{
			// Arrange
			const string input = @"function foo(x, y) {
	var z = x + y;
	if (z > 20) {
		bar();
	}
}

(function (foo) {
	var a = 8;
	var b = 15;

	foo(a, b);
})(foo);";
			string targetOutput = "ReferenceError: Variable \"bar\" is not defined" + Environment.NewLine +
				"   at 4:3 -> 		bar();"
				;

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(input, "functions.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			Assert.NotNull(exception);
			Assert.Equal(targetOutput, exception.Message);
		}

		#endregion

		#endregion
	}
}
#endif