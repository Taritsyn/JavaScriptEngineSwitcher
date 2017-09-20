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
			var sb = new StringBuilder();
			const string input = @"var successfulWork = new Promise(function(resolve, reject) {
	resolve(""Resolved promise from JavaScript"");
});

var unsuccessfulWork = new Promise(function (resolve, reject) {
	reject(""Rejected promise from JavaScript"");
});

function resolveCallback(result) {
	console.AppendLine('Resolved: ' + result);
}

function rejectCallback(reason) {
	console.AppendLine('Rejected: ' + reason);
}

successfulWork.then(resolveCallback, rejectCallback);
unsuccessfulWork.then(resolveCallback, rejectCallback);";

			// Act
			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("sb", sb);
				jsEngine.Execute(input);
			}

			// Assert
		}

		#endregion
	}
}
