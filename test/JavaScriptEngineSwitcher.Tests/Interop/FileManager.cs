namespace JavaScriptEngineSwitcher.Tests.Interop
{
	using System;
	using System.IO;

	public sealed class FileManager
	{
		public string ReadFile(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			string content = File.ReadAllText(path);

			return content;
		}
	}
}