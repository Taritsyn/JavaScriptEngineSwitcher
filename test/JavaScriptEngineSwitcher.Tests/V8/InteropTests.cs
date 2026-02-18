using System;
using System.Reflection;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;

namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}


		private IJsEngine CreateJsEngine(bool allowReflection)
		{
			var jsEngine = new V8JsEngine(new V8Settings
			{
				AllowReflection = allowReflection
			});

			return jsEngine;
		}

		#region Embedding of objects

		#region Objects with fields

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
			Assert.Equal("The field is read-only", exception.Description);
		}

		#endregion

		#region Objects with methods

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
			Assert.Equal("Use of reflection is prohibited in this script engine", exception.Description);
		}

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
			Assert.Equal("Use of reflection is prohibited in this script engine", exception.Description);
		}

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

		public override void EmbeddingOfInstanceOfDelegateAndCheckingItsPrototype()
		{ }

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
			Assert.Equal("System.String Cry()", TestAllowReflectionSetting(true));

			var exception = Assert.Throws<JsRuntimeException>(() => TestAllowReflectionSetting(false));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception.Description);
		}

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

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

			var exception = Assert.Throws<JsRuntimeException>(() => TestAllowReflectionSetting(false));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception.Description);
		}

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
			Assert.Equal("Use of reflection is prohibited in this script engine", exception.Description);
		}

		#endregion

		#region Types with fields

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
			Assert.Equal("The field is read-only", exception.Description);
		}

		#endregion

		#region Types with methods

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