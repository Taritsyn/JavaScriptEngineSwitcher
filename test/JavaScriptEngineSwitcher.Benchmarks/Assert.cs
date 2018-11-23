using System;
using System.Text;

namespace JavaScriptEngineSwitcher.Benchmarks
{
	internal static class Assert
	{
		public static void Equal(string expected, string actual)
		{
			if (actual != expected)
			{
				var messageBuilder = new StringBuilder();
				messageBuilder.AppendLine("Assert.Equal() Failure");
				messageBuilder.AppendLine();
				messageBuilder.AppendLine($"Expected: {expected}");
				messageBuilder.Append($"Actual:   {actual}");

				string errorMessage = messageBuilder.ToString();

				throw new InvalidOperationException(errorMessage);
			}
		}
	}
}