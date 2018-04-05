using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JavaScriptEngineSwitcher.Core.Extensions
{
	/// <summary>
	/// Extensions for StringBuilder
	/// </summary>
	internal static class StringBuilderExtensions
	{
		/// <summary>
		/// Regular expression for format placeholder
		/// </summary>
		private static readonly Regex _formatPlaceholderRegExp =
			new Regex(@"\{[0-9]\}", RegexOptions.Multiline);

		/// <summary>
		/// Appends the default line terminator to the end of the current <see cref="StringBuilder"/> instance
		/// </summary>
		/// <param name="source">Instance of <see cref="StringBuilder"/></param>
		/// <returns>Instance of <see cref="StringBuilder"/></returns>
		public static StringBuilder AppendFormatLine(this StringBuilder source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.AppendLine();
		}

		/// <summary>
		/// Appends the string returned by processing a composite format string, which
		/// contains zero or more format items, with default line terminator to this instance.
		/// Each format item is replaced by the string representation of a corresponding
		/// argument in a parameter array.
		/// </summary>
		/// <param name="source">Instance of <see cref="StringBuilder"/></param>
		/// <param name="format">A composite format string</param>
		/// <param name="args">An array of objects to format</param>
		/// <returns>Instance of <see cref="StringBuilder"/></returns>
		public static StringBuilder AppendFormatLine(this StringBuilder source, string format, params object[] args)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (_formatPlaceholderRegExp.IsMatch(format))
			{
				return source.AppendFormat(format, args).AppendLine();
			}

			return source.AppendLine(format.Replace("{{", "{").Replace("}}", "}"));
		}

		/// <summary>
		/// Removes the all trailing white-space characters from the current <see cref="StringBuilder"/> instance
		/// </summary>
		/// <param name="source">Instance of <see cref="StringBuilder"/></param>
		/// <returns>Instance of <see cref="StringBuilder"/> without trailing white-space characters</returns>
		public static StringBuilder TrimEnd(this StringBuilder source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			int charCount = source.Length;
			if (charCount == 0)
			{
				return source;
			}

			int charIndex = charCount - 1;

			for (; charIndex >= 0; charIndex--)
			{
				char charValue = source[charIndex];
				if (!char.IsWhiteSpace(charValue))
				{
					break;
				}
			}

			if (charIndex < source.Length - 1)
			{
				source.Length = charIndex + 1;
			}

			return source;
		}
	}
}