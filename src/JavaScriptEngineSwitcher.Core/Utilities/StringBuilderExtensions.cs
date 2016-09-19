using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JavaScriptEngineSwitcher.Core.Utilities
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
				throw new ArgumentNullException("source");
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
				throw new ArgumentNullException("source");
			}

			if (_formatPlaceholderRegExp.IsMatch(format))
			{
				return source.AppendFormat(format, args).AppendLine();
			}

			return source.AppendLine(format.Replace("{{", "{").Replace("}}", "}"));
		}
	}
}