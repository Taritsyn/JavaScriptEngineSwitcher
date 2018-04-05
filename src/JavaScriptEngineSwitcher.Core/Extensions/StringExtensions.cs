using System;

namespace JavaScriptEngineSwitcher.Core.Extensions
{
	/// <summary>
	/// Extensions for String
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Returns a value indicating whether the specified quoted string occurs within this string
		/// </summary>
		/// <param name="source">Instance of <see cref="String"/></param>
		/// <param name="value">The string without quotes to seek</param>
		/// <returns>true if the quoted value occurs within this string; otherwise, false</returns>
		public static bool ContainsQuotedValue(this string source, string value)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			bool result = source.Contains("'" + value + "'") || source.Contains("\"" + value + "\"");

			return result;
		}

		/// <summary>
		/// Removes leading occurrence of the specified string from the current <see cref="String"/> object
		/// </summary>
		/// <param name="source">Instance of <see cref="String"/></param>
		/// <param name="trimString">An string to remove</param>
		/// <returns>The string that remains after removing of the specified string from the start of
		/// the current string</returns>
		public static string TrimStart(this string source, string trimString)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (trimString == null)
			{
				throw new ArgumentNullException(nameof(trimString));
			}

			if (source.Length == 0 || trimString.Length == 0)
			{
				return source;
			}

			string result = source;
			if (source.StartsWith(trimString, StringComparison.Ordinal))
			{
				result = source.Substring(trimString.Length);
			}

			return result;
		}

		/// <summary>
		/// Splits a string into lines
		/// </summary>
		/// <param name="source">Instance of <see cref="String"/></param>
		/// <returns>An array of lines</returns>
		public static string[] SplitToLines(this string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			string[] result = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

			return result;
		}
	}
}