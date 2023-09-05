#if NET48 || NETCOREAPP3_1_OR_GREATER
using Xunit;

namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class SecurityTests : TestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}


		[Fact]
		public void AccessingToProcess()
		{
			// Arrange
			const string input1 = @"typeof process === 'undefined';";
			const string input2 = @"let process = this.constructor.constructor('return this.process;')();
typeof process === 'undefined';";

			// Act
			bool output1 = false;
			bool output2 = false;

			using (var jsEngine = CreateJsEngine())
			{
				output1 = jsEngine.Evaluate<bool>(input1);
			}

			using (var jsEngine = CreateJsEngine())
			{
				output2 = jsEngine.Evaluate<bool>(input2);
			}

			// Assert
			Assert.True(output1);
			Assert.True(output2);
		}
	}
}
#endif