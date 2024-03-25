using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}


		#region Error handling

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringEvaluationOfExpression()
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
			Assert.Equal("Invalid or unexpected token", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(5, exception.ColumnNumber);
			Assert.Equal("var @variable3 = 678;", exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringEvaluationOfExpression()
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
			Assert.Equal("variable2 is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(5, exception.LineNumber);
			Assert.Equal(15, exception.ColumnNumber);
			Assert.Equal("$variable1 + -variable2 - variable3;", exception.SourceFragment);
			Assert.Equal("   at variables.js:5:15", exception.CallStack);
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
			Assert.Equal("Unexpected token ')'", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("factorial.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(13, exception.ColumnNumber);
			Assert.Equal("factorial(2%);", exception.SourceFragment);
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
			Assert.Equal(9, exception.ColumnNumber);
			Assert.Equal("		throw new Error(\"The value must be greater than or equal to zero.\");", exception.SourceFragment);
			Assert.Equal(
				"   at factorial (factorial.js:3:9)" + Environment.NewLine +
				"   at factorial.js:10:1",
				exception.CallStack
			);
		}

		/*
		[Fact]
		public void MappingRuntimeErrorDuringOutOfMemory()
		{
			// Arrange
			const string input = @"var arr = [];

for (var i = 0; i < 10000; i++) {
	arr.push('Current date: ' + new Date());
}";

			JsRuntimeException exception = null;

			// Act
			using (IJsEngine jsEngine = new V8JsEngine(
				new V8Settings
				{
					MaxHeapSize = new UIntPtr(640 * 1024)
				}
			))
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
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("The V8 runtime has exceeded its memory limit", exception.Description);
		}
		*/

		#endregion

		#region Generation of error messages

		[Fact]
		public void GenerationOfCompilationErrorMessage()
		{
			// Arrange
			const string input = @"var arr = [];
var obj = {};
var foo = 'Browser's bar';";
			string targetOutput = "SyntaxError: Unexpected identifier 's'" + Environment.NewLine +
				"   at variables.js:3:20 -> var foo = 'Browser's bar';"
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
			string targetOutput = "ReferenceError: bar is not defined" + Environment.NewLine +
				"   at foo (functions.js:4:3) -> 		bar();" + Environment.NewLine +
				"   at functions.js:12:2" + Environment.NewLine +
				"   at functions.js:13:3"
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