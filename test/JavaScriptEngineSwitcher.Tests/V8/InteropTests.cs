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


		#region Embedding of objects

		#region Objects with methods

		public override void EmbeddingOfInstanceOfCustomValueTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			static string TestAllowReflectionSetting(bool allowReflection)
			{
				var date = new Date();

				const string input = "date.GetType();";

				using (var jsEngine = new V8JsEngine(new V8Settings { AllowReflection = allowReflection }))
				{
					jsEngine.EmbedHostObject("date", date);
					return jsEngine.Evaluate<string>(input);
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
			static string TestAllowReflectionSetting(bool allowReflection)
			{
				var cat = new Cat();

				const string input = "cat.GetType();";

				using (var jsEngine = new V8JsEngine(new V8Settings { AllowReflection = allowReflection }))
				{
					jsEngine.EmbedHostObject("cat", cat);
					return jsEngine.Evaluate<string>(input);
				}
			}

			// Act and Assert
			Assert.Equal(typeof(Cat).FullName, TestAllowReflectionSetting(true));

			JsRuntimeException exception = Assert.Throws<JsRuntimeException>(() => TestAllowReflectionSetting(false));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception.Description);
		}

#		endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateAndCheckingItsPrototype()
		{ }

		public override void EmbeddingOfInstanceOfDelegateAndGettingItsMethodProperty()
		{
			// Arrange
			static string TestAllowReflectionSetting(bool allowReflection)
			{
				var cat = new Cat();
				var cryFunc = new Func<string>(cat.Cry);

				const string input = "cry.Method;";

				using (var jsEngine = new V8JsEngine(new V8Settings { AllowReflection = allowReflection }))
				{
					jsEngine.EmbedHostObject("cry", cryFunc);
					return jsEngine.Evaluate<string>(input);
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
			static string TestAllowReflectionSetting(bool allowReflection)
			{
				Type invalidOperationExceptionType = typeof(InvalidOperationException);
				const string input = "new InvalidOperationError(\"A terrible thing happened!\").TargetSite;";

				using (var jsEngine = new V8JsEngine(new V8Settings { AllowReflection = allowReflection }))
				{
					jsEngine.EmbedHostType("InvalidOperationError", invalidOperationExceptionType);
					return jsEngine.Evaluate<string>(input);
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
			static string TestAllowReflectionSetting(bool allowReflection)
			{
				Type loginFailedExceptionType = typeof(LoginFailedException);
				const string input = "new LoginFailedError(\"Wrong password entered!\").GetType();";

				using (var jsEngine = new V8JsEngine(new V8Settings { AllowReflection = allowReflection }))
				{
					jsEngine.EmbedHostType("LoginFailedError", loginFailedExceptionType);
					return jsEngine.Evaluate<string>(input);
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