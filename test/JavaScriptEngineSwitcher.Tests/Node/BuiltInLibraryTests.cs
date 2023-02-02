#if !NET452
using Xunit;

using JavaScriptEngineSwitcher.Node;

namespace JavaScriptEngineSwitcher.Tests.Node
{
	public class BuiltinLibraryTests : TestsBase
	{
		protected override string EngineName
		{
			get { return "NodeJsEngine"; }
		}


		[Fact]
		public void AccessingToRequireFunction()
		{
			// Arrange
			var withoutBuiltinLibrary = new NodeSettings { UseBuiltinLibrary = false };
			var withBuiltinLibrary = new NodeSettings { UseBuiltinLibrary = true };

			const string input = @"typeof require !== 'undefined';";

			// Act
			bool output1 = false;
			bool output2 = false;

			using (var jsEngine = new NodeJsEngine(withoutBuiltinLibrary))
			{
				output1 = jsEngine.Evaluate<bool>(input);
			}

			using (var jsEngine = new NodeJsEngine(withBuiltinLibrary))
			{
				output2 = jsEngine.Evaluate<bool>(input);
			}

			// Assert
			Assert.False(output1);
			Assert.True(output2);
		}

		[Fact]
		public void ReadingOfFile()
		{
			// Arrange
			const string input = @"let fs = require('fs');
fs.readFileSync('Files/link.txt', 'utf8')";
			const string targetOutput = "http://www.panopticoncentral.net/2015/09/09/the-two-faces-of-jsrt-in-windows-10/";

			// Act
			string output = string.Empty;

			using (var jsEngine = new NodeJsEngine(new NodeSettings { UseBuiltinLibrary = true }))
			{
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}
	}
}
#endif