using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Helpers;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.NiL.Helpers
{
	/// <summary>
	/// JS error helpers
	/// </summary>
	internal static class NiLJsErrorHelpers
	{
		#region Error location

		private const string AtLinePrefix = "   at ";
		private const string DotNetStackTraceLinePrefix = AtLinePrefix + "NiL.JS.";

		private const string OriginalGlobalCode = "anonymous";
		private const string OriginalAnonymousFunctionName = "<anonymous method>";
		private const string WrapperGlobalCode = "Global code";
		private const string WrapperAnonymousFunctionName = "Anonymous function";

		/// <summary>
		/// Regular expression for working with line of the script error location
		/// </summary>
		private static readonly Regex _errorLocationLineRegex =
			new Regex(@"^" + AtLinePrefix +
				@"(?<functionName>" +
					@"[\w][\w ]*" +
					@"|" +
					CommonRegExps.JsFullNamePattern +
					@"|" +
					Regex.Escape(OriginalAnonymousFunctionName) +
				@")" +
				@"(?::line (?<lineNumber>\d+):(?<columnNumber>\d+))?$");


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

				// Completing parsing when a .NET stack trace is found
				if (line.StartsWith(DotNetStackTraceLinePrefix, StringComparison.Ordinal))
				{
					break;
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
						LineNumber = lineNumberGroup.Success ? int.Parse(lineNumberGroup.Value) : 0,
						ColumnNumber = columnNumberGroup.Success ? int.Parse(columnNumberGroup.Value) : 0,
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
		/// Fixes a error location items
		/// </summary>
		/// <param name="errorLocationItems">An array of <see cref="ErrorLocationItem"/> instances</param>
		public static void FixErrorLocationItems(ErrorLocationItem[] errorLocationItems)
		{
			foreach (ErrorLocationItem errorLocationItem in errorLocationItems)
			{
				string functionName = errorLocationItem.FunctionName;
				if (functionName.Length == 0 || functionName == OriginalGlobalCode)
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