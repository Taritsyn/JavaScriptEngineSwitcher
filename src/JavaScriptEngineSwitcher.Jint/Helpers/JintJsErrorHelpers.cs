using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using AdvancedStringBuilder;

using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Helpers;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Jint.Helpers
{
	/// <summary>
	/// JS error helpers
	/// </summary>
	internal static class JintJsErrorHelpers
	{
		#region Error location

		private const string OriginalAnonymousFunctionName = "(anonymous)";
		private const string WrapperAnonymousFunctionName = "Anonymous function";
		private const string DelegateFunctionName = "delegate";

		/// <summary>
		/// Regular expression for working with line of the script error location
		/// </summary>
		private static readonly Regex _errorLocationLineRegex =
			new Regex(@"^[ ]{3}at " +
				@"(?:" +
					@"(?:" +
						@"(?<functionName>" +
							@"[\w][\w ]*" +
							@"|" +
							CommonRegExps.JsFullNamePattern +
							@"|" +
							Regex.Escape(OriginalAnonymousFunctionName) +
						@") " +
						@"\((?:" + CommonRegExps.JsFullNamePattern + @"(?:, " + CommonRegExps.JsFullNamePattern + @")*)?\)" +
						@"|" +
						@"(?<functionName>" + Regex.Escape(DelegateFunctionName) + @")" +
					@") " +
				@")?" +
				@"(?<documentName>" + CommonRegExps.DocumentNamePattern + @"):" +
				@"(?<lineNumber>\d+)(?::(?<columnNumber>\d+))?$");

		/// <summary>
		/// Regular expression for working with the syntax error message
		/// </summary>
		private static readonly Regex _syntaxErrorMessageRegex =
			new Regex(@"^(?<description>[\s\S]+?) \(" + CommonRegExps.DocumentNamePattern + @":\d+:\d+\)$");

		/// <summary>
		/// Regular expression for working with the script preparation error message
		/// </summary>
		private static readonly Regex _scriptPreparationErrorMessageRegex =
			new Regex(@"^Could not prepare script: (?<description>[\s\S]+?) " +
				@"\((?<documentName>" + CommonRegExps.DocumentNamePattern + @"):" +
				@"(?<lineNumber>\d+):(?<columnNumber>\d+)\)$");


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
				Match lineMatch = _errorLocationLineRegex.Match(line);

				if (lineMatch.Success)
				{
					GroupCollection lineGroups = lineMatch.Groups;

					var errorLocationItem = new ErrorLocationItem
					{
						FunctionName = lineGroups["functionName"].Value,
						DocumentName = lineGroups["documentName"].Value,
						LineNumber = int.Parse(lineGroups["lineNumber"].Value),
						ColumnNumber = lineGroups["columnNumber"].Success ?
							int.Parse(lineGroups["columnNumber"].Value) : 0
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
				if (functionName.Length == 0 || functionName == DelegateFunctionName)
				{
					errorLocationItem.FunctionName = "Global code";
				}
				else if (functionName == OriginalAnonymousFunctionName)
				{
					errorLocationItem.FunctionName = WrapperAnonymousFunctionName;
				}
			}
		}

		/// <summary>
		/// Converts a call chain to stack
		/// </summary>
		/// <param name="callChain">Call chain</param>
		/// <returns>Call stack</returns>
		public static string ConvertCallChainToStack(string callChain)
		{
			string callStack = string.Empty;
			string[] callChainItems = callChain
				.Split(new string[] { "->" }, StringSplitOptions.None)
				;

			if (callChainItems.Length > 0)
			{
				var stringBuilderPool = StringBuilderPool.Shared;
				StringBuilder stackBuilder = stringBuilderPool.Rent();

				for (int chainItemIndex = callChainItems.Length - 1; chainItemIndex >= 0; chainItemIndex--)
				{
					string chainItem = callChainItems[chainItemIndex];
					if (chainItem == OriginalAnonymousFunctionName)
					{
						chainItem = WrapperAnonymousFunctionName;
					}

					JsErrorHelpers.WriteErrorLocationLine(stackBuilder, chainItem, string.Empty, 0, 0);
					if (chainItemIndex > 0)
					{
						stackBuilder.AppendLine();
					}
				}

				callStack = stackBuilder.ToString();
				stringBuilderPool.Return(stackBuilder);
			}

			return callStack;
		}

		/// <summary>
		/// Gets a description from the syntax error message
		/// </summary>
		/// <param name="message">Error message</param>
		/// <returns>Description of error</returns>
		public static string GetDescriptionFromSyntaxErrorMessage(string message)
		{
			Match messageMatch = _syntaxErrorMessageRegex.Match(message);
			string description = messageMatch.Success ?
				messageMatch.Groups["description"].Value : message;

			return description;
		}

		/// <summary>
		/// Parses a script preparation error message
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="description">Description of error</param>
		/// <param name="errorLocation">Script error location</param>
		public static void ParseScriptPreparationErrorMessage(string message, out string description,
			out ErrorLocationItem errorLocation)
		{
			Match messageMatch = _scriptPreparationErrorMessageRegex.Match(message);

			if (messageMatch.Success)
			{
				GroupCollection messageGroups = messageMatch.Groups;

				description = messageGroups["description"].Value;
				errorLocation = new ErrorLocationItem
				{
					DocumentName = messageGroups["documentName"].Value,
					LineNumber = int.Parse(messageGroups["lineNumber"].Value),
					ColumnNumber = int.Parse(messageGroups["columnNumber"].Value)
				};
			}
			else
			{
				description = message;
				errorLocation = new ErrorLocationItem();
			}
		}

		#endregion
	}
}