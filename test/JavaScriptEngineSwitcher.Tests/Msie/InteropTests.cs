using System;
using System.IO;
using System.Reflection;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;

namespace JavaScriptEngineSwitcher.Tests.Msie
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "MsieJsEngine"; }
		}


		private IJsEngine CreateJsEngine(bool allowReflection)
		{
			var jsEngine = new MsieJsEngine(new MsieSettings
			{
				AllowReflection = allowReflection,
				EngineMode = JsEngineMode.ChakraIeJsRt
			});

			return jsEngine;
		}

		#region Embedding of objects

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
			Assert.Equal("'argumens' is undefined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(4, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at sum (math.js:10:4)" + Environment.NewLine +
				"   at calculateResult (index.js:7:4)" + Environment.NewLine +
				"   at Global code (Script Document:1:1)",
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