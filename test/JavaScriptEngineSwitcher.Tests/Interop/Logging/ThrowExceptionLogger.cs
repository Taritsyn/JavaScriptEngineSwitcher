using System;
using System.Text;

namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public sealed class ThrowExceptionLogger : ILogger
	{
		public void Error(string category, string message, string filePath = "",
			int lineNumber = 0, int columnNumber = 0, string sourceFragment = "")
		{
			var errorBuilder = new StringBuilder();
			errorBuilder.AppendLine("Category: " + category);
			errorBuilder.AppendLine("Message: " + message);

			if (!string.IsNullOrWhiteSpace(filePath))
			{
				errorBuilder.AppendLine("File: " + filePath);
			}

			if (lineNumber > 0)
			{
				errorBuilder.AppendLine("Line number: " + lineNumber);
			}

			if (columnNumber > 0)
			{
				errorBuilder.AppendLine("Column number: " + columnNumber);
			}

			if (!string.IsNullOrWhiteSpace(sourceFragment))
			{
				errorBuilder.AppendLine("Source fragment:" + Environment.NewLine + sourceFragment);
			}

			string errorMessage = errorBuilder.ToString();
			errorBuilder.Clear();

			throw new InvalidOperationException(errorMessage);
		}

		public void Warn(string category, string message,
			string filePath = "", int lineNumber = 0, int columnNumber = 0,
			string sourceFragment = "")
		{ }

		public void Debug(string category, string message, string filePath = "")
		{ }

		public void Info(string category, string message, string filePath = "")
		{ }

		public override string ToString()
		{
			return "[throw exception logger]";
		}
	}
}