#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1
using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Node;

namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}


		#region Evaluation of scripts

		[Fact]
		public override void EvaluationOfExpressionWithUndefinedResultIsCorrect()
		{ }

		[Fact]
		public override void EvaluationOfExpressionWithNullResultIsCorrect()
		{
			// Arrange
			const string input = "null";
			const string targetOutput = "null";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Calling of functions

		[Fact]
		public override void CallingOfFunctionWithoutParametersIsCorrect()
		{
			// Arrange
			const string functionCode = @"function hooray() {
	return 'Hooray!';
}";
			const string targetOutput = "Hooray!";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(functionCode);
				output = jsEngine.CallFunction<string>("hooray");
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public override void CallingOfFunctionWithUndefinedResultIsCorrect()
		{ }

		[Fact]
		public override void CallingOfFunctionWithNullResultIsCorrect()
		{
			// Arrange
			const string functionCode = @"function testNull(value) {
	if (value !== null) {
		throw new TypeError();
	}

	return null;
}";
			const object input = null;
			const string targetOutput = "null";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(functionCode);
				output = jsEngine.CallFunction<string>("testNull", input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public override void CallingOfFunctionWithManyParametersIsCorrect()
		{ }

		#endregion

		#region Getting, setting and removing variables

		[Fact]
		public override void SettingAndGettingVariableWithUndefinedValueIsCorrect()
		{ }

		[Fact]
		public override void SettingAndGettingVariableWithNullValueIsCorrect()
		{
			// Arrange
			const string variableName = "myVar2";
			const object input = null;
			const string targetOutput = "null";

			// Act
			bool variableExists;
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.SetVariableValue(variableName, input);
				variableExists = jsEngine.HasVariable(variableName);
				output = jsEngine.GetVariableValue<string>(variableName);
			}

			// Assert
			Assert.True(variableExists);
			Assert.Equal(targetOutput, output);
		}

		#endregion

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
			Assert.Equal("Invalid or unexpected token", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(5, exception.ColumnNumber);
			Assert.Equal("var @variable3 = 678;", exception.SourceFragment);
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
			Assert.Equal("variable2 is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(5, exception.LineNumber);
			Assert.Equal(15, exception.ColumnNumber);
			Assert.Equal("$variable1 + -variable2 - variable3;", exception.SourceFragment);
			Assert.Equal("   at variables.js:5:15", exception.CallStack);
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
			Assert.Equal("Unexpected token ')'", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("factorial.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(13, exception.ColumnNumber);
			Assert.Equal("factorial(2%);", exception.SourceFragment);
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
			Assert.Equal("factorial.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(9, exception.ColumnNumber);
			Assert.Equal(
				"		throw new Error(\"The value must be greater than or equal to zero.\");",
				exception.SourceFragment
			);
			Assert.Equal(
				"   at factorial (factorial.js:3:9)" + Environment.NewLine +
				"   at factorial.js:10:1",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingTimeoutErrorDuringExecutionOfCodeIsCorrect()
		{
			// Arrange
			const string input = @"while (true);";

			JsTimeoutException exception = null;

			// Act
			using (var jsEngine = new NodeJsEngine(
				new NodeSettings
				{
					TimeoutInterval = TimeSpan.FromMilliseconds(30)
				}
			))
			{
				try
				{
					jsEngine.Execute(input, "infinite-loop.js");
				}
				catch (JsTimeoutException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Timeout error", exception.Category);
			Assert.Equal("Script execution exceeded timeout.", exception.Description);
			Assert.Empty(exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(0, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
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
			string targetOutput = "SyntaxError: Unexpected identifier" + Environment.NewLine +
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
#endif