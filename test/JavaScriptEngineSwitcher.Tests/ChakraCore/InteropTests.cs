using System;
using Xunit;

namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}

		[Fact]
		public void EmbeddedInstanceOfDelegateHasFunctionPrototype()
		{
			// Arrange
			var someFunc = new Func<int>(() => 42);

			const string input = "Object.getPrototypeOf(embeddedFunc) === Function.prototype";

			// Act
			bool output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("embeddedFunc", someFunc);
				output = jsEngine.Evaluate<bool>(input);
			}

			// Assert
			Assert.True(output);
		}
	}
}