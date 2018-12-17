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

			Encoding utf8Encoding = Encoding.UTF8;
			Encoding ansiEncoding = Encoding.GetEncoding(0);

			var byteArrayPool = ArrayPool<byte>.Shared;
			int bufferLength = utf8Encoding.GetByteCount(value);
			byte[] buffer = byteArrayPool.Rent(bufferLength);

			unsafe
			{
				fixed (char* pValue = value)
				fixed (byte* pBuffer = buffer)
				{
					utf8Encoding.GetBytes(pValue, value.Length, pBuffer, bufferLength);
				}
			}

			string result = ansiEncoding.GetString(buffer, 0, bufferLength);
			byteCount = ansiEncoding.GetByteCount(result);

			byteArrayPool.Return(buffer);

			return result;
		}
	}
}