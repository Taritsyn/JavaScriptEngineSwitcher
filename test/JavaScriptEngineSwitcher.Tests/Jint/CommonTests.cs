using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;

namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
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
			Assert.Equal("Unexpected token @", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("variables.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(5, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
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
			Assert.Empty(exception.SourceFragment);
			Assert.Equal("   at Global code (variables.js:5:15)", exception.CallStack);
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
			Assert.Equal("factorial.js", exception.DocumentName);
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
			Assert.Equal("The value must be greater than or equal to zero.", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("factorial.js", exception.DocumentName);
			Assert.Equal(3, exception.LineNumber);
			Assert.Equal(9, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at factorial (factorial.js:3:9)" + Environment.NewLine +
				"   at Global code (factorial.js:10:1)",
				exception.CallStack
			);
		}

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
			using (IJsEngine jsEngine = new JintJsEngine(
				new JintSettings
				{
					MemoryLimit = 2 * 1024 * 1024
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
			Assert.Matches(@"^Script has allocated \d+ but is limited to 2097152$", exception.Description);
		}

		[Fact]
		public void MappingRuntimeErrorDuringArraySizeExceeded()
		{
			// Arrange
			const string input = @"var arr = new Array(1000000000);";

			JsRuntimeException exception = null;

			// Act
			using (IJsEngine jsEngine = new JintJsEngine(
				new JintSettings
				{
					MaxArraySize = 1_000_000
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
			Assert.Equal("The array size 1000000000 is larger than maximum allowed (1000000)", exception.Description);
		}

		[Fact]
		public void MappingRuntimeErrorDuringMaxJsonParseDepthReached()
		{
			// Arrange
			const string input = @"var data = '{\n' +
	'	""menu"": {\n' +
	'		""id"": ""file"",\n' +
	'		""value"": ""File"",\n' +
	'		""popup"": {\n' +
	'			""menuItem"": [\n' +
	'				{ ""value"": ""New"", ""onclick"": ""CreateNewDoc()"" },\n' +
	'				{ ""value"": ""Open"", ""onclick"": ""OpenDoc()"" },\n' +
	'				{ ""value"": ""Close"", ""onclick"": ""CloseDoc()"" }\n' +
	'			]\n' +
	'		}\n' +
	'	}\n' +
	'}'
	;

JSON.parse(data);";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = new JintJsEngine(
				new JintSettings
				{
					MaxJsonParseDepth = 4
				}
			))
			{
				try
				{
					jsEngine.Execute(input, "menu.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Max. depth level of JSON reached at position 82", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("menu.js", exception.DocumentName);
			Assert.Equal(16, exception.LineNumber);
			Assert.Equal(1, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at Global code (parse menu.js:16:12)" + Environment.NewLine +
				"   at Global code (menu.js:16:1)",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursionDepthOverflow()
		{
			// Arrange
			const string input = @"function fibonacci(n) {
	if (n === 1) {
		return 1;
	}
	else if (n === 2) {
		return 1;
	}
	else {
		return fibonacci(n - 1) + fibonacci(n - 2);
	}
}

(function (fibonacci) {
	var a = 5;
	var b = 11;
	var c = fibonacci(b) - fibonacci(a);
})(fibonacci);";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = new JintJsEngine(
				new JintSettings
				{
					MaxRecursionDepth = 5
				}
			))
			{
				try
				{
					jsEngine.Execute(input, "fibonacci.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("The recursion is forbidden by script host.", exception.Description);
			Assert.Equal("RangeError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(0, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at fibonacci" + Environment.NewLine +
				"   at fibonacci" + Environment.NewLine +
				"   at fibonacci" + Environment.NewLine +
				"   at fibonacci" + Environment.NewLine +
				"   at fibonacci" + Environment.NewLine +
				"   at fibonacci" + Environment.NewLine +
				"   at Anonymous function",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingRuntimeErrorDuringStatementsCountOverflow()
		{
			// Arrange
			const string input = @"while (true);";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = new JintJsEngine(
				new JintSettings
				{
					MaxStatements = 5
				}
			))
			{
				try
				{
					jsEngine.Execute(input, "infinite-loop.js");
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("The maximum number of statements executed have been reached.", exception.Description);
			Assert.Equal("RangeError", exception.Type);
			Assert.Empty(exception.DocumentName);
			Assert.Equal(0, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Empty(exception.CallStack);
		}

		[Fact]
		public void MappingTimeoutErrorDuringExecutionOfCode()
		{
			// Arrange
			const string input = @"while (true);";

			JsTimeoutException exception = null;

			// Act
			using (var jsEngine = new JintJsEngine(
				new JintSettings
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

		[Fact]
		public void MappingTimeoutErrorDuringRegexHanging()
		{
			// Arrange
			const string input = @"var regexp = /^(\w+\s?)*$/,
	str = 'An input string that takes a long time or even makes this regular expression to hang!'
	;

// Will take a very long time
regexp.test(str);";

			JsTimeoutException exception = null;

			// Act
			using (var jsEngine = new JintJsEngine(
				new JintSettings
				{
					RegexTimeoutInterval = TimeSpan.FromMilliseconds(25)
				}
			))
			{
				try
				{
					jsEngine.Execute(input, "regexp-hanging.js");
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
		public void GenerationOfCompilationErrorMessage()
		{
			// Arrange
			const string input = @"var arr = [];
var obj = {};
var foo = 'Browser's bar';";
			string targetOutput = "SyntaxError: Unexpected identifier" + Environment.NewLine +
				"   at variables.js:3:20"
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
				"   at foo (functions.js:4:3)" + Environment.NewLine +
				"   at Anonymous function (functions.js:12:2)" + Environment.NewLine +
				"   at Global code (functions.js:13:2)"
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