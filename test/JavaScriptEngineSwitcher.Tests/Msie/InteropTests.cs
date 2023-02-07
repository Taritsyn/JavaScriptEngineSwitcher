using System;
using System.IO;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.Msie
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "MsieJsEngine"; }
		}


		#region Embedding of objects

		#region Recursive calls

		#region Mapping of errors

		[Fact]
		public void MappingRuntimeErrorDuringRecursiveEvaluationOfFiles()
		{
			// Arrange
			const string directoryPath = "Files/recursive-evaluation/runtime-error";
			const string input = "evaluateFile('index').calculateResult();";

			// Act
			JsRuntimeException exception = null;

			using (var jsEngine = CreateJsEngine())
			{
				try
				{
					Func<string, object> evaluateFile = path => {
						string absolutePath = Path.Combine(directoryPath, $"{path}.js");
						string code = File.ReadAllText(absolutePath);
						object result = jsEngine.Evaluate(code, absolutePath);

						return result;
					};

					jsEngine.EmbedHostObject("evaluateFile", evaluateFile);
					double output = jsEngine.Evaluate<double>(input);
				}
				catch (JsRuntimeException e)
				{
					exception = e;
				}
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("'argumens' is undefined", exception.Description);
			Assert.Equal("ReferenceError", exception.Type);
			Assert.Equal("math.js", exception.DocumentName);
			Assert.Equal(10, exception.LineNumber);
			Assert.Equal(4, exception.ColumnNumber);
			Assert.Empty(exception.SourceFragment);
			Assert.Equal(
				"   at sum (math.js:10:4)" + Environment.NewLine +
				"   at calculateResult (index.js:7:4)" + Environment.NewLine +
				"   at Global code (Script Document:1:1)",
				exception.CallStack
			);
		}

		#endregion

		#endregion

		#endregion
	}
}