#if NET45_OR_GREATER || NETSTANDARD || NET10_0_OR_GREATER
using System.Buffers;
#endif
using System.Text;
#if NET40

using PolyfillsForOldDotNet.System.Buffers;
#endif

namespace JavaScriptEngineSwitcher.ChakraCore.Helpers
{
	/// <summary>
	/// Encoding helpers
	/// </summary>
	internal static class EncodingHelpers
	{
		public static string UnicodeToAnsi(string value, out int byteCount)
		{
			if (string.IsNullOrEmpty(value))
			{
				byteCount = 0;
				return value;
			}

			string result;
			int valueLength = value.Length;
			Encoding utf8Encoding = Encoding.UTF8;
#if NETFRAMEWORK
			Encoding ansiEncoding = Encoding.Default;
#else
			Encoding ansiEncoding = Encoding.GetEncoding(0);
#endif

			var byteArrayPool = ArrayPool<byte>.Shared;
			int bufferLength = utf8Encoding.GetByteCount(value);
			byte[] buffer = byteArrayPool.Rent(bufferLength + 1);
			buffer[bufferLength] = 0;

			try
			{
#if NET45_OR_GREATER || NETSTANDARD || NET10_0_OR_GREATER
				result = ConvertStringInternal(utf8Encoding, ansiEncoding, value, valueLength, buffer, bufferLength);
#else
				utf8Encoding.GetBytes(value, 0, valueLength, buffer, 0);
				result = ansiEncoding.GetString(buffer, 0, bufferLength);
#endif
			}
			finally
			{
				byteArrayPool.Return(buffer);
			}

			byteCount = bufferLength;

			return result;
		}
#if NET45_OR_GREATER || NETSTANDARD || NET10_0_OR_GREATER

		private static unsafe string ConvertStringInternal(Encoding srcEncoding, Encoding dstEncoding, string s,
			int charCount, byte[] bytes, int byteCount)
		{
			fixed (char* pString = s)
			fixed (byte* pBytes = bytes)
			{
				srcEncoding.GetBytes(pString, charCount, pBytes, byteCount);
#if NET471 || NETSTANDARD
				string result = dstEncoding.GetString(pBytes, byteCount);

				return result;
			}
#else
			}

			int resultLength = dstEncoding.GetCharCount(bytes, 0, byteCount);
			var charArrayPool = ArrayPool<char>.Shared;
			char[] resultChars = charArrayPool.Rent(resultLength + 1);
			resultChars[resultLength] = '\0';

			string result;

			try
			{
				fixed (byte* pBytes = bytes)
				fixed (char* pResultChars = resultChars)
				{
					dstEncoding.GetChars(pBytes, byteCount, pResultChars, resultLength);
					result = new string(pResultChars, 0, resultLength);
				}
			}
			finally
			{
				charArrayPool.Return(resultChars);
			}

			return result;
#endif
		}
#endif
	}
}