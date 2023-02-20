using System;

using Xunit;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;

namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "JurassicJsEngine"; }
		}

		#region Embedding of objects

		#region Objects with properties

		public override void EmbeddingOfInstanceOfAnonymousTypeWithProperties()
		{ }

		#endregion

		#region Objects with methods

		public override void EmbeddingOfInstanceOfCustomValueTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			var date = new Date();

			const string input = "date.GetType();";
			string targetOutput = typeof(Date).FullName;

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("date", date);
				output = jsEngine.Evaluate(input).ToString();
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		public override void EmbeddingOfInstanceOfCustomReferenceTypeAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			var cat = new Cat();

			const string input = "cat.GetType();";
			string targetOutput = typeof(Cat).FullName;

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("cat", cat);
				output = jsEngine.Evaluate(input).ToString();
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Delegates

		public override void EmbeddingOfInstanceOfDelegateAndCallingItWithMissingParameter()
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

		public override void EmbeddingOfInstanceOfDelegateAndGettingItsMethodProperty()
		{
			// Arrange
			var cat = new Cat();
			var cryFunc = new Func<string>(cat.Cry);

			const string input = "cry.Method;";
			string targetOutput = "undefined";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("cry", cryFunc);
				output = jsEngine.Evaluate(input).ToString();
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		public override void CreatingAnInstanceOfEmbeddedBuiltinExceptionAndGettingItsTargetSiteProperty()
		{
			// Arrange
			Type invalidOperationExceptionType = typeof(InvalidOperationException);

			const string input = "new InvalidOperationError(\"A terrible thing happened!\").TargetSite;";

			// Act
			string output;
			const string targetOutput = "undefined";

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("InvalidOperationError", invalidOperationExceptionType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		public override void CreatingAnInstanceOfEmbeddedCustomExceptionAndCallingOfItsGetTypeMethod()
		{
			// Arrange
			Type loginFailedExceptionType = typeof(LoginFailedException);

			const string input = "new LoginFailedError(\"Wrong password entered!\").GetType();";
			string targetOutput = loginFailedExceptionType.FullName;

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("LoginFailedError", loginFailedExceptionType);
				output = jsEngine.Evaluate(input).ToString();
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with constants

		public override void EmbeddingOfBuiltinReferenceTypeWithConstants()
		{ }

		public override void EmbeddingOfCustomValueTypeWithConstants()
		{ }

		public override void EmbeddingOfCustomReferenceTypeWithConstant()
		{ }

		#endregion

		#endregion
	}
}