using System;
using System.IO;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}


		#region Embedding of objects

		#region Recursive calls

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringRecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursiveEvaluation/compilationError";
			const string input = "require('index').calculateResult();";

			// Act
			JsCompilationException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> loadModule = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("require", loadModule);
					double output = jsEngine.Evaluate<double>(input);
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Compilation error", exception.Category);
			Assert.Equal("Expected identifier", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(25, exception.LineNumber);
			Assert.Equal(11, exception.ColumnNumber);
			Assert.Equal("		PI: 3,14,", exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursiveEvaluation/runtimeError";
			const string input = "require('index').calculateResult();";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> loadModule = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("require", loadModule);
					double output = jsEngine.Evaluate<double>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("'argumens' is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(4, exception.ColumnNumber);
			Assert.Equal("			result += argumens[i];", exception.SourceFragment);
			Assert.Equal(
				"   at sum (math.js:10:4)" + Environment.NewLine +
				"   at calculateResult (index.js:7:4)" + Environment.NewLine +
				"   at Global code (Script Document:1:1)",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursiveEvaluation/hostError";
			const string input = "require('index').calculateResult();";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> loadModule = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("require", loadModule);
					double output = jsEngine.Evaluate<double>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.StartsWith("During invocation of the host delegate an error has occurred - ",
				exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("index.js", exception.DocumentName);
			Assert.Equal(6, exception.LineNumber);
			Assert.Equal(3, exception.ColumnNumber);
			Assert.Equal("		var math = require('./match'),", exception.SourceFragment);
			Assert.Equal(
				"   at calculateResult (index.js:6:3)" + Environment.NewLine +
				"   at Global code (Script Document:1:1)",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingCompilationErrorDuringRecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursiveExecution/compilationError";
			const string variableName = "num";

			// Act
			JsCompilationException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Action<string> executeFile = path => jsEngine.ExecuteFile(path);

					jsEngine.SetVariableValue("directoryPath", directoryPath);
					jsEngine.EmbedHostObject("executeFile", executeFile);
					jsEngine.ExecuteFile(Path.Combine(directoryPath, "mainFile.js"));

					int output = jsEngine.GetVariableValue<int>(variableName);
				}
				catch (JsCompilationException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Compilation error", exception.Category);
			Assert.Equal("Invalid character", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("secondFile.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(6, exception.ColumnNumber);
			Assert.Equal("num -# 3;", exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursiveExecution/runtimeError";
			const string variableName = "num";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Action<string> executeFile = path => jsEngine.ExecuteFile(path);

					jsEngine.SetVariableValue("directoryPath", directoryPath);
					jsEngine.EmbedHostObject("executeFile", executeFile);
					jsEngine.ExecuteFile(Path.Combine(directoryPath, "mainFile.js"));

					int output = jsEngine.GetVariableValue<int>(variableName);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("'nuм' is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("secondFile.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(1, exception.ColumnNumber);
			Assert.Equal("nuм -= 3;", exception.SourceFragment);
			Assert.Equal(
				"   at Global code (secondFile.js:1:1)" + Environment.NewLine +
				"   at Global code (firstFile.js:2:1)" + Environment.NewLine +
				"   at Global code (mainFile.js:2:1)",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursiveExecution/hostError";
			const string variableName = "num";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Action<string> executeFile = path => jsEngine.ExecuteFile(path);

					jsEngine.SetVariableValue("directoryPath", directoryPath);
					jsEngine.EmbedHostObject("executeFile", executeFile);
					jsEngine.ExecuteFile(Path.Combine(directoryPath, "mainFile.js"));

					int output = jsEngine.GetVariableValue<int>(variableName);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.StartsWith(
				"During invocation of the host delegate an error has occurred - ",
				exception.Description
			);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("firstFile.js", exception.DocumentName);
			Assert.Equal(2, exception.LineNumber);
			Assert.Equal(1, exception.ColumnNumber);
			Assert.Equal(
				"executeFile(directoryPath + \"/secondFile.jsx\");",
				exception.SourceFragment
			);
			Assert.Equal(
				"   at Global code (firstFile.js:2:1)" + Environment.NewLine +
				"   at Global code (mainFile.js:2:1)",
				exception.CallStack
			);
		}

		#endregion

		#endregion

		#endregion
	}
}