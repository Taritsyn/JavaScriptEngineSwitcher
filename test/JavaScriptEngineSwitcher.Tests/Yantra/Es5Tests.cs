using Xunit;

namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}


		#region Array methods

		[Fact]
		public override void SupportsArrayIndexOfMethod()
		{
			// Arrange
			const string initCode = "var arr = [2, 5, 9, 2]";

			const string input1 = "arr.indexOf(2);";
			const int targetOutput1 = 0;

			const string input2 = "arr.indexOf(7);";
			const int targetOutput2 = -1;

			const string input3 = "arr.indexOf(2, 3)";
			const int targetOutput3 = 3;

			const string input4 = "arr.indexOf(2, 2);";
			const int targetOutput4 = 3;

			const string input5 = "arr.indexOf(2, -2);";
#if NET9_0_OR_GREATER
			const int targetOutput5 = 0;
#else
			const int targetOutput5 = 3;
#endif

			const string input6 = "arr.indexOf(2, -1);";
#if NET9_0_OR_GREATER
			const int targetOutput6 = 0;
#else
			const int targetOutput6 = 3;
#endif

			const string input7 = "[].lastIndexOf(2, 0);";
			const int targetOutput7 = -1;

			// Act
			int output1;
			int output2;
			int output3;
			int output4;
			int output5;
			int output6;
			int output7;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
				output4 = jsEngine.Evaluate<int>(input4);
				output5 = jsEngine.Evaluate<int>(input5);
				output6 = jsEngine.Evaluate<int>(input6);
				output7 = jsEngine.Evaluate<int>(input7);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
			Assert.Equal(targetOutput5, output5);
			Assert.Equal(targetOutput6, output6);
			Assert.Equal(targetOutput7, output7);
		}

		[Fact]
		public override void SupportsArrayLastIndexOfMethod()
		{
			// Arrange
			const string initCode = "var arr = [2, 5, 9, 2]";

			const string input1 = "arr.lastIndexOf(2);";
			const int targetOutput1 = 3;

			const string input2 = "arr.lastIndexOf(7);";
			const int targetOutput2 = -1;

			const string input3 = "arr.lastIndexOf(2, 3)";
			const int targetOutput3 = 3;

			const string input4 = "arr.lastIndexOf(2, 2);";
			const int targetOutput4 = 0;

			const string input5 = "arr.lastIndexOf(2, -2);";
			const int targetOutput5 = 0;

			const string input6 = "arr.lastIndexOf(2, -1);";
#if NET9_0_OR_GREATER
			const int targetOutput6 = 0;
#else
			const int targetOutput6 = 3;
#endif

			const string input7 = "[].lastIndexOf(2, 0);";
			const int targetOutput7 = -1;

			// Act
			int output1;
			int output2;
			int output3;
			int output4;
			int output5;
			int output6;
			int output7;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
				output4 = jsEngine.Evaluate<int>(input4);
				output5 = jsEngine.Evaluate<int>(input5);
				output6 = jsEngine.Evaluate<int>(input6);
				output7 = jsEngine.Evaluate<int>(input7);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
			Assert.Equal(targetOutput5, output5);
			Assert.Equal(targetOutput6, output6);
			Assert.Equal(targetOutput7, output7);
		}

		#endregion

	}
}