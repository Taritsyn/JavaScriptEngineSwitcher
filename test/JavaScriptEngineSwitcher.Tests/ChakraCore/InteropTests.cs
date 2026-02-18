using System;
using System.IO;
using System.Reflection;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.ChakraCore;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;

namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}


		private IJsEngine CreateJsEngine(bool allowReflection)
		{
			var jsEngine = new ChakraCoreJsEngine(new ChakraCoreSettings
			{
				AllowReflection = allowReflection
			});

			return jsEngine;
		}

		#region Embedding of objects

		#region Objects with fields

		[Fact]
		public override void EmbeddingOfInstanceOfCustomValueTypeWithReadonlyField()
		{ }

		#endregion

		#region Objects with methods

		[Fact]
		public override void EmbeddingOfInstanceOfCustomValueTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			string TestAllowReflectionSetting(bool allowReflection)
			{
				var date = new Date();

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostObject("date", date);
					return jsEngine.Evaluate<string>("date.GetType();");
				}
			}

			// Act and Assert
			Assert.Equal(typeof(Date).FullName, TestAllowReflectionSetting(true));

			var exception = Assert.Throws<JsRuntimeException>(() => TestAllowReflectionSetting(false));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Object doesn't support property or method 'GetType'", exception.Description);
		}

		[Fact]
		public override void EmbeddingOfInstanceOfCustomReferenceTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			string TestAllowReflectionSetting(bool allowReflection)
			{
				var cat = new Cat();

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostObject("cat", cat);
					return jsEngine.Evaluate<string>("cat.GetType();");
				}
			}

			// Act and Assert
			Assert.Equal(typeof(Cat).FullName, TestAllowReflectionSetting(true));

			JsRuntimeException exception = Assert.Throws<JsRuntimeException>(() => TestAllowReflectionSetting(false));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Object doesn't support property or method 'GetType'", exception.Description);
		}

		[Fact]
		public override void EmbeddingOfInstanceOfAssemblyTypeAndCallingOfItsCreateInstanceMethod()
		{
			// Arrange
			string TestAllowReflectionSetting(bool allowReflection)
			{
				Assembly assembly = this.GetType().Assembly;
				string personTypeName = typeof(Person).FullName;

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostObject("assembly", assembly);
					return jsEngine.Evaluate<string>("assembly.CreateInstance(\"" + personTypeName + "\");");
				}
			}

			// Act and Assert
			Assert.Equal("{FirstName=,LastName=}", TestAllowReflectionSetting(true));
			Assert.Equal("{FirstName=,LastName=}", TestAllowReflectionSetting(false));
		}

		#endregion

		#region Delegates

		[Fact]
		public override void EmbeddingOfInstanceOfDelegateAndGettingItsMethodProperty()
		{
			// Arrange
			string TestAllowReflectionSetting(bool allowReflection)
			{
				var cat = new Cat();
				var cryFunc = new Func<string>(cat.Cry);

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostObject("cry", cryFunc);
					return jsEngine.Evaluate<string>("cry.Method;");
				}
			}

			// Act and Assert
			Assert.Equal("undefined", TestAllowReflectionSetting(true));
			Assert.Equal("undefined", TestAllowReflectionSetting(false));
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
			Assert.Equal("Expected identifier", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(25, exception.LineNumber);
			Assert.Equal(11, exception.ColumnNumber);
			Assert.Equal("		PI: 3,14,", exception.SourceFragment);
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
			Assert.StartsWith("During invocation of the host delegate an error has occurred - ",
				exception.Description);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("index.js", exception.DocumentName);
			Assert.Equal(6, exception.LineNumber);
			Assert.Equal(3, exception.ColumnNumber);
			Assert.Equal("		var math = evaluateFile('./match'),", exception.SourceFragment);
			Assert.Equal(
				"   at calculateResult (index.js:6:3)" + Environment.NewLine +
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
			Assert.Equal("Invalid character", exception.Description);
			Assert.Equal("SyntaxError", exception.Type);
			Assert.Equal("second-file.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(6, exception.ColumnNumber);
			Assert.Equal("num -# 3;", exception.SourceFragment);
		}

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveExecutionOfFiles()
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
			Assert.Equal("'nuм' is not defined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("second-file.js", exception.DocumentName);
			Assert.Equal(1, exception.LineNumber);
			Assert.Equal(1, exception.ColumnNumber);
			Assert.Equal("nuм -= 3;", exception.SourceFragment);
			Assert.Equal(
				"   at Global code (second-file.js:1:1)" + Environment.NewLine +
				"   at Global code (first-file.js:2:1)" + Environment.NewLine +
				"   at Global code (main-file.js:2:1)",
				exception.CallStack
			);
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
			Assert.StartsWith(
				"During invocation of the host delegate an error has occurred - ",
				exception.Description
			);
			Assert.Equal("Error", exception.Type);
			Assert.Equal("first-file.js", exception.DocumentName);
			Assert.Equal(2, exception.LineNumber);
			Assert.Equal(1, exception.ColumnNumber);
			Assert.Equal(
				"executeFile(directoryPath + \"/second-file.jsx\");",
				exception.SourceFragment
			);
			Assert.Equal(
				"   at Global code (first-file.js:2:1)" + Environment.NewLine +
				"   at Global code (main-file.js:2:1)",
				exception.CallStack
			);
		}

		#endregion

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedBuiltinExceptionAndGettingItsTargetSiteProperty()
		{
			// Arrange
			string TestAllowReflectionSetting(bool allowReflection)
			{
				Type invalidOperationExceptionType = typeof(InvalidOperationException);

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostType("InvalidOperationError", invalidOperationExceptionType);
					return jsEngine.Evaluate<string>("new InvalidOperationError(\"A terrible thing happened!\").TargetSite;");
				}
			}

			// Act and Assert
			Assert.Null(TestAllowReflectionSetting(true));
			Assert.Equal("undefined", TestAllowReflectionSetting(false));
		}

		[Fact]
		public override void CreatingAnInstanceOfEmbeddedCustomExceptionAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			string TestAllowReflectionSetting(bool allowReflection)
			{
				Type loginFailedExceptionType = typeof(LoginFailedException);

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostType("LoginFailedError", loginFailedExceptionType);
					return jsEngine.Evaluate<string>("new LoginFailedError(\"Wrong password entered!\").GetType();");
				}
			}

			// Act and Assert
			Assert.Equal(typeof(LoginFailedException).FullName, TestAllowReflectionSetting(true));

			var exception = Assert.Throws<JsRuntimeException>(() => TestAllowReflectionSetting(false));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Object doesn't support property or method 'GetType'", exception.Description);
		}

		#endregion

		#region Types with fields

		[Fact]
		public override void EmbeddingOfCustomReferenceTypeWithReadonlyFields()
		{ }

		#endregion

		#region Types with methods

		[Fact]
		public override void EmbeddingOfTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			string dateTimeTypeName = typeof(DateTime).FullName;

			string TestAllowReflectionSetting(bool allowReflection)
			{
				Type type = typeof(Type);

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostType("Type", type);
					return jsEngine.Evaluate<string>("Type.GetType(\"" + dateTimeTypeName + "\");");
				}
			}

			// Act and Assert
			Assert.Equal(dateTimeTypeName, TestAllowReflectionSetting(true));
			Assert.Equal(dateTimeTypeName, TestAllowReflectionSetting(false));
		}

		[Fact]
		public override void EmbeddingOfAssemblyTypeAndCallingOfItsLoadMethod()
		{
			// Arrange
			const string reflectionEmitAssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

			string TestAllowReflectionSetting(bool allowReflection)
			{
				Type assemblyType = typeof(Assembly);

				using (var jsEngine = CreateJsEngine(allowReflection: allowReflection))
				{
					jsEngine.EmbedHostType("Assembly", assemblyType);
					return jsEngine.Evaluate<string>("Assembly.Load(\"" + reflectionEmitAssemblyName + "\");");
				}
			}

			// Act and Assert
			Assert.Equal(reflectionEmitAssemblyName, TestAllowReflectionSetting(true));
			Assert.Equal(reflectionEmitAssemblyName, TestAllowReflectionSetting(false));
		}

		#endregion

		#endregion
	}
}