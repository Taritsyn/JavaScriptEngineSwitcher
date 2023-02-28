#if NETFRAMEWORK || NETCOREAPP3_1_OR_GREATER
using System;

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

		#endregion
	}
}
#endif