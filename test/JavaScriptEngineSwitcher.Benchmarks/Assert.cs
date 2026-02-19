using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace JavaScriptEngineSwitcher.Benchmarks
{
	internal static class Assert
	{
		private static readonly char[] _lineBreakChars = new[] { '\r', '\n' };


		public static void Equal(string expected, string actual, bool ignoreLineBreaks = false)
		{
			if (!EqualInternal(expected, actual, ignoreLineBreaks))
			{
				var messageBuilder = new StringBuilder();
				messageBuilder.AppendLine("Assert.Equal() Failure");
				messageBuilder.AppendLine();
				messageBuilder.AppendLine($"Expected: {expected}");
				messageBuilder.Append($"Actual:   {actual}");

				string errorMessage = messageBuilder.ToString();

				throw new InvalidOperationException(errorMessage);
			}
		}

		private static bool EqualInternal(string a, string b, bool ignoreLineBreaks)
		{
			if (!ignoreLineBreaks)
			{
				return a == b;
			}

			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (a is null || b is null)
			{
				return false;
			}

			if (a.IndexOfAny(_lineBreakChars) == -1 && b.IndexOfAny(_lineBreakChars) == -1)
			{
				return a.Equals(b);
			}

			int aIndex = 0;
			int aLength = a.Length;
			int bIndex = 0;
			int bLength = b.Length;

			while (true)
			{
				if (aIndex >= aLength)
				{
					return bIndex >= bLength;
				}

				if (bIndex >= bLength)
				{
					return false;
				}

				char aChar = a[aIndex];
				char bChar = b[bIndex];

				if (aChar != bChar)
				{
					if (Array.IndexOf(_lineBreakChars, aChar) != -1 && Array.IndexOf(_lineBreakChars, bChar) != -1)
					{
						ProcessLineBreaks(a, aChar, ref aIndex, aLength);
						ProcessLineBreaks(b, bChar, ref bIndex, bLength);

						continue;
					}
					else
					{
						return false;
					}
				}

				aIndex++;
				bIndex++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ProcessLineBreaks(string value, char charValue, ref int charIndex, int charCount)
		{
			if (charValue == '\r')
			{
				int nextCharIndex = charIndex + 1;
				if (nextCharIndex < charCount && value[nextCharIndex] == '\n')
				{
					charIndex++;
				}
			}
			charIndex++;
		}
	}
}