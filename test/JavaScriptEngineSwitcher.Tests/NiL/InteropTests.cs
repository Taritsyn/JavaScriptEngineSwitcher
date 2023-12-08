using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;

namespace JavaScriptEngineSwitcher.Tests.NiL
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "NiLJsEngine"; }
		}


		#region Embedding of objects

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
			Assert.Equal("date.GetType is not a function", exception.Description);
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
			Assert.Equal("cat.GetType is not a function", exception.Description);
		}

		#endregion

		#region Delegates

		[Fact]
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

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		[Fact]
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

		#endregion

		#region Types with methods

		[Fact]
		public override void EmbeddingOfBuiltinReferenceTypeWithMethods()
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

		#endregion

		#endregion
	}
}