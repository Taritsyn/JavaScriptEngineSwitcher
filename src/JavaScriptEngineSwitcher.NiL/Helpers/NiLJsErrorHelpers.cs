using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Helpers;

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
		/// Removes a .NET stack trace from string representation of the script error location
		/// </summary>
		/// <param name="errorLocation">String representation of the script error location</param>
		/// <returns>String representation of the script error location without .NET stack trace</returns>
		public static string RemoveDotNetStackTraceFromErrorLocation(string errorLocation)
		{
			if (string.IsNullOrWhiteSpace(errorLocation))
			{
				return string.Empty;
			}

			string jsErrorLocation = errorLocation;
			int dotNetStackTraceIndex = errorLocation.IndexOf(DotNetStackTraceLinePrefix);

			if (dotNetStackTraceIndex != -1)
			{
				jsErrorLocation = errorLocation.Substring(0, dotNetStackTraceIndex);
			}

			return jsErrorLocation;
		}

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
					LineNumber = lineNumberGroup.Success ? int.Parse(lineNumberGroup.Value) : 0,
					ColumnNumber = columnNumberGroup.Success ? int.Parse(columnNumberGroup.Value) : 0,
				};
			}

			return item;
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