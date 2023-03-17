#if NET6_0_OR_GREATER
using System;
using System.IO;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;
using JavaScriptEngineSwitcher.Tests.Interop.Logging;

namespace JavaScriptEngineSwitcher.Tests.Topaz
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "TopazJsEngine"; }
		}


		#region Embedding of objects

		#region Objects with properties

		[Fact]
		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithProperties()
		{
			// Arrange
			var person = new Person("Vanya", "Ivanov");
			const string updateCode = @"person.LastName = 'Ivanoff';
person.Patronymic = null;";

			const string input1 = "person.FirstName";
			const string targetOutput1 = "Vanya";

			const string input2 = "person.LastName";
			const string targetOutput2 = "Ivanoff";

			// Act
			string output1;
			string output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("person", person);
				jsEngine.Execute(updateCode);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#region Objects with methods

		[Fact]
		public override void EmbeddingOfInstanceOfCustomValueTypeWithMethods()
		{
			// Arrange
			var programmerDayDate = new Date(2015, 9, 13);

			const string input1 = "programmerDay.GetDayOfYear()";
			const int targetOutput1 = 256;

			const string input2 = @"programmerDay.AddDays(6).GetDayOfYear();";
			const int targetOutput2 = 262;

			// Act
			int output1;
			int output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("programmerDay", programmerDayDate);
				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#region Delegates

		[Fact]
		public override void EmbeddingOfInstanceOfDelegateAndCheckingItsPrototype()
		{ }

		[Fact]
		public override void EmbeddingOfInstanceOfDelegateAndCallingItWithExtraParameter()
		{
			// Arrange
			var sumFunc = new Func<int, int, int>((a, b) => a + b);

			const string input = "sum(678, 711, 611)";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.EmbedHostObject("sum", sumFunc);
					jsEngine.Evaluate<int>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal(
				"Type Error: Delegate method call argument mismatch. delegate(678, 711, 611)",
				exception.Description
			);
		}

		[Fact]
		public override void EmbeddingOfInstanceOfDelegateAndGettingItsMethodProperty()
		{
			// Arrange
			var cat = new Cat();
			var cryFunc = new Func<string>(cat.Cry);

			const string input = "cry.Method;";
			string targetOutput = "System.String Cry()";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("cry", cryFunc);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Recursive calls

		#region Mapping of errors

		[Fact]
		public void MappingCompilationErrorDuringRecursiveEvaluationOfFiles()
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
			Assert.Empty(exception.DocumentName);
			Assert.Equal(25, exception.LineNumber);
			Assert.Equal(11, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveEvaluationOfFiles()
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
			Assert.Empty(exception.DocumentName);
			Assert.Equal(0, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Empty(exception.CallStack);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveEvaluationOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/host-error";
			const string input = "evaluateFile('index').calculateResult();";

			// Act
			JsException exception = null;

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
				catch (JsException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Unknown error", exception.Category);
			Assert.StartsWith("Could not find file ", exception.Description);
		}

		[Fact]
		public void MappingCompilationErrorDuringRecursiveExecutionOfFiles()
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
			Assert.Empty(exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(6, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveExecutionOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-execution/runtime-error";
			const string variableName = "num";

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				Action<string> executeFile = path => jsEngine.ExecuteFile(path);

				jsEngine.SetVariableValue("directoryPath", directoryPath);
				jsEngine.EmbedHostObject("executeFile", executeFile);
				jsEngine.ExecuteFile(Path.Combine(directoryPath, "main-file.js"));

				output = jsEngine.GetVariableValue<int>(variableName);
			}

			// Assert
			Assert.Equal(15, output);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveExecutionOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-execution/host-error";
			const string variableName = "num";

			// Act
			JsException exception = null;

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
				catch (JsException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Unknown error", exception.Category);
			Assert.StartsWith("File ", exception.Description);
		}

		#endregion

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedBuiltinReferenceType()
		{
			// Arrange
			Type uriType = typeof(Uri);

			const string inputCode = @"var baseUri = new Uri('https://github.com'),
	relativeUri = 'Taritsyn/MsieJavaScriptEngine'
	;";
			const string inputExpression = @"(new Uri(baseUri, relativeUri)).ToString()";
			const string targetOutput = "https://github.com/Taritsyn/MsieJavaScriptEngine";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Uri", uriType);
				jsEngine.Execute(inputCode);
				output = jsEngine.Evaluate<string>(inputExpression);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with fields

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithField()
		{
			// Arrange
			Type defaultLoggerType = typeof(DefaultLogger);

			const string input = "DefaultLogger.Current.ToString()";
			const string targetOutput = "[null logger]";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("DefaultLogger", defaultLoggerType);

				lock (DefaultLogger.SyncRoot)
				{
					output = jsEngine.Evaluate<string>(input);
				}
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with properties

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithProperty()
		{
			// Arrange
			Type bundleTableType = typeof(BundleTable);

			const string input = "BundleTable.EnableOptimizations";
			const bool targetOutput = true;

			// Act
			bool output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("BundleTable", bundleTableType);

				lock (BundleTable.SyncRoot)
				{
					output = jsEngine.Evaluate<bool>(input);
				}
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#endregion
	}
}
#endif