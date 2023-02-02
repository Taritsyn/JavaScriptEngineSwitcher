using Xunit;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class Es2015TestsBase : TestsBase
	{
		#region Promises

		[Fact]
		public virtual void SupportsPromises()
		{
			// Arrange
			const string input = @"var output = '',
	successfulWork = new Promise(function(resolve, reject) {
		resolve('Success!');
	}),
	unsuccessfulWork = new Promise(function (resolve, reject) {
		reject('Fail!');
	})
	;

function resolveCallback(result) {
	output += 'Resolved: ' + result + '\n';
}

function rejectCallback(reason) {
	output += 'Rejected: ' + reason + '\n';
}

successfulWork.then(resolveCallback, rejectCallback);
unsuccessfulWork.then(resolveCallback, rejectCallback);";
			string targetOutput = "Resolved: Success!\n" +
				"Rejected: Fail!\n"
				;

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(input);
				output = jsEngine.GetVariableValue<string>("output");
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion
	}
}