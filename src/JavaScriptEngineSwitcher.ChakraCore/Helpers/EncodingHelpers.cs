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

			int valueLength = value.Length;
			Encoding utf8Encoding = Encoding.UTF8;
			Encoding ansiEncoding = Encoding.GetEncoding(0);

			var byteArrayPool = ArrayPool<byte>.Shared;
			int bufferLength = utf8Encoding.GetByteCount(value);
			byte[] buffer = byteArrayPool.Rent(bufferLength + 1);

			string result;
#if NET471 || NETSTANDARD || NETCOREAPP2_1

			unsafe
			{
				fixed (char* pValue = value)
				fixed (byte* pBuffer = buffer)
				{
					utf8Encoding.GetBytes(pValue, valueLength, pBuffer, bufferLength);
					pBuffer[bufferLength] = 0;

					result = ansiEncoding.GetString(pBuffer, bufferLength);
				}
			}

#else
			utf8Encoding.GetBytes(value, 0, valueLength, buffer, 0);
			buffer[bufferLength] = 0;

			result = ansiEncoding.GetString(buffer, 0, bufferLength);
#endif
			byteCount = ansiEncoding.GetByteCount(result);

			byteArrayPool.Return(buffer);

			return result;
		}
	}
}