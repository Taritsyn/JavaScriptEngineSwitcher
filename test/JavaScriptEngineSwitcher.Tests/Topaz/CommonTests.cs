#if NET6_0_OR_GREATER
using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.Topaz
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "TopazJsEngine"; }
		}

		#region Evaluation of scripts

		[Fact]
		public override void EvaluationOfExpressionWithBooleanResult()
		{ }

		[Fact]
		public override void EvaluationOfExpressionWithDoubleResult()
		{
			// Math object not implemented
		}

		#endregion

		#region Error handling

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringEvaluationOfExpression()
		{
			// Arrange
			const string inputCode = @"var $variable1 = 611;
var _variable2 = 711;
var variable3 = 678;";
			const string inputExpression = "$variable1 + _variable2 - @variable3;";

			JsCompilationException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(inputCode);
					int result = jsEngine.Evaluate<int>(inputExpression, "variables.js");
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Compilation error", exception.Category);
			Assert.Equal("Unexpected token ILLEGAL", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(27, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringEvaluationOfExpression()
		{
			// Arrange
			const string inputCode = @"var $variable1 = 611;
var _variable2 = 711;
var variable3 = 678;";
			const string inputExpression = @"$variable1 + _variable2() - variable3;";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.Execute(inputCode);
					int result = jsEngine.Evaluate<int>(inputExpression, "variables.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Can not call 711 as a function.", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(0, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Empty(exception.CallStack);
		}

		[Fact]
		public void MappingCompilationErrorDuringExecutionOfCode()
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
			Assert.Equal("Unexpected token )", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(13, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringExecutionOfCode()
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
			Assert.Equal("Constructor call on undefined is not defined.", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(0, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Empty(exception.CallStack);
		}

		#endregion

		#region Generation of error messages

		[Fact]
		public void GenerationOfCompilationErrorMessage()
		{
			// Arrange
			const string input = @"var arr = [];
var obj = {};
var foo = 'Browser's bar';";
			string targetOutput = "SyntaxError: Unexpected identifier" + Environment.NewLine +
				"   at 3:20";

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
		public void GenerationOfRuntimeErrorMessage()
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
			string targetOutput = "Error: Can not call undefined as a function.";

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