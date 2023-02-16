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

		public override void EmbeddingOfInstanceOfCustomReferenceTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			var reflectionAllowedSettings = new V8Settings { AllowReflection = true };
			var reflectionDisallowedSettings = new V8Settings { AllowReflection = false };

			var cat = new Cat();

			const string itemName = "cat";
			const string input = "cat.GetType();";

			// Act
			string output1;

			using (var jsEngine1 = new V8JsEngine(reflectionAllowedSettings))
			{
				jsEngine1.EmbedHostObject(itemName, cat);
				output1 = jsEngine1.Evaluate<string>(input);
			}

			JsRuntimeException exception2 = null;

			using (var jsEngine2 = new V8JsEngine(reflectionDisallowedSettings))
			{
				try
				{
					jsEngine2.EmbedHostObject(itemName, cat);
					jsEngine2.Evaluate<string>(input);
				}
				catch (JsRuntimeException e)
				{
					exception2 = e;
				}
			}

			// Assert
			Assert.Equal(typeof(Cat).FullName, output1);

			Assert.NotNull(exception2);
			Assert.Equal("Runtime error", exception2.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception2.Description);
		}

#		endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateAndCheckingItsPrototype()
		{ }

		public override void EmbeddingOfInstanceOfDelegateAndGettingItsMethodProperty()
		{
			// Arrange
			var reflectionAllowedSettings = new V8Settings { AllowReflection = true };
			var reflectionDisallowedSettings = new V8Settings { AllowReflection = false };

			var cat = new Cat();
			var cryFunc = new Func<string>(cat.Cry);

			const string itemName = "cry";
			const string input = "cry.Method;";

			// Act
			string output1;

			using (var jsEngine1 = new V8JsEngine(reflectionAllowedSettings))
			{
				jsEngine1.EmbedHostObject(itemName, cryFunc);
				output1 = jsEngine1.Evaluate<string>(input);
			}

			JsRuntimeException exception2 = null;

			using (var jsEngine2 = new V8JsEngine(reflectionDisallowedSettings))
			{
				try
				{
					jsEngine2.EmbedHostObject(itemName, cryFunc);
					jsEngine2.Evaluate<string>(input);
				}
				catch (JsRuntimeException e)
				{
					exception2 = e;
				}
			}

			// Assert
			Assert.Equal("System.String Cry()", output1);

			Assert.NotNull(exception2);
			Assert.Equal("Runtime error", exception2.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception2.Description);
		}

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		public override void CreatingAnInstanceOfEmbeddedBuiltinExceptionAndGettingItsTargetSiteProperty()
		{
			// Arrange
			var reflectionAllowedSettings = new V8Settings { AllowReflection = true };
			var reflectionDisallowedSettings = new V8Settings { AllowReflection = false };

			Type invalidOperationExceptionType = typeof(InvalidOperationException);
			const string itemName = "InvalidOperationError";
			const string input = "new InvalidOperationError(\"A terrible thing happened!\").TargetSite;";

			// Act
			string output1;

			using (var jsEngine1 = new V8JsEngine(reflectionAllowedSettings))
			{
				jsEngine1.EmbedHostType(itemName, invalidOperationExceptionType);
				output1 = jsEngine1.Evaluate<string>(input);
			}

			JsRuntimeException exception2 = null;

			using (var jsEngine2 = new V8JsEngine(reflectionDisallowedSettings))
			{
				try
				{
					jsEngine2.EmbedHostType(itemName, invalidOperationExceptionType);
					jsEngine2.Evaluate<string>(input);
				}
				catch (JsRuntimeException e)
				{
					exception2 = e;
				}
			}

			// Assert
			Assert.Null(output1);

			Assert.NotNull(exception2);
			Assert.Equal("Runtime error", exception2.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception2.Description);
		}

		public override void CreatingAnInstanceOfEmbeddedCustomExceptionAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			var reflectionAllowedSettings = new V8Settings { AllowReflection = true };
			var reflectionDisallowedSettings = new V8Settings { AllowReflection = false };

			Type loginFailedExceptionType = typeof(LoginFailedException);
			const string itemName = "LoginFailedError";
			const string input = "new LoginFailedError(\"Wrong password entered!\").GetType();";

			// Act
			string output1;

			using (var jsEngine1 = new V8JsEngine(reflectionAllowedSettings))
			{
				jsEngine1.EmbedHostType(itemName, loginFailedExceptionType);
				output1 = jsEngine1.Evaluate<string>(input);
			}

			JsRuntimeException exception2 = null;

			using (var jsEngine2 = new V8JsEngine(reflectionDisallowedSettings))
			{
				try
				{
					jsEngine2.EmbedHostType(itemName, loginFailedExceptionType);
					jsEngine2.Evaluate<string>(input);
				}
				catch (JsRuntimeException e)
				{
					exception2 = e;
				}
			}

			// Assert
			Assert.Equal(typeof(LoginFailedException).FullName, output1);

			Assert.NotNull(exception2);
			Assert.Equal("Runtime error", exception2.Category);
			Assert.Equal("Use of reflection is prohibited in this script engine", exception2.Description);
		}

		#endregion

		#endregion
	}
}
#endif