using Xunit;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class IntlTestsBase : TestsBase
	{
		[Fact]
		public virtual void DateTimeFormatConstructorIsSupported()
		{
			// Arrange
			const string functionCode = @"function formatDate(value, locale) {
	if (typeof value === 'string' && value.length > 0) {
		value = new Date(value);
	}

	return new Intl.DateTimeFormat(locale).format(value);
}";
			const string targetOutput = "16.09.2021";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(functionCode);
				output = jsEngine.CallFunction<string>("formatDate", "2021-09-16", "ru-ru");
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}
	}
}