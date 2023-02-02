using System.Threading;

using Xunit;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class MultithreadingTestsBase : TestsBase
	{
		[Fact]
		public virtual void ExecutionOfCodeFromDifferentThreads()
		{
			// Arrange
			const string variableName = "foo";
			string inputCode1 = string.Format("var {0} = 'bar';", variableName);
			string inputCode2 = string.Format("{0} = 'baz';", variableName);
			const string targetOutput = "baz";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(inputCode1);

				var thread = new Thread(() => jsEngine.Execute(inputCode2));
				thread.Start();
				thread.Join();

				output = jsEngine.GetVariableValue<string>(variableName);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}
	}
}