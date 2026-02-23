using System;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Helpers;

namespace JavaScriptEngineSwitcher.Yantra.Helpers
{
	/// <summary>
	/// JS error helpers
	/// </summary>
	internal static class YantraJsErrorHelpers
	{
		#region Error location

		private const string OriginalGlobalCode = "native";
		private const string OriginalAnonymousFunctionName = "inline";
		private const string WrapperGlobalCode = "Global code";
		private const string WrapperAnonymousFunctionName = "Anonymous function";

		/// <summary>
		/// Regular expression for working with line of the script error location
		/// </summary>
		private static readonly Regex _errorLocationLineRegex =
			new Regex(@"^[ ]{4}at " +
				@"(?<functionName>" +
					@"[\w][\w ]*" +
					@"|" +
					CommonRegExps.JsFullNamePattern +
				@")" +
				@":(?<documentName>" + CommonRegExps.DocumentNamePattern + @")" +
				@":(?<lineNumber>\d+),(?<columnNumber>\d+)$")
				;


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
				item = new ErrorLocationItem
				{
					FunctionName = lineGroups["functionName"].Value,
					DocumentName = lineGroups["documentName"].Value,
					LineNumber = int.Parse(lineGroups["lineNumber"].Value),
					ColumnNumber = int.Parse(lineGroups["columnNumber"].Value)
				};
			}

			return item;
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

				if (documentName.StartsWith("/home/runner/work/yantra/"))
				{
					break;
				}

				itemIndex++;
			}

			if (itemIndex == itemCount)
			{
				return errorLocationItems;
			}

			int firstSuitableItemIndex = itemIndex + 1;
			int suitableItemCount = itemCount - firstSuitableItemIndex;

			var processedErrorLocationItems = new ErrorLocationItem[suitableItemCount];
			Array.Copy(errorLocationItems, firstSuitableItemIndex, processedErrorLocationItems, 0, suitableItemCount);

			return processedErrorLocationItems;
		}

		/// <summary>
		/// Fixes a error location items
		/// </summary>
		/// <param name="errorLocationItems">An array of <see cref="ErrorLocationItem"/> instances</param>
		public static void FixErrorLocationItems(ErrorLocationItem[] errorLocationItems)
		{
			foreach (ErrorLocationItem errorLocationItem in errorLocationItems)
			{
				string functionName = errorLocationItem.FunctionName;
				if (functionName == OriginalGlobalCode)
				{
					errorLocationItem.FunctionName = WrapperGlobalCode;
				}
				else if (functionName == OriginalAnonymousFunctionName)
				{
					errorLocationItem.FunctionName = WrapperAnonymousFunctionName;
				}
			}
		}

		#endregion
	}
}