using System;
using System.Text;

using Xunit;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class Es2015TestsBase : TestsBase
	{
		#region Promises

		[Fact]
		public virtual void ExecutionOfPromisesIsCorrect()
		{
			// Arrange
			var stringBuilder = new StringBuilder();
			const string input = @"var successfulWork = new Promise(function(resolve, reject) {
	resolve(""Success!"");
});

var unsuccessfulWork = new Promise(function (resolve, reject) {
	reject(""Fail!"");
});

function resolveCallback(result) {
	stringBuilder.AppendLine('Resolved: ' + result);
}

function rejectCallback(reason) {
	stringBuilder.AppendLine('Rejected: ' + reason);
}

successfulWork.then(resolveCallback, rejectCallback);
unsuccessfulWork.then(resolveCallback, rejectCallback);";
			string targetOutput = "Resolved: Success!" + Environment.NewLine +
				"Rejected: Fail!" + Environment.NewLine
				;

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("stringBuilder", stringBuilder);
				jsEngine.Execute(input);

				output = stringBuilder.ToString();
				stringBuilder.Clear();
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion
	}
}