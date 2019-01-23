namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public interface ILogger
	{
		void Error(string category, string message,
			string filePath = "", int lineNumber = 0, int columnNumber = 0,
			string sourceFragment = "");

		void Warn(string category, string message,
			string filePath = "", int lineNumber = 0, int columnNumber = 0,
			string sourceFragment = "");

		void Debug(string category, string message, string filePath = "");

		void Info(string category, string message, string filePath = "");
	}
}