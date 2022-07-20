using System;
using System.Runtime.InteropServices;

namespace JavaScriptEngineSwitcher.ChakraCore.Helpers
{
	/// <summary>
	/// Marshalling helpers
	/// </summary>
	internal static class MarshallingHelpers
    {
		public static IntPtr ByteArrayToPtr(byte[] bytes)
		{
			if (bytes == null)
			{
				return IntPtr.Zero;
			}

			int byteLength = bytes.Length;
			IntPtr ptr = Marshal.AllocHGlobal(byteLength + 1);
			Marshal.Copy(bytes, 0, ptr, byteLength);
			Marshal.WriteByte(ptr, byteLength, 0);

			return ptr;
		}

		public static byte[] PtrToByteArray(IntPtr ptr, int len)
		{
			if (ptr == IntPtr.Zero)
			{
				return null;
			}

			if (len == 0)
			{
				return new byte[0];
			}

			byte[] bytes = new byte[len];
			Marshal.Copy(ptr, bytes, 0, len);

			return bytes;
		}
	}
}