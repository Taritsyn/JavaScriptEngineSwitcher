#if !NET452
using System;
using System.IO;

using Xunit;

using JavaScriptEngineSwitcher.Core;

using JavaScriptEngineSwitcher.Tests.Interop;

namespace JavaScriptEngineSwitcher.Tests.Jint
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JintJsEngine"; }
		}


		#region Embedding of objects

		#region Objects with fields

		[Fact]
		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithFieldsIsCorrect()
		{
			// Arrange
			var product = new Product
			{
				Name = "Red T-shirt",
				Description = string.Empty,
				Price = 995.00
			};

			const string updateCode = "product.Price *= 1.15;";

			const string input1 = "product.Name";
			const string targetOutput1 = "Red T-shirt";

			const string input2 = "product.Price";
			const double targetOutput2 = 1144.25;

			// Act
			string output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("product", product);
				jsEngine.Execute(updateCode);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<double>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#region Delegates

		[Fact]
		public override void CallingOfEmbeddedDelegateWithMissingParameter()
		{
			// Arrange
			var sumFunc = new Func<int, int, int>((a, b) => a + b);

			const string input = "sum(678)";
			const int targetOutput = 678;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("sum", sumFunc);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

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
			Assert.Equal(
				"   at sum (math.js:10:14)" + Environment.NewLine +
				"   at calculateResult (index.js:7:13)" + Environment.NewLine +
				"   at Global code (Script Document:1:1)",
				exception.CallStack
			);
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
			Assert.Equal("Unexpected number", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("second-file.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(8, exception.ColumnNumber);
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
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at Global code (second-file.js:1:1)" + Environment.NewLine +
				"   at Global code (first-file.js:2:1)" + Environment.NewLine +
				"   at Global code (main-file.js:2:1)",
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


		#region Embedding of types

		#region Types with methods

		#if NET471
		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithMethodsIsCorrect()
		{
			// Arrange
			Type mathType = typeof(Math);

			const string input1 = "Math2.Max(5.37, 5.56)";
			const double targetOutput1 = 5.56;

			const string input2 = "Math2.Log10(23)";
			const double targetOutput2 = 1.36172783601759;

			// Act
			double output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Math2", mathType);
				output1 = Math.Round(jsEngine.Evaluate<double>(input1), 2);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 14);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}
		#endif

		#endregion

		#endregion
	}
}
#endif