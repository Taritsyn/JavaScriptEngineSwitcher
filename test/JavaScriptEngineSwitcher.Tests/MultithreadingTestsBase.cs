using System;
using System.IO;
using System.Threading;

using Xunit;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class MultithreadingTestsBase : TestsBase
	{
		[Fact]
		public virtual void ExecutionOfCodeFromDifferentThreadsIsCorrect()
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

		[Fact]
		public virtual void RecursiveExecutionOfFilesIsCorrect()
		{
			// Arrange
			const string variableName = "num";
			const int targetOutput = 12;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				Action<string> executeFile = path => jsEngine.ExecuteFile(path);

				jsEngine.EmbedHostObject("executeFile", executeFile);
				jsEngine.ExecuteFile("Files/recursiveExecution/mainFile.js");

				output = jsEngine.GetVariableValue<int>(variableName);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void RecursiveEvaluationOfFilesIsCorrect()
		{
			// Arrange
			const string input = "require('index').calculateResult();";
			const double targetOutput = 132.14;

			// Act
			double output;

			using (var jsEngine = CreateJsEngine())
			{
				Func<string, object> loadModule = name => {
					string path = Path.Combine("Files/recursiveEvaluation", $"{name}.js");
					string code = File.ReadAllText(path);
					object result = jsEngine.Evaluate(code, path);

					return result;
				};

				jsEngine.EmbedHostObject("require", loadModule);
				output = jsEngine.Evaluate<double>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}
	}
}