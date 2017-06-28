using Xunit;

using JavaScriptEngineSwitcher.Core.Helpers;

namespace JavaScriptEngineSwitcher.Tests
{
	public class ValidationTests
	{
		[Fact]
		public void NameFormatIsCorrect()
		{
			// Arrange

			// Act
			bool name1FormatIsCorrect = ValidationHelpers.CheckNameFormat("good_parts");
			bool name2FormatIsCorrect = ValidationHelpers.CheckNameFormat("i18n");
			bool name3FormatIsCorrect = ValidationHelpers.CheckNameFormat("fooBar");
			bool name4FormatIsCorrect = ValidationHelpers.CheckNameFormat("$grid");
			bool name5FormatIsCorrect = ValidationHelpers.CheckNameFormat("a");
			bool name6FormatIsCorrect = ValidationHelpers.CheckNameFormat("À_la_maison");

			// Assert
			Assert.True(name1FormatIsCorrect);
			Assert.True(name2FormatIsCorrect);
			Assert.True(name3FormatIsCorrect);
			Assert.True(name4FormatIsCorrect);
			Assert.True(name5FormatIsCorrect);
			Assert.True(name6FormatIsCorrect);
		}

		[Fact]
		public void NameFormatIsWrong()
		{
			// Arrange

			// Act
			bool name1FormatIsCorrect = ValidationHelpers.CheckNameFormat("good-parts");
			bool name2FormatIsCorrect = ValidationHelpers.CheckNameFormat("1sale");
			bool name3FormatIsCorrect = ValidationHelpers.CheckNameFormat("Foo Bar");
			bool name4FormatIsCorrect = ValidationHelpers.CheckNameFormat("@grid");
			bool name5FormatIsCorrect = ValidationHelpers.CheckNameFormat("2");

			// Assert
			Assert.False(name1FormatIsCorrect);
			Assert.False(name2FormatIsCorrect);
			Assert.False(name3FormatIsCorrect);
			Assert.False(name4FormatIsCorrect);
			Assert.False(name5FormatIsCorrect);
		}
	}
}