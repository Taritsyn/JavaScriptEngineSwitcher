#if NET471 || NETSTANDARD
using System;
using System.Runtime.InteropServices;
#endif
using System.Text;

namespace JavaScriptEngineSwitcher.ChakraCore.Helpers
{
	/// <summary>
	/// Encoding helpers
	/// </summary>
	internal static class EncodingHelpers
	{
#if NET471 || NETSTANDARD
		public static unsafe string UnicodeToUtf8(string value, out int byteCount)
#else
		public static string UnicodeToUtf8(string value, out int byteCount)
#endif
		{
			if (string.IsNullOrEmpty(value))
			{
				byteCount = 0;
				return value;
			}

			string result;
#if NET471 || NETSTANDARD
			byteCount = Encoding.UTF8.GetByteCount(value);
			IntPtr bufferPtr = Marshal.AllocHGlobal(byteCount);

			try
			{
				fixed (char* pValue = value)
				{
					Encoding.UTF8.GetBytes(pValue, value.Length, (byte*)bufferPtr, byteCount);
				}

				result = Encoding.GetEncoding(0).GetString((byte*)bufferPtr, byteCount);
			}
			finally
			{
				Marshal.FreeHGlobal(bufferPtr);
			}
#else
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			byteCount = bytes.Length;
			result = Encoding.GetEncoding(0).GetString(bytes);
#endif

			return result;
		}
	}
}