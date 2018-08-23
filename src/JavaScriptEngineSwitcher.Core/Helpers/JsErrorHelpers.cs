using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Resources;
using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.Core.Helpers
{
	/// <summary>
	/// JS error helpers
	/// </summary>
	public static class JsErrorHelpers
	{
		#region Error location

		/// <summary>
		/// Pattern for working with document names with coordinates
		/// </summary>
		private static readonly string DocumentNameWithCoordinatesPattern =
			@"(?<documentName>" + CommonRegExps.DocumentNamePattern + @"):" +
			@"(?<lineNumber>\d+)(?::(?<columnNumber>\d+))?";

		/// <summary>
		/// Regular expression for working with line of the script error location
		/// </summary>
		private static readonly Regex _errorLocationLineRegex =
			new Regex(@"^[ ]{3,4}at " +
				@"(?:" +
					@"(?<functionName>[\w][\w ]*|" + CommonRegExps.JsFullNamePattern + @") " +
					@"\(" + DocumentNameWithCoordinatesPattern + @"\)" +
					@"|" +
					DocumentNameWithCoordinatesPattern +
				@")" +
				@"(?: -> (?<sourceFragment>[^\n\r]+))?$");

		/// <summary>
		/// Regular expression for working with a line break
		/// </summary>
		private static readonly Regex _lineBreakRegex = new Regex("\r\n|\n|\r");

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
							int.Parse(lineGroups["columnNumber"].Value) : 0,
						SourceFragment = lineGroups["sourceFragment"].Value
					};
					errorLocationItems.Add(errorLocationItem);
				}
				else
				{
					Debug.WriteLine(string.Format(Strings.Runtime_InvalidErrorLocationLineFormat, line));
					return new ErrorLocationItem[0];
				}
			}

			return errorLocationItems.ToArray();
		}

		/// <summary>
		/// Produces a string representation of the script error location from array of
		/// <see cref="ErrorLocationItem"/> instances
		/// </summary>
		/// <param name="errorLocationItems">An array of <see cref="ErrorLocationItem"/> instances</param>
		/// <param name="omitSourceFragment">Flag for whether to omit source fragment</param>
		/// <returns>String representation of the script error location</returns>
		public static string StringifyErrorLocationItems(ErrorLocationItem[] errorLocationItems,
			bool omitSourceFragment = false)
		{
			if (errorLocationItems == null)
			{
				throw new ArgumentException(nameof(errorLocationItems));
			}

			int locationItemCount = errorLocationItems.Length;
			if (locationItemCount == 0)
			{
				return string.Empty;
			}

			StringBuilder locationBuilder = StringBuilderPool.GetBuilder();

			for (int locationItemIndex = 0; locationItemIndex < locationItemCount; locationItemIndex++)
			{
				ErrorLocationItem locationItem = errorLocationItems[locationItemIndex];

				if (locationItemIndex > 0)
				{
					locationBuilder.AppendLine();
				}
				WriteErrorLocationLine(locationBuilder, locationItem.FunctionName, locationItem.DocumentName,
					locationItem.LineNumber, locationItem.ColumnNumber,
					!omitSourceFragment ? locationItem.SourceFragment : string.Empty);
			}

			string errorLocation = locationBuilder.ToString();
			StringBuilderPool.ReleaseBuilder(locationBuilder);

			return errorLocation;
		}

		/// <summary>
		/// Writes a error location line to the buffer
		/// </summary>
		/// <param name="buffer">Instance of <see cref="StringBuilder"/></param>
		/// <param name="functionName">Function name</param>
		/// <param name="documentName">Document name</param>
		/// <param name="lineNumber">Line number</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="sourceFragment">Source fragment</param>
		public static void WriteErrorLocationLine(StringBuilder buffer, string functionName,
			string documentName, int lineNumber, int columnNumber, string sourceFragment = "")
		{
			bool functionNameNotEmpty = !string.IsNullOrWhiteSpace(functionName);
			bool documentNameNotEmpty = !string.IsNullOrWhiteSpace(documentName);

			if (functionNameNotEmpty || documentNameNotEmpty || lineNumber > 0)
			{
				buffer.Append("   at ");
				if (functionNameNotEmpty)
				{
					buffer.Append(functionName);
				}
				if (documentNameNotEmpty || lineNumber > 0)
				{
					if (functionNameNotEmpty)
					{
						buffer.Append(" (");
					}
					if (documentNameNotEmpty)
					{
						buffer.Append(documentName);
					}
					if (lineNumber > 0)
					{
						if (documentNameNotEmpty)
						{
							buffer.Append(":");
						}
						buffer.Append(lineNumber);
						if (columnNumber > 0)
						{
							buffer.Append(":");
							buffer.Append(columnNumber);
						}
					}
					if (functionNameNotEmpty)
					{
						buffer.Append(")");
					}
					if (!string.IsNullOrWhiteSpace(sourceFragment))
					{
						buffer.Append(" -> ");
						buffer.Append(sourceFragment);
					}
				}
			}
		}

		#endregion

		#region Generation of error messages

		/// <summary>
		/// Generates a engine load error message
		/// </summary>
		/// <param name="description">Description of error</param>
		/// <param name="engineName">Name of JS engine</param>
		/// <param name="quoteDescription">Makes a quote from the description</param>
		/// <returns>Engine load error message</returns>
		public static string GenerateEngineLoadErrorMessage(string description, string engineName,
			bool quoteDescription = false)
		{
			if (engineName == null)
			{
				throw new ArgumentNullException(nameof(engineName));
			}

			if (string.IsNullOrWhiteSpace(engineName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(engineName)),
					nameof(engineName)
				);
			}

			string jsEngineNotLoadedPart = string.Format(Strings.Engine_JsEngineNotLoaded, engineName);
			string message;

			if (!string.IsNullOrWhiteSpace(description))
			{
				StringBuilder messageBuilder = StringBuilderPool.GetBuilder();
				messageBuilder.Append(jsEngineNotLoadedPart);
				messageBuilder.Append(" ");
				if (quoteDescription)
				{
					messageBuilder.AppendFormat(Strings.Common_SeeOriginalErrorMessage, description);
				}
				else
				{
					messageBuilder.Append(description);
				}

				message = messageBuilder.ToString();
				StringBuilderPool.ReleaseBuilder(messageBuilder);
			}
			else
			{
				message = jsEngineNotLoadedPart;
			}

			return message;
		}

		/// <summary>
		/// Generates a script error message
		/// </summary>
		/// <param name="type">Type of the script error</param>
		/// <param name="description">Description of error</param>
		/// <param name="documentName">Document name</param>
		/// <param name="lineNumber">Line number</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="sourceFragment">Source fragment</param>
		/// <returns>Script error message</returns>
		public static string GenerateScriptErrorMessage(string type, string description,
			string documentName, int lineNumber, int columnNumber, string sourceFragment = "")
		{
			return GenerateScriptErrorMessage(type, description, documentName, lineNumber, columnNumber,
				sourceFragment, string.Empty);
		}

		/// <summary>
		/// Generates a script error message
		/// </summary>
		/// <param name="type">Type of the script error</param>
		/// <param name="description">Description of error</param>
		/// <param name="callStack">String representation of the script call stack</param>
		/// <returns>Script error message</returns>
		public static string GenerateScriptErrorMessage(string type, string description, string callStack)
		{
			return GenerateScriptErrorMessage(type, description, string.Empty, 0, 0, string.Empty, callStack);
		}

		/// <summary>
		/// Generates a script error message
		/// </summary>
		/// <param name="type">Type of the script error</param>
		/// <param name="description">Description of error</param>
		/// <param name="documentName">Document name</param>
		/// <param name="lineNumber">Line number</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="sourceFragment">Source fragment</param>
		/// <param name="callStack">String representation of the script call stack</param>
		/// <returns>Script error message</returns>
		private static string GenerateScriptErrorMessage(string type, string description, string documentName,
			int lineNumber, int columnNumber, string sourceFragment, string callStack)
		{
			if (description == null)
			{
				throw new ArgumentNullException(nameof(description));
			}

			if (string.IsNullOrWhiteSpace(description))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(description)),
					nameof(description)
				);
			}

			StringBuilder messageBuilder = StringBuilderPool.GetBuilder();
			if (!string.IsNullOrWhiteSpace(type))
			{
				messageBuilder.Append(type);
				messageBuilder.Append(": ");
			}
			messageBuilder.Append(description);

			if (!string.IsNullOrWhiteSpace(callStack))
			{
				messageBuilder.AppendLine();
				messageBuilder.Append(callStack);
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(documentName) || lineNumber > 0)
				{
					messageBuilder.AppendLine();
					WriteErrorLocationLine(messageBuilder, string.Empty, documentName, lineNumber, columnNumber,
						sourceFragment);
				}
			}

			string errorMessage = messageBuilder.ToString();
			StringBuilderPool.ReleaseBuilder(messageBuilder);

			return errorMessage;
		}

		#endregion

		#region Generation of error details

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsException">JS exception</param>
		/// <param name="omitMessage">Flag for whether to omit message</param>
		/// <returns>Detailed error message</returns>
		public static string GenerateErrorDetails(JsException jsException, bool omitMessage = false)
		{
			if (jsException == null)
			{
				throw new ArgumentNullException(nameof(jsException));
			}

			StringBuilder detailsBuilder = StringBuilderPool.GetBuilder();
			WriteCommonErrorDetails(detailsBuilder, jsException, omitMessage);

			var jsScriptException = jsException as JsScriptException;
			if (jsScriptException != null)
			{
				WriteScriptErrorDetails(detailsBuilder, jsScriptException);

				var jsRuntimeException = jsScriptException as JsRuntimeException;
				if (jsRuntimeException != null)
				{
					WriteRuntimeErrorDetails(detailsBuilder, jsRuntimeException);
				}
			}

			detailsBuilder.TrimEnd();

			string errorDetails = detailsBuilder.ToString();
			StringBuilderPool.ReleaseBuilder(detailsBuilder);

			return errorDetails;
		}

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsScriptException">JS script exception</param>
		/// <param name="omitMessage">Flag for whether to omit message</param>
		/// <returns>Detailed error message</returns>
		public static string GenerateErrorDetails(JsScriptException jsScriptException,
			bool omitMessage = false)
		{
			if (jsScriptException == null)
			{
				throw new ArgumentNullException(nameof(jsScriptException));
			}

			StringBuilder detailsBuilder = StringBuilderPool.GetBuilder();
			WriteCommonErrorDetails(detailsBuilder, jsScriptException, omitMessage);
			WriteScriptErrorDetails(detailsBuilder, jsScriptException);

			var jsRuntimeException = jsScriptException as JsRuntimeException;
			if (jsRuntimeException != null)
			{
				WriteRuntimeErrorDetails(detailsBuilder, jsRuntimeException);
			}

			detailsBuilder.TrimEnd();

			string errorDetails = detailsBuilder.ToString();
			StringBuilderPool.ReleaseBuilder(detailsBuilder);

			return errorDetails;
		}

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsRuntimeException">JS runtime exception</param>
		/// <param name="omitMessage">Flag for whether to omit message</param>
		/// <returns>Detailed error message</returns>
		public static string GenerateErrorDetails(JsRuntimeException jsRuntimeException,
			bool omitMessage = false)
		{
			if (jsRuntimeException == null)
			{
				throw new ArgumentNullException(nameof(jsRuntimeException));
			}

			StringBuilder detailsBuilder = StringBuilderPool.GetBuilder();
			WriteCommonErrorDetails(detailsBuilder, jsRuntimeException, omitMessage);
			WriteScriptErrorDetails(detailsBuilder, jsRuntimeException);
			WriteRuntimeErrorDetails(detailsBuilder, jsRuntimeException);

			detailsBuilder.TrimEnd();

			string errorDetails = detailsBuilder.ToString();
			StringBuilderPool.ReleaseBuilder(detailsBuilder);

			return errorDetails;
		}

		/// <summary>
		/// Writes a detailed error message to the buffer
		/// </summary>
		/// <param name="buffer">Instance of <see cref="StringBuilder"/></param>
		/// <param name="jsException">JS exception</param>
		/// <param name="omitMessage">Flag for whether to omit message</param>
		private static void WriteCommonErrorDetails(StringBuilder buffer, JsException jsException,
			bool omitMessage = false)
		{
			if (!omitMessage)
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_Message,
					jsException.Message);
			}
			if (!string.IsNullOrWhiteSpace(jsException.EngineName))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_EngineName,
					jsException.EngineName);
			}
			if (!string.IsNullOrWhiteSpace(jsException.EngineVersion))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_EngineVersion,
					jsException.EngineVersion);
			}
			if (!string.IsNullOrWhiteSpace(jsException.Category))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_Category,
					jsException.Category);
			}
			if (!string.IsNullOrWhiteSpace(jsException.Description))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_Description,
					jsException.Description);
			}
		}

		/// <summary>
		/// Writes a detailed error message to the buffer
		/// </summary>
		/// <param name="buffer">Instance of <see cref="StringBuilder"/></param>
		/// <param name="jsScriptException">JS script exception</param>
		private static void WriteScriptErrorDetails(StringBuilder buffer,
			JsScriptException jsScriptException)
		{
			if (!string.IsNullOrWhiteSpace(jsScriptException.Type))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_Type,
					jsScriptException.Type);
			}
			if (!string.IsNullOrWhiteSpace(jsScriptException.DocumentName))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_DocumentName,
					jsScriptException.DocumentName);
			}
			if (jsScriptException.LineNumber > 0)
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_LineNumber,
					jsScriptException.LineNumber.ToString(CultureInfo.InvariantCulture));
			}
			if (jsScriptException.ColumnNumber > 0)
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_ColumnNumber,
					jsScriptException.ColumnNumber.ToString(CultureInfo.InvariantCulture));
			}
			if (!string.IsNullOrWhiteSpace(jsScriptException.SourceFragment))
			{
				buffer.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_SourceFragment,
					jsScriptException.SourceFragment);
			}
		}

		/// <summary>
		/// Writes a detailed error message to the buffer
		/// </summary>
		/// <param name="buffer">Instance of <see cref="StringBuilder"/></param>
		/// <param name="jsRuntimeException">JS runtime exception</param>
		private static void WriteRuntimeErrorDetails(StringBuilder buffer,
			JsRuntimeException jsRuntimeException)
		{
			if (!string.IsNullOrWhiteSpace(jsRuntimeException.CallStack))
			{
				buffer.AppendFormatLine("{1}:{0}{2}", Environment.NewLine,
					Strings.ErrorDetails_CallStack,
					jsRuntimeException.CallStack);
			}
		}

		#endregion

		#region Exception wrapping

		public static JsEngineLoadException WrapEngineLoadException(Exception exception,
			string engineName, string engineVersion, bool quoteDescription = false)
		{
			string description = exception.Message;
			string message = GenerateEngineLoadErrorMessage(description, engineName, quoteDescription);

			var jsEngineLoadException = new JsEngineLoadException(message, engineName, engineVersion,
				exception)
			{
				Description = description
			};

			return jsEngineLoadException;
		}

		#endregion

		#region Misc

		/// <summary>
		/// Gets a fragment from the source code
		/// </summary>
		/// <param name="sourceCode">Source code</param>
		/// <param name="lineNumber">Line number</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="maxFragmentLength">Maximum length of the source fragment</param>
		public static string GetSourceFragmentFromCode(string sourceCode, int lineNumber, int columnNumber,
			int maxFragmentLength = 100)
		{
			if (lineNumber <= 0 || string.IsNullOrEmpty(sourceCode))
			{
				return string.Empty;
			}

			int lineStartPosition;
			int lineLength;
			GetPositionOfLine(sourceCode, lineNumber, out lineStartPosition, out lineLength);

			string fragment = GetSourceFragment(sourceCode, lineStartPosition, lineLength, columnNumber,
				maxFragmentLength);

			return fragment;
		}

		/// <summary>
		/// Gets a fragment from the source line
		/// </summary>
		/// <param name="sourceLine">Content of the source line</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="maxFragmentLength">Maximum length of the source fragment</param>
		public static string GetSourceFragmentFromLine(string sourceLine, int columnNumber,
			int maxFragmentLength = 100)
		{
			if (string.IsNullOrEmpty(sourceLine))
			{
				return string.Empty;
			}

			int lineStartPosition = 0;
			int lineLength = sourceLine.Length;
			string fragment = GetSourceFragment(sourceLine, lineStartPosition, lineLength,
				columnNumber, maxFragmentLength);

			return fragment;
		}

		private static string GetSourceFragment(string source, int position, int length,
			int columnNumber, int maxFragmentLength)
		{
			if (length == 0)
			{
				return string.Empty;
			}

			string fragment;

			if (length > maxFragmentLength)
			{
				const string ellipsisSymbol = "…";
				string startPart = string.Empty;
				string endPart = string.Empty;

				var leftOffset = (int)Math.Floor((double)maxFragmentLength / 2);
				int fragmentStartPosition = columnNumber - leftOffset - 1;
				if (fragmentStartPosition > position)
				{
					if (length - fragmentStartPosition < maxFragmentLength)
					{
						fragmentStartPosition = length - maxFragmentLength;
					}
				}
				else
				{
					fragmentStartPosition = position;
				}
				int fragmentLength = maxFragmentLength;

				if (fragmentStartPosition > position)
				{
					startPart = ellipsisSymbol;
				}
				if (fragmentStartPosition + fragmentLength < length)
				{
					endPart = ellipsisSymbol;
				}

				StringBuilder fragmentBuilder = StringBuilderPool.GetBuilder();
				if (startPart.Length > 0)
				{
					fragmentBuilder.Append(startPart);
				}
				fragmentBuilder.Append(source.Substring(fragmentStartPosition, fragmentLength));
				if (endPart.Length > 0)
				{
					fragmentBuilder.Append(endPart);
				}

				fragment = fragmentBuilder.ToString();
				StringBuilderPool.ReleaseBuilder(fragmentBuilder);
			}
			else
			{
				fragment = position > 0 || length < source.Length ?
					source.Substring(position, length) : source;
			}

			return fragment;
		}

		private static void GetPositionOfLine(string sourceCode, int lineNumber, out int position, out int length)
		{
			int currentLineNumber = 0;
			position = 0;
			length = 0;

			int sourceCodeLength = sourceCode.Length;
			if (sourceCodeLength > 0)
			{
				int currentPosition;
				int currentLength;
				int sourceCodeEndPosition = sourceCodeLength - 1;
				int lineBreakPosition = int.MinValue;
				int lineBreakLength = 0;

				do
				{
					currentLineNumber++;
					currentPosition = lineBreakPosition == int.MinValue ? 0 : lineBreakPosition + lineBreakLength;
					currentLength = sourceCodeEndPosition - currentPosition + 1;

					FindLineBreak(sourceCode, currentPosition, currentLength,
						out lineBreakPosition, out lineBreakLength);

					if (currentLineNumber == lineNumber)
					{
						if (lineBreakPosition != 0)
						{
							position = currentPosition;
							int endPosition = lineBreakPosition != -1 ?
								lineBreakPosition - 1 : sourceCodeEndPosition;
							length = endPosition - position + 1;
						}
						break;
					}
				}
				while (lineBreakPosition != -1 && lineBreakPosition <= sourceCodeEndPosition);
			}
		}

		/// <summary>
		/// Finds a line break
		/// </summary>
		/// <param name="sourceCode">Source code</param>
		/// <param name="startPosition">Position in the input string that defines the leftmost
		/// position to be searched</param>
		/// <param name="lineBreakPosition">Position of line break</param>
		/// <param name="lineBreakLength">Length of line break</param>
		private static void FindLineBreak(string sourceCode, int startPosition,
			out int lineBreakPosition, out int lineBreakLength)
		{
			int length = sourceCode.Length - startPosition;

			FindLineBreak(sourceCode, startPosition, length,
				out lineBreakPosition, out lineBreakLength);
		}

		/// <summary>
		/// Finds a line break
		/// </summary>
		/// <param name="sourceCode">Source code</param>
		/// <param name="startPosition">Position in the input string that defines the leftmost
		/// position to be searched</param>
		/// <param name="length">Number of characters in the substring to include in the search</param>
		/// <param name="lineBreakPosition">Position of line break</param>
		/// <param name="lineBreakLength">Length of line break</param>
		private static void FindLineBreak(string sourceCode, int startPosition, int length,
			out int lineBreakPosition, out int lineBreakLength)
		{
			Match lineBreakMatch = _lineBreakRegex.Match(sourceCode, startPosition, length);
			if (lineBreakMatch.Success)
			{
				lineBreakPosition = lineBreakMatch.Index;
				lineBreakLength = lineBreakMatch.Length;
			}
			else
			{
				lineBreakPosition = -1;
				lineBreakLength = 0;
			}
		}

		#endregion

		#region Obsolete methods

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsException">JS exception</param>
		/// <returns>Detailed error message</returns>
		[Obsolete("Use a `GenerateErrorDetails` method")]
		public static string Format(JsException jsException)
		{
			return GenerateErrorDetails(jsException);
		}

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsScriptException">JS script exception</param>
		/// <returns>Detailed error message</returns>
		[Obsolete("Use a `GenerateErrorDetails` method")]
		public static string Format(JsScriptException jsScriptException)
		{
			return GenerateErrorDetails(jsScriptException);
		}

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsRuntimeException">JS runtime exception</param>
		/// <returns>Detailed error message</returns>
		[Obsolete("Use a `GenerateErrorDetails` method")]
		public static string Format(JsRuntimeException jsRuntimeException)
		{
			return GenerateErrorDetails(jsRuntimeException);
		}

		#endregion
	}
}