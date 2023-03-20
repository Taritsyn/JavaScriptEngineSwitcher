#if NET471 || NETCOREAPP3_1_OR_GREATER
using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}


		#region Error handling

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringEvaluationOfExpression()
		{
			// Arrange
			const string input = @"var $variable1 = 611;
var _variable2 = 711;
var @variable3 # 678;

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
			Assert.Equal("Unexpected token Hash: #", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(15, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringEvaluationOfExpression()
		{
			// Arrange
			const string input = @"var $variable1 = 611;
var _variable2 = 711;
var variable3 = 678;

$variable1 + _variable2() - variable3;";

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
			Assert.Equal("711 is not a function", exception.Description);
			Assert.Equal("TypeError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(5, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal("   at Global code (variables.js:5)", exception.CallStack);
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
factorial(%2);
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
			Assert.Equal("Unexpected token Mod: %", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("factorial.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(10, exception.ColumnNumber);
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
			Assert.Equal("The value must be greater than or equal to zero.", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("factorial.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(2, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at factorial (factorial.js:3:2)" + Environment.NewLine +
				"   at Global code (factorial.js:10)",
				exception.CallStack
			);
		}

		#endregion

		#region Generation of error messages

		[Fact]
		public void GenerationOfCompilationErrorMessage()
		{
			// Arrange
			const string input = @"var arr = [];
var obj = {};
var foo = 'Browser's bar;";
			string targetOutput = "SyntaxError: Undefined binary operation Identifier" + Environment.NewLine +
				"   at variables.js:3:1"
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
			string targetOutput = "TypeError: undefined is not a function" + Environment.NewLine +
				"   at foo (functions.js:4:2)" + Environment.NewLine +
				"   at Anonymous function (functions.js:12:1)" + Environment.NewLine +
				"   at Global code (functions.js:8)"
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