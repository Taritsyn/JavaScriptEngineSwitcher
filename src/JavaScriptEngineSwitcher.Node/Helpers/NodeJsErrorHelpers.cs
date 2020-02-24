using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Helpers;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Node.Helpers
{
	/// <summary>
	/// JS error helpers
	/// </summary>
	internal static class NodeJsErrorHelpers
	{
		#region Error location

		/// <summary>
		/// Pattern for working with document names with coordinates
		/// </summary>
		private static readonly string DocumentNameWithCoordinatesPattern =
			@"(?<documentName>" + CommonRegExps.DocumentNamePattern + @"|<anonymous>|\[eval\])" +
			@"(?::(?<lineNumber>\d+)(?::(?<columnNumber>\d+))?)?";

		/// <summary>
		/// Pattern for working with JS function names
		/// </summary>
		private static readonly string JsFunctionNamePattern = CommonRegExps.JsNamePattern +
			@"(?:\.(?:" + CommonRegExps.JsNamePattern + @"|<anonymous>))*";

		/// <summary>
		/// Regular expression for working with line of the script error location
		/// </summary>
		private static readonly Regex _errorLocationLineRegex =
			new Regex(@"^[ ]{3,4}at " +
				@"(?:" +
					@"(?<functionName>[\w][\w ]*|" + JsFunctionNamePattern + @") " +
					@"\(" + DocumentNameWithCoordinatesPattern + @"\)" +
					@"|" +
					DocumentNameWithCoordinatesPattern +
				@")" +
				@"(?: -> (?<sourceFragment>[^\n\r]+))?$");


		/// <summary>
		/// Parses a string representation of the script error location to produce an array of
		/// <see cref="ErrorLocationItem"/> instances
		/// </summary>
		/// <param name="errorLocation">String representation of the script error location</param>
		/// <returns>An array of <see cref="ErrorLocationItem"/> instances</returns>
		public static ErrorLocationItem[] ParseErrorLocation(string errorLocation)
		{
			if (string.IsNullOrWhiteSpace(errorLocation))
			{
				return new ErrorLocationItem[0];
			}

			var errorLocationItems = new List<ErrorLocationItem>();
			string[] lines = errorLocation.SplitToLines();
			int lineCount = lines.Length;

			for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
			{
				string line = lines[lineIndex];
				if (line.Length == 0)
				{
					continue;
				}

				Match lineMatch = _errorLocationLineRegex.Match(line);

				if (lineMatch.Success)
				{
					GroupCollection lineGroups = lineMatch.Groups;
					Group lineNumberGroup = lineGroups["lineNumber"];
					Group columnNumberGroup = lineGroups["columnNumber"];

					var errorLocationItem = new ErrorLocationItem
					{
						FunctionName = lineGroups["functionName"].Value,
						DocumentName = lineGroups["documentName"].Value,
						LineNumber = lineNumberGroup.Success ? int.Parse(lineNumberGroup.Value) : 0,
						ColumnNumber = columnNumberGroup.Success ? int.Parse(columnNumberGroup.Value) : 0,
						SourceFragment = lineGroups["sourceFragment"].Value
					};
					errorLocationItems.Add(errorLocationItem);
				}
				else
				{
					Debug.WriteLine(string.Format(CoreStrings.Runtime_InvalidErrorLocationLineFormat, line));
					return new ErrorLocationItem[0];
				}
			}

			return errorLocationItems.ToArray();
		}

		/// <summary>
		/// Gets a column count from the text line
		/// </summary>
		/// <param name="textLine">Content of the text line</param>
		/// <returns>Column count from the text line</returns>
		public static int GetColumnCountFromLine(string textLine)
		{
			if (string.IsNullOrEmpty(textLine))
			{
				return 0;
			}

			if (textLine.IndexOf('\t') == -1)
			{
				return textLine.Length;
			}

			int charCount = textLine.Length;
			int columnCount = 0;

			for (int charIndex = 0; charIndex < charCount; charIndex++)
			{
				char charValue = textLine[charIndex];
				int increment = charValue == '\t' ? 4 : 1;

				columnCount += increment;
			}

			return columnCount;
		}

		#endregion
	}
}