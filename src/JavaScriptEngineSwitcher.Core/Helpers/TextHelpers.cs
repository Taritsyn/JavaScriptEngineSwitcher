using System;
using System.Text;

using AdvancedStringBuilder;

using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.Core.Helpers
{
	/// <summary>
	/// Text helpers
	/// </summary>
	public static class TextHelpers
	{
		/// <summary>
		/// Array of characters used to find the next line break
		/// </summary>
		private static readonly char[] _nextLineBreakChars = EnvironmentShortcuts.NewLineChars;


		/// <summary>
		/// Gets a fragment from the source text
		/// </summary>
		/// <param name="sourceText">Source text</param>
		/// <param name="lineNumber">Line number</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="maxFragmentLength">Maximum length of the text fragment</param>
		public static string GetTextFragment(string sourceText, int lineNumber, int columnNumber,
			int maxFragmentLength = 100)
		{
			if (lineNumber <= 0 || string.IsNullOrEmpty(sourceText))
			{
				return string.Empty;
			}

			int lineStartPosition;
			int lineLength;
			GetPositionOfLine(sourceText, lineNumber, out lineStartPosition, out lineLength);

			string fragment = GetTextFragmentInternal(sourceText, lineStartPosition, lineLength, columnNumber,
				maxFragmentLength);

			return fragment;
		}

		/// <summary>
		/// Gets a fragment from the text line
		/// </summary>
		/// <param name="textLine">Content of the text line</param>
		/// <param name="columnNumber">Column number</param>
		/// <param name="maxFragmentLength">Maximum length of the text fragment</param>
		public static string GetTextFragmentFromLine(string textLine, int columnNumber,
			int maxFragmentLength = 100)
		{
			if (string.IsNullOrEmpty(textLine))
			{
				return string.Empty;
			}

			int lineStartPosition = 0;
			int lineLength = textLine.Length;
			string fragment = GetTextFragmentInternal(textLine, lineStartPosition, lineLength,
				columnNumber, maxFragmentLength);

			return fragment;
		}

		private static string GetTextFragmentInternal(string source, int position, int length,
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

				var stringBuilderPool = StringBuilderPool.Shared;
				StringBuilder fragmentBuilder = stringBuilderPool.Rent();
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
				stringBuilderPool.Return(fragmentBuilder);
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

					FindNextLineBreak(sourceCode, currentPosition, currentLength,
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
		/// Finds a next line break
		/// </summary>
		/// <param name="sourceText">Source text</param>
		/// <param name="startPosition">Position in the input string that defines the leftmost
		/// position to be searched</param>
		/// <param name="lineBreakPosition">Position of line break</param>
		/// <param name="lineBreakLength">Length of line break</param>
		private static void FindLineBreak(string sourceText, int startPosition,
			out int lineBreakPosition, out int lineBreakLength)
		{
			int length = sourceText.Length - startPosition;

			FindNextLineBreak(sourceText, startPosition, length,
				out lineBreakPosition, out lineBreakLength);
		}

		/// <summary>
		/// Finds a next line break
		/// </summary>
		/// <param name="sourceText">Source text</param>
		/// <param name="startPosition">Position in the input string that defines the leftmost
		/// position to be searched</param>
		/// <param name="length">Number of characters in the substring to include in the search</param>
		/// <param name="lineBreakPosition">Position of line break</param>
		/// <param name="lineBreakLength">Length of line break</param>
		public static void FindNextLineBreak(string sourceText, int startPosition, int length,
			out int lineBreakPosition, out int lineBreakLength)
		{
			lineBreakPosition = sourceText.IndexOfAny(_nextLineBreakChars, startPosition, length);
			if (lineBreakPosition != -1)
			{
				lineBreakLength = 1;
				char currentCharacter = sourceText[lineBreakPosition];

				if (currentCharacter == '\r')
				{
					int nextCharacterPosition = lineBreakPosition + 1;
					char nextCharacter;

					if (sourceText.TryGetChar(nextCharacterPosition, out nextCharacter)
						&& nextCharacter == '\n')
					{
						lineBreakLength = 2;
					}
				}
			}
			else
			{
				lineBreakLength = 0;
			}
		}
	}
}