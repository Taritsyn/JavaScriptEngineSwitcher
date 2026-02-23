using System;

namespace JavaScriptEngineSwitcher.Core.Utilities
{
	/// <summary>
	/// Shortcuts for accessing to values that depend on the current environment and platform
	/// </summary>
	internal static class EnvironmentShortcuts
	{
		/// <summary>
		/// Gets a array of the newline characters
		/// </summary>
		internal static readonly char[] NewLineChars = Environment.NewLine == "\r\n" ? ['\r', '\n'] : ['\n', '\r'];
	}
}