using System;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Helpers;

namespace JavaScriptEngineSwitcher.Node.Helpers
{
	/// <summary>
	/// JS error helpers
	/// </summary>
	internal static class NodeJsErrorHelpers
	{
		#region Error location

		/// <summary>
		/// Name of file, which identifies the generated function call
		/// </summary>
		private const string GeneratedFunctionCallDocumentName = "JavaScriptEngineSwitcher.Node.Resources.generated-function-call.js";

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
			return JsErrorHelpers.ParseErrorLocation(errorLocation, MapErrorLocationItem);
		}

		private static ErrorLocationItem MapErrorLocationItem(string errorLocationLine)
		{
			ErrorLocationItem item = null;
			Match lineMatch = _errorLocationLineRegex.Match(errorLocationLine);

			if (lineMatch.Success)
			{
				GroupCollection lineGroups = lineMatch.Groups;
				Group lineNumberGroup = lineGroups["lineNumber"];
				Group columnNumberGroup = lineGroups["columnNumber"];

				item = new ErrorLocationItem
				{
					FunctionName = lineGroups["functionName"].Value,
					DocumentName = lineGroups["documentName"].Value,
					LineNumber = lineNumberGroup.Success ? int.Parse(lineNumberGroup.Value) : 0,
					ColumnNumber = columnNumberGroup.Success ? int.Parse(columnNumberGroup.Value) : 0,
					SourceFragment = lineGroups["sourceFragment"].Value
				};
			}


			return item;
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

		/// <summary>
		/// Filters a error location items
		/// </summary>
		/// <param name="errorLocationItems">An array of <see cref="ErrorLocationItem"/> instances</param>
		public static ErrorLocationItem[] FilterErrorLocationItems(ErrorLocationItem[] errorLocationItems)
		{
			int itemCount = errorLocationItems.Length;
			if (itemCount == 0)
			{
				return errorLocationItems;
			}

			int itemIndex = 0;

			while (itemIndex < itemCount)
			{
				ErrorLocationItem item = errorLocationItems[itemIndex];
				string documentName = item.DocumentName;
				string functionName = item.FunctionName;

				if (documentName == "node:vm"
					|| documentName == "vm.js"
					|| documentName == GeneratedFunctionCallDocumentName
					|| (documentName == "anonymous" && functionName == "callFunction"))
				{
					break;
				}

				itemIndex++;
			}

			if (itemIndex == itemCount)
			{
				return errorLocationItems;
			}

			var processedErrorLocationItems = new ErrorLocationItem[itemIndex];
			Array.Copy(errorLocationItems, processedErrorLocationItems, itemIndex);

			return processedErrorLocationItems;
		}

		#endregion
	}
}