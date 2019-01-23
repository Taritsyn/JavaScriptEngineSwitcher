namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public sealed class NullLogger : ILogger
	{
		public void Error(string category, string message,
			string filePath = "", int lineNumber = 0, int columnNumber = 0,
			string sourceFragment = "")
		{ }

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
			return "[null logger]";
		}
	}
}