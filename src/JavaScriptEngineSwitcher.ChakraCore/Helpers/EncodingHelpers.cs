#if NET45 || NET471 || NETSTANDARD || NETCOREAPP2_1
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
			Encoding ansiEncoding = Encoding.GetEncoding(0);

			var byteArrayPool = ArrayPool<byte>.Shared;
			int bufferLength = utf8Encoding.GetByteCount(value);
			byte[] buffer = byteArrayPool.Rent(bufferLength + 1);
			buffer[bufferLength] = 0;

			try
			{
#if NET471 || NETSTANDARD || NETCOREAPP2_1
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
#if NET471 || NETSTANDARD || NETCOREAPP2_1

		private static unsafe string ConvertStringInternal(Encoding srcEncoding, Encoding dstEncoding, string s,
			int charCount, byte[] bytes, int byteCount)
		{
			fixed (char* pString = s)
			fixed (byte* pBytes = bytes)
			{
				srcEncoding.GetBytes(pString, charCount, pBytes, byteCount);
				string result = dstEncoding.GetString(pBytes, byteCount);

				return result;
			}
		}
#endif
	}
}