using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class PrecompilationTests : PrecompilationTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}


		#region Error handling

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringPrecompilationOfCodeIsCorrect()
		{
			// Arrange
			const string input = @"function guid() {
	function s4() {
		return Math.floor((1 + Math.random() * 0x10000)
			.toString(16)
			.substring(1)
			;
	}

	var result = s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();

	return result;
}";

			IPrecompiledScript precompiledScript = null;
			JsCompilationException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					precompiledScript = jsEngine.Precompile(input, "guid.js");
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.Null(precompiledScript);
			Assert.NotNull(exception);
			Assert.Equal("Compilation error", exception.Category);
			Assert.Equal("Expected ')'", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("guid.js", exception.DocumentName);
			Assert.Equal(6, exception.LineNumber);
			Assert.Equal(4, exception.ColumnNumber);
			Assert.Equal("			;", exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringExecutionOfPrecompiledCodeIsCorrect()
		{
			// Arrange
			const string input = @"function getItem(items, itemIndex) {
	var item = items[itemIndex];

	return item;
}

(function (getItem) {
	var items = null,
		item = getItem(items, 5)
		;

	return item;
})(getItem);";

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					IPrecompiledScript precompiledScript = jsEngine.Precompile(input, "getItem.js");
					jsEngine.Execute(precompiledScript);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Unable to get property '5' of undefined or null reference", exception.Description);
			Assert.Equal("TypeError", exception.Type);
			Assert.Equal("getItem.js", exception.DocumentName);
			Assert.Equal(2, exception.LineNumber);
			Assert.Equal(2, exception.ColumnNumber);
			Assert.Equal("	var item = items[itemIndex];", exception.SourceFragment);
			Assert.Equal(
				"   at getItem (getItem.js:2:2)" + Environment.NewLine +
				"   at Anonymous function (getItem.js:9:3)" + Environment.NewLine +
				"   at Global code (getItem.js:7:2)",
				exception.CallStack
			);
		}

		#endregion

		#region Generation of error messages

		[Fact]
		public void GenerationOfCompilationErrorMessageIsCorrect()
		{
			// Arrange
			const string input = @"function makeId(length) {
	var result = '',
		possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',
		charIndex
		;

	for (charIndex = 0; charIndex < length; charIndex++) 
		result += possible.charAt(Math.floor(Math.random() * possible.length));
	}

	return result;
}";
			string targetOutput = "SyntaxError: 'return' statement outside of function" + Environment.NewLine +
				"   at makeId.js:11:2 -> 	return result;"
				;

			IPrecompiledScript precompiledScript = null;
			JsCompilationException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					precompiledScript = jsEngine.Precompile(input, "makeId.js");
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			Assert.Null(precompiledScript);
			Assert.NotNull(exception);
			Assert.Equal(targetOutput, exception.Message);
		}

		[Fact]
		public void GenerationOfRuntimeErrorMessageIsCorrect()
		{
			// Arrange
			const string input = @"function getFullName(firstName, lastName) {
	var fullName = firstName + ' ' + middleName + ' ' + lastName;

	return fullName;
}

(function (getFullName) {
	var firstName = 'Vasya',
		lastName = 'Pupkin'
		;

	return getFullName(firstName, lastName);
})(getFullName);";
			string targetOutput = "ReferenceError: 'middleName' is not defined" + Environment.NewLine +
				"   at getFullName (getFullName.js:2:2) -> 	var fullName = firstName + ' ' + middleName + " +
				"' ' + lastName;" + Environment.NewLine +
				"   at Anonymous function (getFullName.js:12:2)" + Environment.NewLine +
				"   at Global code (getFullName.js:7:2)"
				;

			JsRuntimeException exception = null;

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					IPrecompiledScript precompiledScript = jsEngine.Precompile(input, "getFullName.js");
					jsEngine.Execute(precompiledScript);
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