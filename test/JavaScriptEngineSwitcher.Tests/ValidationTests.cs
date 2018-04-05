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
			bool name1FormatIsWrong = ValidationHelpers.CheckNameFormat("good-parts");
			bool name2FormatIsWrong = ValidationHelpers.CheckNameFormat("1sale");
			bool name3FormatIsWrong = ValidationHelpers.CheckNameFormat("Foo Bar");
			bool name4FormatIsWrong = ValidationHelpers.CheckNameFormat("@grid");
			bool name5FormatIsWrong = ValidationHelpers.CheckNameFormat("2");

			// Assert
			Assert.False(name1FormatIsWrong);
			Assert.False(name2FormatIsWrong);
			Assert.False(name3FormatIsWrong);
			Assert.False(name4FormatIsWrong);
			Assert.False(name5FormatIsWrong);
		}

		[Fact]
		public void DocumentNameFormatIsCorrect()
		{
			// Arrange

			// Act
			bool documentName1FormatIsCorrect = ValidationHelpers.CheckDocumentNameFormat("Script Document");
			bool documentName2FormatIsCorrect = ValidationHelpers.CheckDocumentNameFormat("Script Document [2]");
			bool documentName3FormatIsCorrect = ValidationHelpers.CheckDocumentNameFormat("doc01.js");
			bool documentName4FormatIsCorrect = ValidationHelpers.CheckDocumentNameFormat("/res/scripts.min.js");
			bool documentName5FormatIsCorrect = ValidationHelpers.CheckDocumentNameFormat(
				@"C:\Users\Vasya\AppData\Roaming\npm\node_modules\typescript\lib\tsc.js");
			bool documentName6FormatIsCorrect = ValidationHelpers.CheckDocumentNameFormat(
				"BundleTransformer.Less.Resources.less-combined.min.js");

			// Assert
			Assert.True(documentName1FormatIsCorrect);
			Assert.True(documentName2FormatIsCorrect);
			Assert.True(documentName3FormatIsCorrect);
			Assert.True(documentName4FormatIsCorrect);
			Assert.True(documentName5FormatIsCorrect);
			Assert.True(documentName6FormatIsCorrect);
		}

		[Fact]
		public void DocumentNameFormatIsWrong()
		{
			// Arrange

			// Act
			bool documentName1FormatIsWrong = ValidationHelpers.CheckDocumentNameFormat("Script	Document");
			bool documentName2FormatIsWrong = ValidationHelpers.CheckDocumentNameFormat("Script Document <2>");
			bool documentName3FormatIsWrong = ValidationHelpers.CheckDocumentNameFormat(" doc01.js");
			bool documentName4FormatIsWrong = ValidationHelpers.CheckDocumentNameFormat(@"Document ""Test""");
			bool documentName5FormatIsWrong = ValidationHelpers.CheckDocumentNameFormat("src/*.js");
			bool documentName6FormatIsWrong = ValidationHelpers.CheckDocumentNameFormat(
				"/js/shared/SubScribeModal/subscribeChecker.js?v=2017-11-09");

			// Assert
			Assert.False(documentName1FormatIsWrong);
			Assert.False(documentName2FormatIsWrong);
			Assert.False(documentName3FormatIsWrong);
			Assert.False(documentName4FormatIsWrong);
			Assert.False(documentName5FormatIsWrong);
			Assert.False(documentName6FormatIsWrong);
		}
	}
}