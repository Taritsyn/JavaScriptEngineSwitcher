using System;
using System.IO;
using System.Text;

namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public sealed class FileManager
	{
		public string ReadFile(string path)
		{
			return ReadFile(path, null);
		}

		public string ReadFile(string path, Encoding encoding)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			encoding = encoding ?? Encoding.UTF8;

			string content = File.ReadAllText(path, encoding);

			return content;
		}
	}
}