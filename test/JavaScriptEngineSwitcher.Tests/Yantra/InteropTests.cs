using System;
using System.IO;

using Xunit;

using JavaScriptEngineSwitcher.Core;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;
using JavaScriptEngineSwitcher.Tests.Interop.Logging;

namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}


		#region Embedding of objects

		#region Objects with fields

		[Fact]
		public override void EmbeddingOfInstanceOfCustomValueTypeWithFields()
		{
			// Arrange
			var date = new Date(2015, 12, 29);

			const string input1 = "date.Year";
			const int targetOutput1 = 2015;

			const string input2 = "date.Month";
			const int targetOutput2 = 12;

			const string input3 = "date.Day";
			const int targetOutput3 = 29;

			// Act
			int output1;
			int output2;
			int output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("date", date);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
		}

		[Fact]
		public override void EmbeddingOfInstanceOfCustomValueTypeWithReadonlyField()
		{
			// Arrange
			var age = new Age(1979);
			const string updateCode = "age.Year = 1982;";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.EmbedHostObject("age", age);
					jsEngine.Execute(updateCode);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Cannot modify property Year of 47 which has only a getter", exception.Description);
		}

		[Fact]
		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithFields()
		{
			// Arrange
			var product = new Product
			{
				Name = "Red T-shirt",
				Description = string.Empty,
				Price = 995.00
			};

			const string updateCode = @"product.Description = '';
product.Price *= 1.15;";

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

		#region Objects with properties

		[Fact]
		public override void EmbeddingOfInstanceOfBuiltinValueTypeWithProperties()
		{ }

		[Fact]
		public override void EmbeddingOfInstanceOfCustomValueTypeWithProperties()
		{ }

		[Fact]
		public override void EmbeddingOfInstanceOfCustomReferenceTypeWithProperties()
		{
			// Arrange
			var person = new Person("Vanya", "Ivanov");
			const string updateCode = @"person.LastName = person.LastName.substr(0, 5) + 'ff';
person.Patronymic = '';";

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
		public override void EmbeddingOfInstanceOfCustomValueTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			var date = new Date();

			const string input = "date.GetType();";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.EmbedHostObject("date", date);
					jsEngine.Evaluate<string>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Method GetType not found in JavaScriptEngineSwitcher.Tests.Interop.Date", exception.Description);
		}

		[Fact]
		public override void EmbeddingOfInstanceOfCustomReferenceTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			var cat = new Cat();

			const string input = @"cat.GetType();";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.EmbedHostObject("cat", cat);
					jsEngine.Evaluate<string>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Method GetType not found in JavaScriptEngineSwitcher.Tests.Interop.Animals.Cat", exception.Description);
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
			const double targetOutput = 132;

			// Act
			double output;

			using (var jsEngine = CreateJsEngine())
			{
				Func<string, object> evaluateFile = path => {
					string absolutePath = Path.Combine(directoryPath, $"{path}.js");
					string code = File.ReadAllText(absolutePath);
					object result = jsEngine.Evaluate(code, absolutePath);

					return result;
				};

				jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
				output = jsEngine.Evaluate<double>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveEvaluationOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/runtime-error";
			const string input = "evaluateFile('index').calculateResult();";
			const double targetOutput = double.NaN;

			// Act
			double output;

			using (var jsEngine = CreateJsEngine())
			{
				Func<string, object> evaluateFile = path => {
					string absolutePath = Path.Combine(directoryPath, $"{path}.js");
					string code = File.ReadAllText(absolutePath);
					object result = jsEngine.Evaluate(code, absolutePath);

					return result;
				};

				jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
				output = jsEngine.Evaluate<double>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveEvaluationOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/host-error";
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
			Assert.StartsWith("System.IO.FileNotFoundException: Could not find file ", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("index.js", exception.DocumentName);
			Assert.Equal(6, exception.LineNumber);
			Assert.Equal(2, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at calculateResult (index.js:6:2)" + Environment.NewLine +
				"   at Global code (Script Document:1:1)",
				exception.CallStack
			);
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
			Assert.Equal("Unexpected token Hash: #", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("second-file.js", exception.DocumentName);
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
			const int targetOutput = 15;

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
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public void MappingHostErrorDuringRecursiveExecutionOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-execution/host-error";
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
			Assert.StartsWith("System.IO.FileNotFoundException: File ", exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("first-file.js", exception.DocumentName);
			Assert.Equal(2, exception.LineNumber);
			Assert.Equal(0, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at Global code (first-file.js:2)" + Environment.NewLine +
				"   at Global code (main-file.js:2)",
				exception.CallStack
			);
		}

		#endregion

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedBuiltinValueType()
		{ }

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedCustomExceptionAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			Type loginFailedExceptionType = typeof(LoginFailedException);

			const string input = "new LoginFailedError(\"Wrong password entered!\").GetType();";
			string targetOutput = "function LoginFailedException() { [clr-native] }";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("LoginFailedError", loginFailedExceptionType);
				output = jsEngine.Evaluate<string>(input);
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

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithReadonlyFields()
		{
			// Arrange
			Type runtimeConstantsType = typeof(RuntimeConstants);
			const string updateCode = @"RuntimeConstants.MinValue = 1;
RuntimeConstants.MaxValue = 100;";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					jsEngine.EmbedHostType("RuntimeConstants", runtimeConstantsType);

					lock (RuntimeConstants.SyncRoot)
					{
						jsEngine.Execute(updateCode);
					}
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
				"Cannot modify property MinValue of function RuntimeConstants() { [clr-native] }" +
				" which has only a getter",
				exception.Description
			);
		}

		#endregion

		#region Types with methods

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithMethods()
		{
			// Arrange
			Type mathType = typeof(Math);

			const string input1 = "Math2.Max(5.37, 5.56)";
			const double targetOutput1 = 5;

			const string input2 = "Math2.Log10(23)";
			const double targetOutput2 = 1.36172783601759;

			// Act
			double output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Math2", mathType);
				output1 = jsEngine.Evaluate<double>(input1);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 14);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public override void EmbeddingOfTypeAndCallingOfItsGetTypeMethod()
		{ }

		#endregion

		#endregion
	}
}