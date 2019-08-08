using System;
using System.IO;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
		}


		#region Embedding of objects

		#region Recursive calls

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringRecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/compilation-error";
			const string input = "evaluateFile('index').calculateResult();";

			// Act
			JsCompilationException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> evaluateFile = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
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
			Assert.Equal("Unexpected token ,", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(25, exception.LineNumber);
			Assert.Equal(11, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/runtime-error";
			const string input = "evaluateFile('index').calculateResult();";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> evaluateFile = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
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
			Assert.Equal("argumens is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(4, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Empty(exception.CallStack);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/host-error";
			const string input = "evaluateFile('index').calculateResult();";

			// Act
			FileNotFoundException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> evaluateFile = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
					double output = jsEngine.Evaluate<double>(input);
				}
				catch (FileNotFoundException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.StartsWith("Could not find file '", exception.Message);
		}

		[Fact]
		public void MappingCompilationErrorDuringRecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursive-execution/compilation-error";
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
					jsEngine.ExecuteFile(Path.Combine(directoryPath, "main-file.js"));

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
			Assert.Equal("Unexpected token ILLEGAL", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("second-file.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(6, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursive-execution/runtime-error";
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
					jsEngine.ExecuteFile(Path.Combine(directoryPath, "main-file.js"));

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
			Assert.Equal("nuм is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("second-file.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(1, exception.ColumnNumber);
			Assert.Equal("", exception.SourceFragment);
			Assert.Equal(
				"",
				exception.CallStack
			);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string directoryPath = "Files/recursive-execution/host-error";
			const string variableName = "num";

			// Act
			FileNotFoundException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Action<string> executeFile = path => jsEngine.ExecuteFile(path);

					jsEngine.SetVariableValue("directoryPath", directoryPath);
					jsEngine.EmbedHostObject("executeFile", executeFile);
					jsEngine.ExecuteFile(Path.Combine(directoryPath, "main-file.js"));

					int output = jsEngine.GetVariableValue<int>(variableName);
				}
				catch (FileNotFoundException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("File '" + directoryPath + "/second-file.jsx' not exist.", exception.Message);
		}

		#endregion

		#endregion

		#endregion
	}
}